/*
# Command Line Options & Usage

LiterateCS is a console tool and it is controlled primarily through command line options. 
[Command Line Parser Library](https://github.com/commandlineparser/commandline) is used to parse 
the options. It simplifies the process and allows us to define new parameters just by adding 
properties to the Option class.

## Dependencies
*/
namespace LiterateCS
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using ExtensionCord;
	using CommandLine;
	using CommandLine.Text;
	using YamlDotNet.Core;
	using YamlDotNet.Core.Events;
	using YamlDotNet.Serialization;
	using LiterateCS.Theme;

	/*
	## Output Format
	The output format is either HTML or Markdown. The following enumeration is used to define 
	the possible options.
	*/
	public enum OutputFormat { md, html }

	/*
	## Available Options
	The Option class defines the available command line options as properties. These properties 
	are decorated by attributes that specify the short and long format of the option, whether 
	the option is mandatory or not, and the associated help text. Also, the name of the option
	in a YAML file is specified. The type of an option (boolean, string, enumeration, etc.) 
	is inferred from the type of the associated property.
	*/
	public class Options
	{
		/*
		There are two special input files which are processed separately. They
		are	defined in [YAML](http://yaml.org/) format and they have fixed names. 
		The first one is the `defaults.yml` file which contains the default settings 
		used by LiterateCS. This file will be processed before any of the other files, 
		so all the global (project-level) properties should be defined in it. 
		
		The first part of the file contains front matter defaults. See the chapter on
		[Front Matter](../FrontMatter.html) to learn about available properties. The
		second part of the file contains the default command line arguments. The 
		processing order is that `defaults.yml` file will be read first and all the
		command line arguments found there are used as default values. However, if 
		the same option is specified in the command line, it overrides the setting 
		found in the defaults file. If an option is not present in the defaults file
		or in the command line, the application-level default value is used.
		*/
		public const string DefaultsFile = "defaults.yml";
		/*
		The other special file is the table of contents or _TOC_ for short. Its file 
		name is `TOC.yml` and its structure is described in 
		[its own section](../TableOfContents.html). You can add entries to TOC manually, 
		or use the `-u` option to automatically update it when files are missing from it.
		*/
		public const string TocFile = "TOC.yml";
		/*
		### Filters for Source Files
		The arguments given without option specifiers `-` or `--` are assumed to be filters 
		that specify what files are processed by the tool. You can give multiple filters or file 
		names to be searched for. The filters may contain literal text and three kinds of 
		wildcards that conform to the _glob_ convention:

		 -	`?` matches a single character.
		 -	`*` matches zero or more occurrences of any character except `\`. So, this wild card 
			basically matches a part of a file or a directory name.
		 -	`**` matches zero or more occurrences of any character including the backslash. This 
			wildcard can be used to select files that reside in subdirectories.

		The file names that are matched against the filters are given relative either to the 
		input folder or to the solution folder.
		*/
		private IEnumerable<string> _filters;

		[Value (0, MetaName = "<filters>", 
		HelpText = "Filters for input files; both source and markdown.")]
		[YamlMember (Alias = "filters")]
		public IEnumerable<string> Filters
		{
			get => _filters;
			set
			{
				/*
				This bit of logic prevents command line parser from overriding
				filters read from `defaults.yml` with an empty list.
				*/
				if ((value != null && value.Any ()) ||
					_filters == null || _filters.None ())
					_filters = value;
			}
		}

		/*
		### Solution File
		Instead of using the input folder get the files to be processed from a MSBuild 
		solution.
		*/
		[Option ('s', "solution", Required = false,
		HelpText = "Read the the C# and markdown files from a msbuild solution (*.sln). " +
		"If this option is specified, the '--input' option is ignored.")]
		[YamlMember (Alias="solution")]
		public string Solution { get; set; }

		/*
		### Input Folder
		Input folder specifies the root of your source files. Typically you want to process 
		all the C# and markdown files in the source folder, so you define it with the `-i` 
		option and give `*.cs` and `*.md` as filters. If this option is omitted, the current 
		directory is assumed to be the input directory.
		*/
		[Option ('i', "input", Required = false,
		HelpText = "The root folder for the files to be processed. Files under this " +
		"directory are checked against the filters. If input folder is not specified, the " +
		"current directory is used.")]
		[YamlMember (Alias = "input")]
		public string InputFolder { get; set; }

		/*
		### Output Folder
		The output folder specifies where the tool stores the generated files. The output 
		folder will contain the markdown and HTML files, as well as all the auxiliary files 
		(CSS, Javascript, etc.)	needed by the documents. If a relative path is specified,
		then it is relative to the input folder or solution folder. If nothing is specified, 
		the default output folder is set to "docs/" under the input folder.
		*/
		[Option ('o', "output", Required = false,
		HelpText = "Output folder where the documentation will be generated to. "+ 
		"The default output folder is <input|solutionfolder>/docs.")]
		[YamlMember (Alias = "output")]
		public string OutputFolder { get; set; } = "docs";

		/*
		### Source File Extension
		LiterateCS needs to recognize which of the input files are source (C#) files, and 
		which ones are markdown files. Typically source files have the `.cs` extension. If 
		you are using a different file extension, you can specify it with the `-e` option.
		*/
		[Option ('e', "csext",
		HelpText = "File extension for the C# source files. Used to identify source files " +
		"in the input folder. The default is '.cs'")]
		[YamlMember (Alias = "csext")]
		public string SourceExt { get; set; } = ".cs";

		/*
		### Markdown File Extension
		Similarly to C# files, markdown files are recognized by the file extension. If not 
		specified, `.md` is assumed to be the extension.
		*/
		[Option ('d', "mdext",
		HelpText = "File extension for the markdown files. Used to identify markdown files " +
		"in the input folder. The default is '.md'")]
		[YamlMember (Alias = "mdext")]
		public string MarkdownExt { get; set; } = ".md";

		/*
		### Output Format
		Output format is specified by the `-f` option. Only valid values are `md` for markdown 
		and `html` for HTML documents.
		*/
		[Option ('f', "format", 
		HelpText = "Format of the outputted documentation; either 'md' or 'html'. " +
		"Default is 'md'")]
		[YamlMember (Alias = "format")]
		public OutputFormat Format { get; set; }

		/*
		### Comment Trimming
		The `--trim` argument indicates whether comments extracted from the source files will be 
		left-trimmed. Visual Studio likes to indent the comment blocks automatically to the same 
		level as the code. When these comment lines are extracted they will contain the leading 
		spaces or tabs that appear in the source file. If not trimmed, these lines will be 
		interpreted as code blocks in the markdown, which is not what we usually want. So, if you 
		have indented comments that you want to be considered as normal text in markdown, include 
		this option in your arguments.
		*/
		[Option ('t', "trim", 
		HelpText = "Left-trim the comments before processing them to avoid interpreting " +
		"them as code blocks.")]
		[YamlMember (Alias = "trim")]
		public bool Trim { get; set; }

		/*
		### Update TOC
		If you want LiterateCS to automatically add the files it processes to the table of
		contents file (`TOC.yml`), enable this option. This makes maintaining the TOC file
		easier and reminds you to update it after new files have been added.
		*/
		[Option ('u', "updatetoc", 
		HelpText = "Adds processed files that are not already in the TOC file to it.")]
		[YamlMember (Alias = "updatetoc")]
		public bool UpdateToc { get; set; }

		/*
		### Theme
		Themes are .NET assemblies which transform parsed code and text into HTML documents 
		using page templates. In addition, themes manage the auxiliary CSS, JS, and font files 
		that need to be copied to the target folder. The default theme is provided by LiterateCS, 
		but if you want, you can clone it and customize it to your liking.	The `--theme` option 
		should be specified, if you want to use a custom theme.
		*/
		[Option ('m', "theme",
		HelpText = "The theme DLL which generates HTML documents according to page " +
		"templates. If not specified, the default theme is used.")]
		[YamlMember (Alias = "theme")]
		public string Theme { get; set; } = "DefaultTheme.dll";

		/*
		### Build Log
		When a solution file is used as an input, LiterateCS first builds the solution to
		determine all its dependencies. If you want to get a log file of the build for 
		troubleshooting, use the `--buildlog` option.
		*/
		[Option ('b', "buildlog", Required = false,
		HelpText = "Path to the build log file.")]
		[YamlMember (Alias = "buildlog")]
		public string BuildLog { get; set; }

		/*
		### Verbose Mode
		If you want the tool to output information as it processes files, you can use the 
		verbose option.
		*/
		[Option ('v', "verbose",
		HelpText = "Outputs information about processed files to the standard output.")]
		[YamlMember (Alias = "verbose")]
		public bool Verbose { get; set; }

		/*
		## Split File Paths
		In order to make reading and parsing of the file and directory paths simpler, we will	
		define few helper properties that present them as [SplitPath](SplitPath.html) 
		structures. The idea behind this data type is explained on its own documentation page, 
		but in a nutshell its main purpose is to separate the two parts of a file path: the 
		absolute base path and the relative file path. Doing this makes the code that needs 
		these paths more succinct and readable. It also makes converting paths to hyperlinks 
		easier. 

		The files produced by LiterateCS are stored in a similar directory hierarchy as the 
		input files. The base directory for the input files is defined by the `-i` option, or 
		if a solution file is provided (with the `-s` option), the base input directory will
		be the one where the solution file resides. If neither is given, the current directory
		is used as the base input path.
		*/
		public SplitPath InputPath =>
			Solution != null ? SplitPath.Base (Path.GetDirectoryName (Solution)) :
			InputFolder != null ? SplitPath.Base (InputFolder) :
			SplitPath.Base (Directory.GetCurrentDirectory ());
		/*
		The output base path is given with the mandatory `-o` option. The `OutputPath` property
		just wraps it inside the `SplitPath` structure.
		*/
		public SplitPath OutputPath =>
			SplitPath.Base (OutputFolder);
		/*
		Lastly, the theme path is constructed. The base path of the theme is the directory
		where the theme DLL resides. If the theme path given as an option is not an absolute
		one, it is assumed that the path points to a folder under the application directory
		(where the executables reside).
		*/
		public SplitPath ThemePath
		{
			get
			{
				var fullPath = Path.IsPathRooted (Theme) ? Theme :
					Path.Combine (
						Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location), 
						Theme);
				return new SplitPath (Path.GetDirectoryName (fullPath),
					Path.GetFileName (fullPath));
			}
		}
		/*
		## Loading Command Line Options from `defaults.yml`
		Typically you run the `literatecs` command with the same options again and
		again. For this reason, it is usually easier to specify the options in the
		`defaults.yml` file. You can still override the options in the command line
		if desired, but the initial values will be fetched from the	defaults file. 
		If it does not appear there either, the application default is used.
		*/
		public static Options LoadFromDefaultsFile ()
		{
			if (!File.Exists (DefaultsFile))
				return new Options ();
			Console.WriteLine ("Reading default command line options from '{0}'", 
				DefaultsFile);
			try
			{
				/*
				We use the [YamlDotNet serialization](
				https://github.com/aaubry/YamlDotNet/wiki/Samples.DeserializingMultipleDocuments) 
				to read the file.
				*/
				using (var input = File.OpenText (DefaultsFile))
				{
					var deserializer = new DeserializerBuilder ()
							.Build ();
					var parser = new YamlDotNet.Core.Parser (input);
					/*
					We skip the first document, which contains the front matter defaults.
					*/
					parser.Expect<StreamStart> ();
					deserializer.Deserialize<Dictionary<string, string>> (parser);
					/*
					Then we can deserialize the default command line options.
					*/
					var result = deserializer
						.Deserialize<Options> (parser);
					/*
					If no settings could be found, we return an Option object initialized 
					with application-level defaults.
					*/
					return result ?? new Options ();
				}
			}
			catch (YamlException e)
			{
				throw new LiterateException (
					"Invalid syntax in the 'defaults.yml' file. Make sure that the YAML data " +
					"is defined according to the specification.",
					DefaultsFile, "http://yaml.org/", e);
			}
		}
		/*
		After both defaults file and command line argument are parsed, it is good
		to check that all effective options are correct. For that we will provide a
		method that outputs all the options as a command line. We utilize the new 
		feature in the Command Line Parser library, which 
		"[unparses](https://github.com/commandlineparser/commandline/wiki#usage-attribute)" 
		the option object back to command line arguments.
		*/
		public void OutputEffectiveOptions ()
		{
			Console.WriteLine ("Command line options used:");
			Console.WriteLine (CommandLine.Parser.Default.FormatCommandLine (this));
		}
		/*
		## Usage Examples
		Below are some example command lines for the most common usage scenarios. 
		*/
		[Usage (ApplicationAlias = "literatecs")]
		public static IEnumerable<Example> Examples =>
			new Example[]
			{
				/*
				### Create Markdown Output
				The following command line processes all C# and markdown files in a 
				solution (including all projects and subfolders), and generates markdown 
				output to a directory called "docs". It also includes the `-t` switch to 
				strip the indentation from the comments. This setting is usually on.
				```
				literatecs **.cs **.md -s <solution>.sln -o docs -f md -t
				```
				Below we construct this example using an instance of Options class.
				Command Line Parser shows this example in the help screen which is
				outputted whenever there are errors in the command line, or when the 
				`--help` option is specified.
				*/
				new Example ("Generate markdown documentation for a solution",
					new UnParserSettings
					{
						PreferShortName = true,
						GroupSwitches = true
					},
					new Options
					{
						Filters = new string[] { "**.cs", "**.md" },
						Solution = "<solution>.sln",
						OutputFolder = "docs",
						Format = OutputFormat.md,
						Trim = true
					}),
				/*
				### Create HTML Output from Files in a Directory
				The example below scans the subfolder "src" for C# files, and the root folder 
				for markdown files. It produces HTML and outputs information to console for
				each processed file (verbose option).

				**Note:** Since we are just reading individual files C# files inside a directory
				and do not provide a solution file, LiterateCS cannot use Roslyn to compile the
				code and produce semantic information. So, the syntax highlighting is limited
				to basic tokens, and the links and type information is not available in the 
				produced HTML files.
				```
				literatecs src\*.cs *.md -i <root> -o docs -f html -tv
				```
				*/
				new Example ("Create HTML documentation from files in a directory",
					new UnParserSettings
					{
						PreferShortName = true,
						GroupSwitches = true
					},
					new Options
					{
						Filters = new string[] { "src\\*.cs", "*.md" },
						InputFolder = "<root>",
						OutputFolder = "docs",
						Format = OutputFormat.html,
						Trim = true,
						Verbose = true
					}),
				/*
				### Create HTML Output for a Solution
				The last example uses all the available functionality. It produces documentation
				for the LiterateCS tool itself. It specifies the solution file, and processes C# 
				files under "src" subdirectory, along with the markdown files under the solution 
				directory.

				Table of contents file is updated while the files are processed. This is specified
				with the `-u` switch.

				```
				literatecs src\*.cs *.md -s <solution>.sln -o docs -f html -tuv
				```
				*/
				new Example ("Create HTML documentation for a solution",
					new UnParserSettings
					{
						PreferShortName = true,
						GroupSwitches = true
					},
					new Options
					{
						Filters = new string[] { "src\\*.cs", "*.md" },
						Solution = "<solution>.sln",
						OutputFolder = "docs",
						Format = OutputFormat.html,
						Trim = true,
						UpdateToc = true,
						Verbose = true
					}),
				/*
				**Tip:** If you think the command line above is a bit dense, you can also use the 
				long format for the options to make them more understandable. The options are 
				case-insensitive, so you can write them in any way you like.

				```
				literatecs src\*.cs *.md --solution <solution>.sln --output docs --format html --trim --UpdateTOC --verbose 
				```
				*/
				new Example ("Same example with long option names",
					new Options
					{
						Filters = new string[] { "src\\*.cs", "*.md" },
						Solution = "<solution>.sln",
						OutputFolder = "docs",
						Format = OutputFormat.html,
						Trim = true,
						UpdateToc = true,
						Verbose = true
					})
			};
	}
}