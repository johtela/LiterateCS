/*
# Enriching Code Blocks in HTML Output

When a solution file is used as an input and we are generating HTML output, we 
can fully utilize the semantic information provided by Roslyn. We do this by 
specializing the [BlockBuilder](BlockBuilder.html) class and redefining the way 
code blocks are produced.

The basic idea is to add style classes and HTML identifiers to selected tokens 
in the code. CSS style sheets defined in the theme control how these attributes 
will be actually used. For example, a specific kind of tokens can be colored 
differently to distinguish them from other tokens. By adding an ID attribute 
to a token, it is possible to use hyperlinks to implement a feature that allows 
jumping to a symbol definition. Like in Visual Studio, a tooltip showing the type 
of a symbol pops up when you hover over it. This feature relies on the 
[Bootstrap](http://getbootstrap.com/) framework to provide the tooltip control.

There are probably things we could do to utilize the semantic information even 
further. But for documentation purposes, we have the most useful information 
now available.

## Dependencies

Adding references to Microsoft.CodeAnalysis namespaces bring in the Roslyn 
libraries. We also need the System.Web assembly to access the function which 
escapes invalid characters in HTML.
*/
namespace LiterateCS
{
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp;
	using Microsoft.CodeAnalysis.CSharp.Syntax;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Web;
	using ExtensionCord;

	public class HtmlBlockBuilder : BlockBuilder
	{
		/*
		## Stored State Variables

		We need access to the SemanticModel object that is created when the project
		is compiled.
		*/
		private Document _document;
		private SemanticModel _model;
		/*
		The hash table below stores the IDs we have assigned thus far. In order not
		to define the same ID in multiple places, we consult the hash table before
		assigning a new ID. We initialize the hash table inside the constructor.
		*/
		private HashSet<string> _ids;

		public HtmlBlockBuilder (Options options)
			: base (options)
		{
			_ids = new HashSet<string> ();
		}
		/*
		To initialize the HTML block builder we override only the second variant 
		of the Execute method. We need a Document object provided by MSBuildWorkspace 
		to get hold of the semantic information. When working with a plain file, this 
		object is obviously missing. Because the same instance of the class is used to 
		process all the documents we also clear the ID table. Finally, we call the
		inherited method to initialize the fields in the base class.
		*/
		public override BlockList FromDocument (Document document)
		{
			_document = document;
			_model = document.GetSemanticModelAsync ().Result;
			_ids.Clear ();
			return base.FromDocument (document);
		}
		/*
		## Outputting Tokens and Trivia

		The other two methods we need to override are OutputToken and OutputTrivia.
		These will be called for each processed token and piece of trivia. Let's 
		first look at how we handle tokens.
		*/
		protected override void OutputToken (SyntaxToken token)
		{
			string href = null;
			/*
			First we get the token's string representation and its syntax kind.
			*/
			var code = token.ToString ();
			var kind = token.Kind ();
			/*
			Next we get the style information for the syntax kind from the 
			StyleClassForSyntax function. This function effectively implements 
			the syntax highlighting feature.
			*/
			var style = StyleClassForSyntax (kind);
			/*
			Then we move up in the AST to get the SyntaxNode under which the token
			resides. We retrieve the syntax kind for the node as well.
			*/
			var node = token.Parent;
			var nodeKind = node.Kind ();
			/*
			Now we can use the semantic model to get the symbol information for
			the syntax node. There are two helper functions we call that try to 
			get either ISymbol or ITypeSymbol interface for the node.
			*/
			var symbol = GetSymbol (node);
			var typeSymbol = GetTypeSymbol (node);
			/*
			If the token kind is identifier, we have a couple of special cases 
			to handle.
			*/
			if (kind == SyntaxKind.IdentifierToken)
			{
				/*
				The first case is the `var` token, which is not classified as a
				keyword but as an identifier in the AST. We treat it as a keyword
				in the syntax highlighting.
				*/
				if (code == "var")
					style = "keyword";
				/*
				The next special case to handle is to determine whether the 
				symbol corresponds to a type name, which should be highlighted
				in the code. Visual Studio calls this semantic highlighting
				because the decision to highlight the token is not based on the 
				token kind but rather on the symbol that the token represents.
				*/
				else if (IsIdentifier (nodeKind))
				{
					if (IsHighightedTypeName (symbol, node))
						style = "typename";
					/*
					While figuring out the type of an identifier type we can 
					also check whether it is clickable. That is, whether we
					add a hyperlink to it.
					*/
					if (IsReferenceableSymbol (symbol))
						href = GetHrefForSymbol (symbol);
				}
				else if (IsHighlightedDeclaration (nodeKind))
					style = "typename";
			}
			/*
			To enable jumping to a symbol definition, we get the Id for the symbol.
			*/
			var id = GetSyntaxNodeId (node, symbol);
			/*
			Finally, we get the type information that can be shown in a tooltip. 
			After that we collect all the information gathered and call 
			OutputDecoratedCode to spit out the token with all of its associated 
			attributes.
			*/
			var tooltip = GetTooltip (node, symbol);
			OutputDecoratedCode (code, style, id, href, tooltip);
		}
		/*
		Outputting trivia is a considerably simpler endeavor. We just convert the
		trivia to string and output it with possible style class for syntax
		highlighting. Typically trivia represent comments and whitespace, but
		preprocessor directives are inside trivia too. We like to have a different
		style for those.
		*/
		protected override void OutputTrivia (SyntaxTrivia trivia)
		{
			var code = trivia.ToString ();
			OutputDecoratedCode (code, StyleClassForSyntax (trivia.Kind ()),
				null, null, null);
		}
		/*
		## Helper Functions

		The rest of the methods are private helper functions that are used by the
		two methods defined above. The first two helper functions return symbol
		information for a node. They try to get ISymbol or ITypeSymbold interfaces
		using various methods provided by Roslyn. If they fail, null is returned.
		*/
		private ISymbol GetSymbol (SyntaxNode node) =>
			_model == null ? null :
				_model.GetSymbolInfo (node).Symbol ??
					_model.GetDeclaredSymbol (node) ??
					_model.GetPreprocessingSymbolInfo (node).Symbol ??
					_model.GetSymbolInfo (node).CandidateSymbols.FirstOrDefault ();

		private ITypeSymbol GetTypeSymbol (SyntaxNode node) =>
			_model?.GetTypeInfo (node).Type;
		/*
		The function below is used by the semantic highlighting feature. It 
		determines whether a symbol represents a type that should be highlighted.
		As in Visual Studio we highlight class, struct, or delegate names, as
		well as attribute names, which are actually methods in the symbol table.
		*/
		private bool IsHighightedTypeName (ISymbol symbol, SyntaxNode node) =>
			symbol != null &&
			(symbol.Kind == SymbolKind.NamedType ||
			 symbol.Kind == SymbolKind.TypeParameter ||
			 (node.Parent.Kind () == SyntaxKind.Attribute &&
				symbol.Kind == SymbolKind.Method));
		/*
		To enable jumping to a symbol definition we need to provide an ID for a 
		token. This is done just by taking the symbol name. If the same symbol name
		is already defined elsewhere, we return null. The check is needed because 
		Roslyn usually returns the same symbol information for several consecutive 
		tokens (since they reside under same syntax node). We effectively anchor the 
		first token with the given symbol name.
		*/
		private string GetSyntaxNodeId (SyntaxNode node, ISymbol symbol)
		{
			if (symbol != null &&
				(node is MemberDeclarationSyntax || node is VariableDeclaratorSyntax))
			{
				var symbolStr = GetSymbolId (symbol);
				if (!_ids.Contains (symbolStr))
				{
					_ids.Add (symbolStr);
					return symbolStr;
				}
			}
			return null;
		}
		/*
		The mirror operation of defining an ID is making a token clickable by 
		surrounding it with an `<a>` tag. The target of the link is in the `href`
		attribute, which is determined by the function below. The link consists
		of the relative file path and the ID inside the file. The file path is
		always used, even if the link target is inside the same file.
		*/
		private string GetHrefForSymbol (ISymbol symbol)
		{
			var sref = symbol.DeclaringSyntaxReferences.First ();
			var reffile = SplitPath.Split (_options.InputPath.BasePath,
				sref.SyntaxTree.FilePath);
			var inputfile = SplitPath.Split (_options.InputPath.BasePath,
				_document.FilePath);
			return Path.Combine (inputfile.RelativePathToRoot,
				reffile.ChangeExtension ("html").FilePath).Replace ('\\', '/') +
				"#" + GetSymbolId (symbol);
		}
		/*
		To generate the link Id from the symbol we use `ToDisplayString` method,
		that let's you specify in great detail which parts of the symbol included 
		in the result. We just need to make sure that the Id is unique, so we 
		pick the parts that distinguish the symbol from the others. We always 
		use the original definition of the symbol. This means that if a symbol 
		is an instantiation of generic type or method, we refer to its definition 
		instead of the specific instance.
		*/
		private static readonly SymbolDisplayFormat _symbolDisplayFormat =
			new SymbolDisplayFormat (
				SymbolDisplayGlobalNamespaceStyle.Omitted,
				SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
				SymbolDisplayGenericsOptions.IncludeTypeParameters,
				SymbolDisplayMemberOptions.IncludeContainingType | 
					SymbolDisplayMemberOptions.IncludeParameters,
				SymbolDisplayDelegateStyle.NameAndSignature,
				SymbolDisplayExtensionMethodStyle.StaticMethod,
				SymbolDisplayParameterOptions.IncludeType,
				SymbolDisplayPropertyStyle.NameOnly,
				SymbolDisplayLocalOptions.None,
				SymbolDisplayKindOptions.None,
				SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

		private string GetSymbolId (ISymbol symbol)
		{
			return symbol.OriginalDefinition.ToDisplayString (_symbolDisplayFormat);
		}
		/*
		Tooltip is a handy way for showing additional information in code blocks 
		without cluttering them excessively. Again, we mimic Visual Studio and 
		use a tooltip to show the type of a symbol.
		*/
		private string GetTooltip (SyntaxNode node, ISymbol symbol)
		{
			var typeSymbol = GetTypeSymbol (node);
			var title = typeSymbol != null ? typeSymbol.ToString () :
				symbol != null && symbol.Kind == SymbolKind.Method ?
					symbol.ToString () :
					null;
			return title == null ? null :
				string.Format (" data-toggle=\"tooltip\" title=\"{0}\"", title);
		}
		/*
		Before we can attach a link to a token, we need to check if we have it
		defined somewhere in the solution. External references are not navigable.
		*/
		private bool IsReferenceableSymbol (ISymbol symbol) =>
			symbol != null &&
			!symbol.Kind.In (SymbolKind.Local, SymbolKind.Parameter, 
				SymbolKind.Namespace, SymbolKind.RangeVariable) &&
			!symbol.DeclaringSyntaxReferences.IsEmpty;
		/*
		The following pair of functions helps determining whether a syntax
		kind represents an identifier or a type definition that should be 
		highlighted.
		*/
		private bool IsIdentifier (SyntaxKind kind) =>
			kind.In (SyntaxKind.IdentifierName, SyntaxKind.GenericName, 
				SyntaxKind.TypeParameter);

		private bool IsHighlightedDeclaration (SyntaxKind kind) =>
			kind.In (SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration,
				SyntaxKind.EnumDeclaration, SyntaxKind.EventDeclaration,
				SyntaxKind.DelegateDeclaration);
		/*
		Syntax highlighting is based primarily just on a kind of a token. There
		are literally hundreds of different token kinds. The main job of the 
		function below is to check whether the specified syntax kind lies within 
		a recognized range. If it does not match any of them, null is returned.
		*/
		private string StyleClassForSyntax (SyntaxKind kind)
		{
			if (kind >= SyntaxKind.TildeToken && kind < SyntaxKind.BoolKeyword)
				return "punctuation";
			if (kind >= SyntaxKind.BoolKeyword &&
				kind < SyntaxKind.InterpolatedStringStartToken)
				return "keyword";
			if (kind >= SyntaxKind.SingleLineCommentTrivia &&
				kind <= SyntaxKind.MultiLineCommentTrivia)
				return "comment";
			if (kind >= SyntaxKind.PreprocessingMessageTrivia &&
				kind < SyntaxKind.XmlElement)
				return "preprocessor";
			if (kind == SyntaxKind.StringLiteralToken)
				return "string";
			if (kind == SyntaxKind.NumericLiteralToken)
				return "number";
			return null;
		}
		/*
		At last we can actually output the token or trivia to the output file.
		This is done using the inherited AppendCode function. But before we
		call it, we combine all the attributes and extra information we collected
		for the token into HTML tags and attributes. 
		*/
		private void OutputDecoratedCode (string code, string style, string id,
			string href, string attrs)
		{
			/* 
			Now we also need the HttpUtility class to encode the characters reserved
			by HTML into escape codes.
			*/
			var escaped = HttpUtility.HtmlEncode (code);
			/*
			Surround with `<a>` tag, if href is given.
			*/
			var output = href == null ?
				escaped :
				string.Format ("<a href=\"{0}\">{1}</a>", href, escaped);
			/*
			We only add a `<span>` tag if at least one of the provided parameters 
			is not null.
			*/
			if (style == null && id == null && attrs == null)
				AppendCode (output);
			else
			{
				/*
				Surround the code with a `<span>` tag, and with all the given
				attributes.
				*/
				var idText = id == null ? "" :
					string.Format (" id=\"{0}\"", id);
				var styleText = style == null ? "" :
					string.Format (" class=\"{0}\"", style);
				AppendCode (string.Format ("<span{0}{1}{2}>{3}</span>", styleText,
					idText, attrs, output));
			}
		}
	}
}
