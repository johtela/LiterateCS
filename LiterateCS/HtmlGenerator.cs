/*
# HTML Generation

The last piece of the HTML related functionality is the HtmlGenerator class 
which is accountable for the following tasks:

* Loading the theme.
* Parsing the front matter of an input file and passing its parameters to the 
  theme.
* Converting documentation blocks and markdown files to HTML.
* Rendering the page using the theme.

This might seem like lot of work but actually most of the steps outlined above
are delegated to other libraries. Parsing the front matter is performed using
the [YamlDotNet](https://github.com/aaubry/YamlDotNet) library. 
[Markdig](https://github.com/lunet-io/markdig) library takes care of converting
markdown to HTML. References to the corresponding namespaces can be found below.
*/
namespace LiterateProgramming
{
	using LiterateCS.Theme;
	using Markdig;
	using System;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using YamlDotNet.RepresentationModel;

	public class HtmlGenerator
	{
		/*
		## Instance Variables

		As with most of the utility classes, we need to store a reference to the 
		Options object containing the command line parameters.
		*/
		private Options _options;
		/*
		Markdig configuration is stored in the field below.
		*/
		private readonly MarkdownPipeline _pipeline;
		/*
		The parameters passed to the theme are stored in the `_params` field.
		*/
		private PageParams _params;
		/*
		The `_theme` field contains the reference to the loaded theme.
		*/
		private Theme _theme;
		/*
		## Constructor

		Options and TOC are passed as parameters to the constructor. The rest of
		the fields are initialized inside it. Theme is loaded from the disk, 
		Markdown pipeline is created and configured, and a PageParams object is
		created.
		*/
		public HtmlGenerator (Options options, Toc toc)
		{
			_options = options;
			_theme = LoadTheme ();
			_pipeline = new MarkdownPipelineBuilder ()
				.UseAutoIdentifiers ()
				.UseYamlFrontMatter ()
				.UseMathematics ()
				.UseDiagrams ()
				.UsePipeTables ()
				.UseEmphasisExtras ()
				.Build ();
			_params = new PageParams
			{
				Toc = toc
			};
			ConvertTocMarkdown ();
		}
		/*
		## Loading the Theme

		The theme assembly is loaded dynamically using the reflection API 
		provided by .NET framework. After the assembly is loaded, we enumerate 
		the types defined in it and find the first one which inherits from the 
		Theme class. If no such type is found, we throw an exception. Otherwise
		we use the reflection API again to construct an instance of the theme
		class we discovered.
		*/
		private Theme LoadTheme ()
		{
			var themeDll = !_options.ThemePath;
			if (!File.Exists (themeDll))
				throw new ArgumentException ("Could not find theme file in: " + themeDll);
			var assy = Assembly.LoadFile (themeDll);
			var themeType = assy.GetTypes ().FirstOrDefault (t => 
				t.IsSubclassOf (typeof (Theme)));
			if (themeType == null)
				throw new ArgumentException (
					"Could not find Theme class from the assembly " +
					_options.Theme);
			return (Theme)Activator.CreateInstance (themeType);
		}
		/*
		## Converting TOC entries to HTML

		It is possible to use markdown also inside TOC file. The functions below
		converts the TOC entries to HTML.
		*/
		private void ConvertTocMarkdown ()
		{
			foreach (var entry in _params.Toc.Contents)
			{
				entry.Page = TocEntryToHtml (entry.Page);
				entry.Desc = TocEntryToHtml (entry.Desc);
			}
		}
		/*
		One annoyance with Markdig is that it automatically wraps converted
		markdown snippets inside `<p>` tag. This is not really desirable when
		outputting TOC entries as it messes up the formatting of list entries.	
		There is no way to disable this behavior, as far as I could find, so 
		the only option is to strip the `<p>` tag after conversion.
		*/
		private string TocEntryToHtml (string markdown)
		{
			if (markdown == null)
				return null;
			var result = Markdown.ToHtml (markdown, _pipeline).Trim ();
			var len = result.Length;
			if (result.Substring (0, 3) == "<p>" && result.Substring (len - 4, 4) == "</p>")
				result = result.Substring (3, len - 7); 
			return result;
		}
		/*
		## Parsing Front Matter

		Front matter contains parameters that are used by themes. Front matter
		can appear in both C# files and markdown files. It has to be the first 
		piece of text in a file, excluding the comment start token `/*` when 
		used inside a C# file.

		Parsing is done in two steps. First, we extract the lines of text that 
		constitute the front matter. Then, we feed these lines to the YamlDotNet 
		parser.
		*/
		private string GetFrontMatter (string markdown)
		{
			/*
			We use the StringReader class to read the input string one line at
			a time. If the first line we read does not contain three hyphens,
			we decide that the front matter is not present and return null.
			*/
			using (var reader = new StringReader (markdown))
			{
				var line = reader.ReadLine ();
				if (line.Trim () != "---")
					return null;
				/*
				Now we know that front matter is defined, so we copy it to a
				new string. We create a StringBuilder to construct it. We append
				lines to the result until we encounter another line with exactly
				three hyphens. The result then contains both starting and ending 
				hyphens.
				*/
				var result = new StringBuilder ();
				result.AppendLine (line);
				do
				{
					line = reader.ReadLine ();
					result.AppendLine (line);
				}
				while (line.Trim () != "---");
				return result.ToString ();
			}
		}
		/*
		Parsing begins with a call to GetFrontMatter function. It isolates the 
		front matter from the markdown string given as argument. We also need
		the input file path for reporting possible errors. Nothing is read from 
		the file, though.

		ParseFrontMatter returns the value of the special parameter `template`. 
		This parameter controls which page template to use, and it needs to be 
		reseted to its default value when it is not present. It works 
		differently from other parameters which hold their value until a new 
		value is set. If the `template` parameter is not present or the whole 
		front matter is absent, null is returned.
		*/
		public string ParseFrontMatter (string frontMatter, string filePath)
		{
			string result = null;
			if (frontMatter != null)
			{
				/*
				Now we can create a new YamlStream and load the front matter 
				using it. The parameters can be iterated through as simple 
				key-value pairs.
				*/
				try
				{
					var yamlStream = new YamlStream ();
					yamlStream.Load (new StringReader (frontMatter));
					var mapping = (YamlMappingNode)yamlStream.Documents[0].RootNode;
					foreach (var entry in mapping.Children)
					{
						var key = entry.Key.ToString ();
						var value = entry.Value.ToString ();
						/*
						If we encounter the special parameter `template`, we store
						its value in the result. Special treatment is required also
						for parameters starting with underscore `_`. Those can 
						contain markdown formatting and thus they are automatically 
						converted to HTML.
						*/
						if (key.ToLower () == "template")
							result = value;
						else
							_params.Add (key, 
								key.StartsWith ("_") ?
									Markdown.ToHtml (value, _pipeline) :
									value);
					}
				}
				/*
				Parsing errors are caught, supplemented with information on 
				which input file the error occurred, and re-thrown to the 
				main program which eventually reports them to the user.
				*/
				catch (YamlDotNet.Core.SyntaxErrorException e)
				{
					throw new InvalidOperationException (
						"Error parsing front matter from: " + filePath, e);
				}			
			}
			return result;
		}
		/*
		## Initializing Page Specific Parameters

		There are a few built-in parameters that the theme can use. They are 
		updated before processing each document by the method below. Refer to 
		the documentation of the [PageParams](../LiterateCS.Theme/PageParams.html) 
		class for more information about the parameters.
		*/
		private void InitParams (SplitPath inputFile, SplitPath outputFile)
		{
			_params.Filename = outputFile.FileNameWithoutExtension;
			_params.Root = outputFile.RelativeFileRoot;
			_params.CurrentSection = _params.Toc.Flattened.FirstOrDefault (e =>
				 e.File == inputFile.FilePath);
		}
		/*
		## Generating HTML Page from Code File

		Once a source file has been split into blocks, we can give the block
		list and the input and output file paths to the `Execute` method. This
		is where the HTML page is actually generated.
		*/
		public void Execute (BlockList blocks, SplitPath inputFile, 
			SplitPath outputFile)
		{
			/*
			First, we initialize the document-level parameters.
			*/
			InitParams (inputFile, outputFile);
			/*
			If the first block in the list is a markdown block, then we need to
			check if it contains a front matter. We initialize the used template
			to null, which selects the default template. The setting can be then
			overridden in the front matter.
			*/
			string template = null;
			var firstMdBlock = blocks.FirstOrDefault (b => b.Kind == BlockKind.Markdown);
			if (firstMdBlock != null)
				template = ParseFrontMatter (
					GetFrontMatter (firstMdBlock.Contents), !inputFile);
			/*
			Next, we iterate through the blocks, convert them to HTML, and finally
			assign the resulted string into the `Contents` parameter, so that the 
			theme can access it.
			*/
			using (var writer = new StringWriter ())
			{
				foreach (var block in blocks)
					writer.Write (Markdown.ToHtml (block.Contents, _pipeline));
				_params.Contents = writer.ToString ();
			}
			/*
			As the last step, we render the page using the theme, and write the 
			resulting HTML page to the output file.
			*/
			using (var writer = File.CreateText (!outputFile))
				writer.Write (_theme.RenderPage (template, _params));
		}
		/*
		## Copying Auxiliary Files

		HTML pages almost never work by themselves. They rely on external
		resources such as style sheets, images, Javascript files, fonts, etc.
		These files need to be copied to the target directory, alongside the 
		HTML files. The theme is responsible for copying the files, but since
		HTML generator is controlling the theme, it has to expose the method
		to other classes.
		*/
		public void CopyAuxiliaryFiles ()
		{
			_theme.CopyAuxiliaryFiles (_options.ThemePath.BasePath,
				_options.OutputPath.BasePath);
		}
	}
}