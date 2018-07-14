/*
# Splitting Files into Blocks

The task of the block builder is to extract a block list from a source file. 
We use a simple regular expression to split markdown files into blocks, but with 
C# files we utilize a more sophisticated tool: the [Roslyn](https://github.com/dotnet/roslyn) 
compiler platform. Roslyn comprises a set of libraries and APIs that expose all 
facets of the C# compiler: lexer, parser, type checker, code generator, and so on. 
This is the same compiler that is used by Visual Studio and other official Microsoft 
tools, so it should be always complete and up-to-date.

We need only the parser to separate documentation and code, but we can take 
advantage of other parts of Roslyn too. For example, we can use the semantic analysis
to distinguish which identifiers represent types and highlight them in the output 
as Visual Studio does. We can also get the type information of any expression
and show it in a tooltip window.

The semantic information is useful only in conjunction with HTML output, though. 
In markdown we cannot make use of it, mostly because the code blocks cannot be 
augmented with styling information. Even if there was a way to add metadata to 
code blocks, any of the markdown-to-HTML converters would not probably use it. 
Consequently, we defer utilizing the semantic information to the 
[HtmlBlockBuilder](HtmlBlockBuild.html) class which is used in context of HTML 
output.

So, let's tackle the parsing problem first, and see how it is performed by 
the BlockBuilder class.
*/
namespace LiterateProgramming
{
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp;
	using Microsoft.CodeAnalysis.CSharp.Syntax;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using ExtensionCord;

	/*
	## Syntax Walker

	BlockBuilder inherits from [CSharpSyntaxWalker](http://www.coderesx.com/roslyn/html/B6833F66.htm),
	which implements the [visitor pattern](https://en.wikipedia.org/wiki/Visitor_pattern).
	It "walks" through an abstract syntax tree and calls a particular
	virtual method in the class based on the type of the node visited.
	Inheriting from the walker allows us to visit only specific types of 
	syntactic elements and ignore the ones we are not interested in.
	*/
	public class BlockBuilder : CSharpSyntaxWalker
	{
		/*
		We are extracting multi-line comment blocks, that is, comments
		starting with `/*`. To get the contents of a comment block, we need
		to strip the comment delimiters using a regular expression. The 
		following regex matches a comment body.
		*/
		private static Regex _commentBody = new Regex (@"/\*(.*?)\*/",
			RegexOptions.Singleline | RegexOptions.Compiled);
		/*
		The following regex matches a macro expansion line. A macro expansion 
		line begins with `<<` and ends with `>>`. Whitespace before and after 
		the double angle brackets is ignored.
		*/
		private static Regex _macroLine = new Regex (@"^\s*(<<.*>>)\s*$",
			RegexOptions.Multiline | RegexOptions.Compiled);
		/*
		While building the linked list of block, we need to maintain the latest 
		(current) block as well as the first block (head of the list).
		*/
		private BlockList _currentBlock;
		private BlockList _blocks;
		/*
		Macros require additional stored state. Since regions can be nested, we 
		need a stack for macros defined in a code file. The innermost region
		is stored at the top of the stack.
		*/
		private Stack<Macro> _macros = new Stack<Macro> ();
		/*
		When we encounter either `#region` or `#endregion` directive, we must
		start a new block. The boolean flag below triggers creating a new block.
		We also need to remember what the was the first region opened after the 
		macro was started or ended. 
		*/
		private bool _startNewBlock;
		private BlockList _newBlock;
		/*
		## Constructor
		We need the command line options to access the parameters controlling
		the parsing. The Options object is given as an argument to the constructor, 
		as usual.
		*/
		protected Options _options;

		public BlockBuilder (Options options)
			: base (SyntaxWalkerDepth.StructuredTrivia)
		{
			_options = options;
		}
		/*
		## Generating Block Lists from Code File
		The methods that generate block lists from code are defined below. 
		There are two variants: one takes a file and the other takes a Roslyn
		[Document](https://github.com/dotnet/roslyn/wiki/Roslyn-Overview#solutions-projects-documents). 
		The implementations are basically the same in both cases. They differ 
		only in how the AST is obtained.
		*/
		public virtual BlockList FromCodeFile (string codeFile)
		{
			var tree = CSharpSyntaxTree.ParseText (File.ReadAllText (codeFile));
			return CreateForTree (tree);
		}

		public virtual BlockList FromDocument (Document document)
		{
			var tree = document.GetSyntaxTreeAsync ().Result;
			return CreateForTree (tree);
		}
		/*
		## Generating Block List from Markdown File
		Parsing a markdown file is simple. We split the file using a regular 
		expression that recognizes macro expansion lines. When we encounter a
		macro line, we expand the blocks of the macro in place of the line.
		*/
		public virtual BlockList FromMdFile (string mdFile)
		{
			var parts = _macroLine.Split (File.ReadAllText (mdFile));
			foreach (var part in parts)
			{
				if (part.StartsWith ("<<") && part.EndsWith (">>"))
				{
					var macro = Macro.Get (part.Substring (2, part.Length - 4));
					foreach (var block in macro)
						AddBlock (new BlockList (block));
				}
				else
					AddBlock (new BlockList (part));
			}
			return _blocks;
		}
		/*
		### Creating New Blocks
		We can create and initialize a block in one go using the `AddBlock` 
		method below.
		*/
		public void AddBlock (BlockList block)
		{
			if (_blocks == null)
				_blocks = block;
			else
				_currentBlock.Next = block;
			_currentBlock = block;
		}
		/*
		Alternatively we can build a block incrementally by first creating it 
		and adding text later. Starting a new block closes the previous one 
		automatically, if such exists.
		*/
		private void StartNewBlock (BlockKind kind)
		{
			var result = new BlockList (kind, _options.Format);
			if (_blocks == null)
				_blocks = result;
			else
			{
				_currentBlock.Next = result;
				_currentBlock.Close (_options.Format);
			}
			_currentBlock = result;
		}
		/*
		The helper function below checks if the kind of the current block
		matches the kind of the block requested. If it does, the current
		block is returned; if not, a new block is started. 
		
		A new block is opened also in case the `_startNewBlock` flag is set. 
		When this occurs, the flag is reset back to false and a reference 
		to the opened block is stored in the `_newBlock` field.
		*/
		private BlockList CurrentBlock (BlockKind kind)
		{
			if (_blocks == null || _currentBlock.Kind != kind || _startNewBlock)
				StartNewBlock (kind);
			if (_startNewBlock)
			{
				_startNewBlock = false;
				_newBlock = _currentBlock;
			}
			return _currentBlock;
		}
		/*
		Execute methods use the method below to initialize a new block list, 
		and to run the syntax walker by visiting the root of the tree. After
		all the blocks are created, the last block is closed, and the head of
		the list is returned.
		*/
		private BlockList CreateForTree (SyntaxTree tree)
		{
			_blocks = null;
			Visit (tree.GetRoot ());
			_currentBlock.Close (_options.Format);
			return _blocks;
		}
		/*
		### Adding Documentation
		Appending a piece of markdown into a block would be very straightforward 
		if we wouldn't need to deal with whitespace. Usually the `--trim` option
		is used to remove the leading whitespace from the comments. This complicates
		the code a bit.
		*/
		protected void AppendMarkdown (string text)
		{
			if (_options.Trim)
			{
				/*
				Trimming starts with splitting the comment block into lines.
				We maintain the variable `firstNonWS` which contains the index
				of the first non-whitespace character in the comment block.
				Remember that we already removed the comment delimiters from
				the text block before calling this function.
				*/
				var firstNonWS = -1;
				foreach (var line in Regex.Split (text, "\r\n|\r|\n"))
				{
					if (firstNonWS < 0)
					{
						var i = line.TakeWhile (char.IsWhiteSpace).Count ();
						if (i < line.Length)
							firstNonWS = i;
					}
					/*
					If the index of the first non-whitespace character is found,
					we extract a substring of the line starting from the index. 
					If not, we add the whole line into the block.
					*/
					CurrentBlock (BlockKind.Markdown)._builder.AppendLine (
						firstNonWS < 0 || line.Length < firstNonWS ?
							line :
							line.Substring (firstNonWS));
				}
			}
			/*
			If the trimming option is not used then we add the whole comment 
			text as-is into the markdown block.
			*/
			else
				CurrentBlock (BlockKind.Markdown)._builder.Append (text);
		}
		/*
		### Adding Code
		`AppendCode` adds text to a code block. Unlike comment text, code is 
		always added one token at a time. So, no multi-line strings are passed 
		to this function.
		*/
		protected void AppendCode (string text)
		{
			CurrentBlock (BlockKind.Code)._builder.Append (text);
		}
		/*
		## Parsing Comments
		Removing comment delimiters is easy. The regular expression defined at the
		start of the class does the work for us.
		*/
		private string StripCommentDelimiters (string comment) =>
			_commentBody.Match (comment).Groups[1].Value;
		/*
		## Parsing Tokens
		Now that we have all the helper functions defined, we can implement the
		visitor for tokens. The visitor overrides the inherited method and outputs 
		text to the current block based on the tokens.

		Roslyn parser does not add comments to AST as syntactic nodes. Instead 
		they are included as trivia which is additional information attached to 
		tokens. Trivia can be present before and/or after a token, so we have both
		leading and trailing trivia. In addition to comments, trivia can represent 
		also whitespace or preprocessor directives.
		*/
		public override void VisitToken (SyntaxToken token)
		{
			/*
			First we process the leading trivia. 
			*/
			ProcessTrivia (token.LeadingTrivia);
			/*
			Then we output the token itself as code. We skip the end-of-file
			token in order not output an empty code block at the end page.
			*/
			if (token.Kind () != SyntaxKind.EndOfFileToken)
				OutputToken (token);
			/*
			And then we output the trailing trivia.
			*/
			ProcessTrivia (token.TrailingTrivia);
			/*
			We must call the inherited method to visit the `#region` directives
			which reside under tokens in AST.
			*/
			base.VisitToken (token);
		}
		/*
		The method below processes a list of trivia. 
		*/
		private void ProcessTrivia (SyntaxTriviaList trivia)
		{
			for (var i = 0; i < trivia.Count; i++)
			{
				switch (trivia[i].Kind ())
				{
					/*
					If the trivia contains multi-line comment, we will output it as 
					documentation. Before doing that, we need to strip the comment 
					delimiters from it.
					*/
					case SyntaxKind.MultiLineCommentTrivia:
						AppendMarkdown (StripCommentDelimiters (trivia.ToString ()));
						break;
					/*
					Whitespace and eol markers are skipped if they preceed or follow
					trivia that we don't want to output. The `IsSkippedTrivia` function
					decides whether a piece of trivia is outputted or not.
					*/
					case SyntaxKind.WhitespaceTrivia:
						if (i == trivia.Count - 1 ||
							!IsSkippedTrivia (trivia[i + 1].Kind ()))
							OutputTrivia (trivia[i]);
						break;
					case SyntaxKind.EndOfLineTrivia:
						if (i == 0 ||
							!IsSkippedTrivia (trivia[i - 1].Kind ()))
							OutputTrivia (trivia[i]);
						break;
					/*
					When we encounter either `#region` or `#endregion` directive, we
					start a new block. The `VisitRegionDirectiveTrivia` visitor method
					actually creates the new macro. Note that we omit the directives 
					in the output altogether.
					*/
					case SyntaxKind.RegionDirectiveTrivia:
					case SyntaxKind.EndRegionDirectiveTrivia:
						_startNewBlock = true;
						break;
					/*
					In case of any other trivia we revert to the generic method which 
					outputs it verbatim as code. 
					*/
					default:
						OutputTrivia (trivia[i]);
						break;
				}
			}
		}
		/*
		As explained above, this helper function checks whether we want to 
		output a certain kind of trivia (as code) or not.
		*/
		private bool IsSkippedTrivia (SyntaxKind kind)
		{
			return kind.In (
				SyntaxKind.RegionDirectiveTrivia,
				SyntaxKind.EndRegionDirectiveTrivia,
				SyntaxKind.MultiLineCommentTrivia);
		}
		/*
		The following two methods are virtual so that subclasses can alter their 
		behavior. The first one outputs a token, and the second one a piece of 
		trivia. Both methods just send the text verbatim to the current code block.
		*/
		protected virtual void OutputToken (SyntaxToken token)
		{
			AppendCode (token.ToString ());
		}

		protected virtual void OutputTrivia (SyntaxTrivia trivia)
		{
			AppendCode (trivia.ToString ());
		}
		/*
		## Creating Macros
		The visitor methods for regions handle creating new macros. When a region
		is opened, we read its name and push a new macro with that name to the stack. 
		When the visitor methods are called, we have already added the new block which
		starts the macro, so we can take its reference from the `_newBlock` field.
		*/
		public override void VisitRegionDirectiveTrivia (
			RegionDirectiveTriviaSyntax node)
		{
			_macros.Push (Macro.Add (
				node.EndOfDirectiveToken.LeadingTrivia.First ().ToString (),
				_newBlock, null));
		}
		/*
		When a region is closed, we close the macro as well by popping it off the stack. 
		We also record the first block which does not belong to the macro (the end marker).
		*/
		public override void VisitEndRegionDirectiveTrivia (
			EndRegionDirectiveTriviaSyntax node)
		{
			_macros.Pop ().End = _newBlock;
		}
	}
}