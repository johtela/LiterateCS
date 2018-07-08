/*
# Markdown Weaver

Now we can look at how markdown output is generated. This is the easier of the 
two use cases. We only need to split source files into comments and code and 
_weave_ the generated blocks to the output files. The splitting part is done 
in the [BlockBuilder](BlockBuilder.html) class. The markdown weaver takes the 
generated block lists and writes them verbatim to the output files.
*/
namespace LiterateProgramming
{
	using Microsoft.CodeAnalysis;
	using System.IO;

	public class MdWeaver : Weaver
	{
		public MdWeaver (Options options)
			: base (options)
		{ }
		/*
		## Generating Output
		
		The first method to be implemented is to generate documentation	from 
		an input directory. We use the inherited function to enumerate input
		files of a specific type. We transform the files by calling the 
		appropriate `Weave*` function.
		*/
		protected override void GenerateFromFiles ()
		{
			foreach (var file in SourceFiles ())
				WeaveFromCode (file);
			foreach (var file in MarkdownFiles ())
				WeaveFromMarkdown (file);
		}
		/*
		The second options is to use a solution as an input. Again we iterate 
		markdown and C# source files separately and call the correct method to
		transform them to output.
		*/
		protected override void GenerateFromSolution ()
		{ 
			var solutionFile = MSBuildHelpers.OpenSolution (_options.Solution);
			foreach (var doc in CSharpDocumentsInSolution (solutionFile))
				WeaveFromCSharpDocument (doc.Item1, doc.Item2);
			foreach (var inputFile in MarkdownFilesInSolution (solutionFile))
				WeaveFromMarkdown (inputFile);
		}
		/*
		### Weaving the Output
		The rest of the code in this class consist of helper functions which 
		split the input files to blocks and compile the output from them.
		Since the blocks already contain valid markdown, no further processing 
		is needed to transform them.
		*/
		private void WeaveFromMarkdown (SplitPath mdFile)
		{
			SplitPath outputFile = CreateOutputPath (mdFile, _options.MarkdownExt);
			OutputBlocks (BlockListFromMarkdown (!mdFile), !outputFile);
		}

		private void WeaveFromCode (SplitPath codeFile)
		{
			SplitPath outputFile = CreateOutputPath (codeFile, _options.MarkdownExt);
			OutputBlocks (BlockListFromCode (!codeFile), !outputFile);
		}

		private void WeaveFromCSharpDocument (SplitPath codeFile, 
			Document document)
		{
			SplitPath outputFile = CreateOutputPath (codeFile, _options.MarkdownExt);
			OutputBlocks (BlockListFromDocument (document), !outputFile);
		}

		private void OutputBlocks (BlockList blocks, string outputFile)
		{
			using (var writer = File.CreateText (outputFile))
				foreach (var block in blocks)
					writer.Write (block.Contents);
		}
	}
}