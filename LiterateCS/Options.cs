/*
# Command Line Options & Usage

`csweave` is a console tool and it is controlled primarily through command line options. 
[Command Line Parser Library](https://commandline.codeplex.com/) is used to parse the options. It
simplifies the process and allows us to define new parameters just by adding properties to the 
Option class.

## Dependencies
*/
namespace LiterateProgramming
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using McMaster.Extensions.CommandLineUtils;
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
	the option is mandatory or not, its default value, and the associated help text. The type 
	of the option (boolean, string, enumeration, etc.) is inferred from the type of the property.
	*/
	[Command ("literatecs", Description = "Literate programming tool for C#")]
	[HelpOption ("-?")]
	public class Options
	{
		/*
		### Filters for Source Files
		The arguments given without an option specifiers `-` or `--` are assumed to be filters 
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
		[Option (CommandOptionType.MultipleValue, 
			Description = "Filters used to select the source files")]
		public IList<string> Filters { get; set; }

		/*
		### Solution File
		Instead of using the input folder get the files to be processed from a MSBuild 
		solution.
		*/
		[Option (CommandOptionType.SingleValue, ShortName = "s", LongName = "solution", 
			Description = "Process the all the C# and markdown files residing under a " +
			"msbuild solution")]
		public string Solution { get; set; }

		/*
		### Input Folder
		Input folder specifies the root of your source files. Typically you want to process 
		all the C# and markdown files in the source folder, so you define it with the `-i` 
		option and give `*.cs` and `*.md` as filters. If this option is omitted, the current 
		directory is assumed to be the input directory.
		*/
		[Option (CommandOptionType.SingleValue, ShortName = "i", LongName = "input", 
			Description = "The root folder for the files to be processed. Files under this " +
			"directory are checked against the filters. If input folder is not specified, " +
			"the current directory is used.")]
		public string InputFolder { get; set; }

		/*
		### Output Folder
		The output folder specifies where the tool stores the generated files. The output 
		folder will contain the markdown and HTML files, as well as all the auxiliary files 
		(CSS, Javascript, etc.)	needed by the documents.
		*/
		[Option (CommandOptionType.SingleValue, ShortName = "o", LongName = "output", 
			Description = "Output folder where the documentation will be generated to.")]
		public string OutputFolder { get; set; }

		/*
		### Source File Extension
		`csweave` needs to recognize which of the input files are source (C#) files, and 
		which ones are markdown files. Typically source files have the `.cs` extension. If 
		you are using a different file extension, you can specify it with the `-e` option.
		*/
		[Option (CommandOptionType.SingleValue, ShortName = "e", LongName = "csext",
			Description = "File extension for the C# source files. Used to identify " +
			"source files in the input folder.")]
		public string SourceExt { get; set; } = ".cs";

		/*
		### Markdown File Extension
		Similarly to C# files, markdown files are recognized by the file extension. If not 
		specified, `.md` is assumed to be the extension.
		*/
		[Option (CommandOptionType.SingleValue, ShortName =	"d", LongName = "mdext",
			Description = "File extension for the markdown files. Used to identify " +
			"markdown files in the input folder.")]
		public string MarkdownExt { get; set; } = ".md";

		/*
		### Include Subfolders
		If you want the tool to scan also the subfolder of the input folder, you can give 
		the `-r` option. When given, `csweave` will traverse the input folder recursively 
		and process all the files matching the filters, regardless of how deep in the folder 
		hierarchy they reside.
		*/
		[Option (CommandOptionType.SingleValue, ShortName = "r", LongName = "recursive", 
			Description = "Searches also the subfolders of the input folder for the files " +
			"to be processed.")]
		public bool Recursive { get; set; }

		/*
		### Output Format
		Output format is specified by the `-f` option. Only valid values are `md` for markdown 
		and `html` for HTML documents.
		*/
		[Option (CommandOptionType.SingleValue, ShortName = "f", LongName = "format", 
			Description = "Format of the outputted documentation; either 'md' or 'html'.")]
		public OutputFormat Format { get; set; } = OutputFormat.md;

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
		[Option (CommandOptionType.SingleValue, ShortName = "t", LongName = "trim", 
			Description = "Left-trim the comments before processing them to avoid interpreting " +
			"them as code blocks.")]
		public bool Trim { get; set; }

		/*
		### Update TOC
		If you want `csweave` to automatically add the files it processes to the table of
		contents file (`TOC.yml`), enable this option. This makes maintaining the TOC file
		easier and reminds you to update it after new files have been added.
		*/
		[Option (CommandOptionType.SingleValue, ShortName = "u", LongName = "updatetoc",
			Description = "Adds processed files that are not already in the TOC file to it.")]
		public bool UpdateToc { get; set; }

		/*
		### Theme
		Themes are DLLs which transform parsed data into HTML documents based on templates. Along 
		with the template there are auxiliary CSS, JS, and font files that need to be deployed to
		the target folder. Theme DLLs manage those files and copy them when needed. The default theme
		is provided by `csweave`, but if you want, you can copy it and customize it to your liking.
		The `--theme` option should be given, if you want to use a custom theme.
		*/
		[Option (CommandOptionType.SingleValue, ShortName = "m", LongName = "theme", 
			Description = "The theme DLL which generates HTML documents according to page " +
			"templates. If not specified, the default theme is used.")]
		public string Theme { get; set; } = @"Themes\Default\DefaultTheme.dll";

		/*
		### Verbose Mode
		If you want the tool to output information as it processes files, you can use the 
		verbose option.
		*/
		[Option (CommandOptionType.NoValue, ShortName = "v", LongName = "verbose",
			Description = "Outputs information about processed files to the standard output.")]
		public bool Verbose { get; set; }

		/*
		## Generating Documentation

		Finally we create a [Weaver](Weaver.html) object and call its `Generate` 
		method to generate the documentation according to the options.
		*/
		private int OnExecute ()
		{
			try
			{
				var weaver = Format == OutputFormat.html ?
					new HtmlWeaver (this) :
					(Weaver)new MdWeaver (this);
				weaver.GenerateDocumentation ();
			}
			catch (Exception e)
			{
				/*
				If an exception is thrown during the process, its error message is 
				outputted and the program is terminated with an error code.
				*/
				#region Main Error Handler

				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine ("Fatal error in document generation:");
				Console.WriteLine (e.Message);
				Console.ResetColor ();
				Console.WriteLine (e.StackTrace);
				if (e.InnerException != null)
				{
					Console.WriteLine ("Inner exception:");
					Console.WriteLine (e.InnerException.Message);
					Console.WriteLine (e.InnerException.StackTrace);
				}
				return 1;
				/*
				_This code block is used as an example on how to embed code into
				markdown files._
				*/
				#endregion
			}
			return 0;
		}
		/*
		## Split File Paths
		In order to make reading and parsing of the file and directory paths simpler, we will	
		define few helper properties that present them as [SplitPath](SplitPath.html) 
		structures. The idea behind this data type is explained on its own documentation page, 
		but in a nutshell its main purpose is to separate the two parts of a file path: the 
		absolute base path and the relative file path. Doing this makes the code that needs 
		these paths more succinct and readable. It also makes mapping the paths to hyperlinks 
		easier. 

		The files produced by `csweave` are stored in a similar directory hierarchy as the 
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
		(where the executables reside).	By default, these files reside under the directory	
		'_applicationDirectory_\Themes', each theme in its own subdirectory.
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
	}
}
/*
## Usage Examples
Below are some example command lines for the most common usage scenarios.

### Create Markdown Output

The following command line processes all C# and markdown files in a solution 
(including all projects and subfolders), and generates markdown output to a 
directory called "docs". It also includes the `-t` switch to strip the indentation 
from the comments. This setting is usually on.
```
csweave **.cs **.md -s <solution>.sln -o docs -f md -t
```

### Create HTML Output from Files in a Directory

The example below scans the subfolder "src" for C# files, and the root folder 
for markdown files. It produces HTML and outputs information to console for
each processed file (verbose option).

**Note 1:** If you use the `-i` switch to specify the input folder, and you want 
to process any subfolders under it, you must include the `-r` option to recursively 
scan them. Specifying the subfolder in the filters is not enough.

**Note 2:** Since we are just reading individual files C# files inside a directory
and do not provide a solution file, `csweave` cannot use Roslyn to compile the
code and produce semantic information. So, the syntax highlighting is limited
to basic tokens, and the links and type information is not available in the 
produced HTML files.
```
csweave src\*.cs *.md -i <root> -r -o docs -f html -t -v
```

### Create HTML Output for a Solution

The last example uses all the available functionality. It produces documentation
for the `csweave` tool itself. It specifies the solution file, and filters out
C# files under "src" and "LiterateCS.Theme" subdirectories, along with the markdown
files under the solution directory.

Table of contents file is updated while the files are processed. This is specified
with the `-u` switch.

```
csweave src\*.cs LiterateCS.Theme\*.cs *.md -s ..\..\LiterateProgramming.sln -o ..\..\docs -f html -t -v -u
```

**Tip:** If you think the command line above is a bit dense, you can also use the 
long format for the options to make them more understandable. The options are 
case-insensitive, so you can write them in any way you like.

```
csweave src\*.cs LiterateCS.Theme\*.cs *.md --Solution ..\..\LiterateProgramming.sln --Output ..\..\docs --Format html --Trim --Verbose --UpdateTOC
```
*/