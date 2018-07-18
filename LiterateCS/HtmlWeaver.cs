/*
# HTML Weaver

On high level, weaving source files into web pages resembles the markdown 
generation process. The biggest difference is that HTML weaver has to manage 
the table of contents when working with input files. Also, two special classes 
are used to produce the actual HTML output: 

* [HtmlBlockBuilder](HtmlBlockBuilder.html) which decorates the code blocks
  with styling information.
* [HtmlGenerator](HtmlGenerator.html) which converts the markdown to HTML, and loads 
  the theme assembly, which in turn renders the HTML into a fully functional website.
  
But first, let's focus on how the source files are processed when HTML output is
selected.
*/
namespace LiterateCS
{
	using Microsoft.CodeAnalysis;
	using System.IO;

	public class HtmlWeaver : Weaver
	{
		/*
		## Initialization
		Constructor loads the TOC manager and creates the HTML generator. After that,
		it loads the default settings.
		*/
		private HtmlGenerator _generateHtml;
		protected TocManager _tocManager;

		public HtmlWeaver (Options options)
			: base (options) 
		{
			LoadToc ();
			_generateHtml = new HtmlGenerator (_options, _tocManager.Toc);
			LoadDefaultFrontMatter ();
		}
		/*
		A customized version of BlockBuilder is employed by overriding the virtual
		method that creates it.
		*/
		protected override BlockBuilder CreateBlockBuilder ()
		{
			return new HtmlBlockBuilder (_options);
		}
		/*
		Generation methods process the input files and update the TOC while 
		looping through them. Finally they copy the auxiliary files (CSS, 
		Javascript, fonts, etc.) that are required by the generated HTML 
		files. Auxiliary files reside in a theme assembly, and themes are
		responsible for copying them to the output folder. The weaver just 
		invokes the copy method through the HTML generator object.
		*/
		protected override void GenerateFromFiles ()
		{
			foreach (var file in SourceFiles ())
			{
				WeaveFromCodeFile (file);
				AddToToc (file);
			}
			foreach (var file in MarkdownFiles ())
			{
				WeaveFromMarkdown (file);
				AddToToc (file);
			}
			ConsoleOut ("Copying auxiliary HTML files...");
			_generateHtml.CopyAuxiliaryFiles ();
			_tocManager.Save ();
		}

		protected override void GenerateFromSolution ()
		{
			foreach (var doc in CSharpDocumentsInSolution ())
			{
				WeaveFromCSharpDocument (doc.Item1, doc.Item2);
				AddToToc (doc.Item1);
			}
			foreach (var inputFile in MarkdownFiles ())
			{
				WeaveFromMarkdown (inputFile);
				AddToToc (inputFile);
			}
			ConsoleOut ("Copying auxiliary HTML files...");
			_generateHtml.CopyAuxiliaryFiles ();
			_tocManager.Save ();
		}
		/*
		The rest of the methods are helper functions that process a single file 
		at a time; first they split it to blocks, and then call HTMLGenerator 
		to convert it to a HTML page.
		*/
		private void WeaveFromCodeFile (SplitPath codeFile)
		{
			_generateHtml.Execute (BlockListFromCode (!codeFile), codeFile, 
				CreateOutputPath (codeFile, ".html"));
		}

		private void WeaveFromCSharpDocument (SplitPath codeFile, Document document)
		{
			_generateHtml.Execute (BlockListFromDocument (document), codeFile, 
				CreateOutputPath (codeFile, ".html"));
		}

		private void WeaveFromMarkdown (SplitPath mdFile)
		{
			_generateHtml.Execute (BlockListFromMarkdown (!mdFile), mdFile,
				CreateOutputPath (mdFile, ".html"));
		}
		/*
		## Loading Defaults
		To load the default front matter we call the `ParseFrontMatter` method in the 
		HTML generator.
		*/
		private void LoadDefaultFrontMatter ()
		{
			var defaults = !_options.InputPath.WithFile (Options.DefaultsFile);
			if (File.Exists (defaults))
			{
				ConsoleOut ("Reading default front matter from '{0}'", defaults);
				_generateHtml.ParseFrontMatter (File.ReadAllText (defaults),
					defaults);
			}
		}
		/*	
		## TOC Management
		Loading and updating table of contents is handled by the 
		[TocManager](TocManager.html) class. We can add new entries to 
		TOC using the `AddToToc`method. Updating is performed only, if 
		`-u` switch is specified in the options.
		*/
		private void LoadToc ()
		{
			var tocFile = !_options.InputPath.WithFile (Options.TocFile);
			if (File.Exists (tocFile))
			{
				ConsoleOut ("Reading TOC from file '{0}'", tocFile);
				_tocManager = TocManager.Load (tocFile);
			}
			else
				_tocManager = new TocManager ();
		}

		private void AddToToc (SplitPath path)
		{
			if (_options.UpdateToc)
				_tocManager.AddToToc (path);
		}
	}
}