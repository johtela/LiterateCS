/*
# Source Blocks
In order to extract the documentation from the code, LiterateCS needs to split a source file
into blocks which represent either text or code. Blocks capture parts of a source file and
make it easier to process them differently depending on their kind.
*/
namespace LiterateCS
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using ExtensionCord;
	/*
	## Block Kinds
	A block can capture either documentation written in markdown or C# code. The enumeration 
	below indicates which one we are dealing with.
	*/
	public enum BlockKind { Markdown, Code }
	/*
	## Linked List of Blocks
	The block data structure itself is a simple singly-linked list. It implements 
	`IEnumerable<T>` to make traversing the structure easier.
	*/
	public class BlockList : IEnumerable<BlockList>
	{
		/*
		The properties below describe the data associated with a block. `Kind` tells the 
		whether it is a code or text block. `Contents` is the part of the source file
		that was extracted. Comment delimiters are excluded from the text, so that 
		contents is valid markdown. `Next` points to the following block. Obviously
		code blocks are in the same order as they appear in the source file.
		*/
		public BlockKind Kind { get; private set; }
		public string Contents { get; private set; }
		public BlockList Next { get; internal set; }
		/*
		The following constants describe how the code blocks are decorated in the outputted
		markdown or HTML. In the case of markdown output, the code blocks are surrounded
		by standard triple backticks followed by the language designator. In HTML output the 
		header and footer contain tags which have the required CSS classes in order for the 
		syntax highlighting to work.
		*/
		private const string _mdCodeHeader = @"``` csharp";
		private const string _mdCodeFooter = @"```";
		private const string _htmlCodeHeader = @"<pre class=""csharp""><code class=""csharp"">";
		private const string _htmlCodeFooter = @"</code></pre>";
		/*
		The string builder is used to construct contents of a block. It is discarded after
		its content is extracted.
		*/
		internal StringBuilder _builder;
		/*
		### Creating a Block
		Blocks are constructed dynamically as the source file is parsed. When creating a new
		block only the kind of the block and the output format needs to be specified. Based
		on those the constructor initializes a new block and inserts a correct header to it.
		
		In markdown output whitespace is a bit more important than in HTML. That is why 
		we sometimes need to add additional line breaks to the output.
		*/
		public BlockList (BlockKind kind, OutputFormat format)
		{
			Kind = kind;
			_builder = new StringBuilder ();
		}
		/*
		Create a new markdown block.
		*/
		public BlockList (string contents)
		{
			Kind = BlockKind.Markdown;
			Contents = contents;
		}
		/*
		Cloning a single block.
		*/
		public BlockList (BlockList block)
		{
			Kind = block.Kind;
			Contents = block.Contents;
		}
		/*
		### Closing a Block
		After a block is filled with its content it can be closed. Closing involves
		adding the correct header and footer to the output. Code blocks are trimmed
		before these are added. If they do not contain any text after trimming, then we 
		don't add the header and footer at all. After the `StringBuilder` object has 
		converted the contents to string, it can be disposed by assigning `null` to the 
		field.
		*/
		public void Close (OutputFormat format)
		{
			if (Kind == BlockKind.Code)
			{
				TrimStart ();
				TrimEnd (); 
				if (_builder.Length > 0)
				{
					_builder.Insert (0, format == OutputFormat.html ?
						_htmlCodeHeader :
						_mdCodeHeader + Environment.NewLine);
					_builder.Append (Environment.NewLine);
					_builder.AppendLine (format == OutputFormat.html ?
						 _htmlCodeFooter :
						 _mdCodeFooter);
				}
			}
			Contents = _builder.ToString ();
			_builder = null;
		}
		/*
		Code blocks are trimmed to get rid of extra white space at the beginning and 
		end of them. This makes the verbatim output more compact and readable. The 
		following methods trim all whitespace from the beginning and from the end 
		of the block. Note that we don't remove the whitespace before the first 
		non-empty line, so indentation is preserved.
		*/
		private void TrimStart ()
		{
			while (RemoveFirstEmptyLine ());
		}

		private void TrimEnd ()
		{
			var last = _builder.Length - 1;
			var i = last;
			while (i >= 0 && char.IsWhiteSpace (_builder[i]))
				i--;
			if (i < last)
				_builder.Remove (i + 1, last - i);
		}
		/*
		The method below removes the first line of a block, if it contains just 
		whitespace. It handles both Windows and Unix style line delimiters. The
		method returns true, if it removed a line, and false if it did not.
		*/
		private bool RemoveFirstEmptyLine ()
		{
			var i = 0;
			while (i < _builder.Length && char.IsWhiteSpace (_builder[i]) &&
				!_builder[i].In ('\r', '\n'))
				i++;
			if (i == _builder.Length || !_builder[i].In ('\r', '\n'))
				return false;
			_builder.Remove (0, i + 1);
			return true;
		}
		/* 
		### IEnumerable Implementation
		Iterator is used to implement `IEnumerable<BlockList>` interface.
		*/
		public IEnumerator<BlockList> GetEnumerator ()
		{
			for (var block = this; block != null; block = block.Next)
				yield return block;
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}
}
