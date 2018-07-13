/*
# Document Weaver

The main workhorse of the application is the weaver. It enumerates the source 
files as specified by command line options and generates documentation for each 
file separately.

The Weaver class is abstract, and the actual documentation generation is
delegated to its subclasses. There are two subclasses for the two output
formats: [MdWeaver](MdWeaver.html) for markdown and [HtmlWeaver](HtmlWeaver.html) 
for HTML.

Let's first look at the base class which contains code common to both weavers.

## Dependencies
We utilize a couple of libraries provided by Microsoft to access MSBuild 
solution and project files. The first one resides in the `Microsoft.Build.*`
assemblies. They contain classes and methods for manipulating MS Build solution
files; to open a solution, enumerate the files in it, get the build 
configurations, and so on. 

The other important dependency is the [Roslyn](https://github.com/dotnet/roslyn)
compiler platform. The assemblies related to that lay under the `Microsoft.CodeAnalysis.*`
namespace. Weaver needs to reference the namespaces to access their types, but 
doesn't really use them. More involved code related to Roslyn and solution compilation 
is separated to the helper class [MSBuildHelpers](MSBuildHelpers.html). Weaver tries 
to keep things simple by working on the higher abstraction level.
*/
namespace LiterateProgramming
{
	using LiterateCS.Theme;
	using Microsoft.CodeAnalysis;
	using Buildalyzer;
	using Buildalyzer.Workspaces;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;

	public abstract class Weaver
	{
		/*
		## Creating a Weaver
		When creating a weaver the selected command line options must be passed to it. 
		*/
		protected Options _options;

		public Weaver (Options options)
		{
			_options = options;
		}
		/*
		## Generating Documentation
		Weaver can generate documentation from two sources: from a directory or from a 
		solution. One of the abstract methods below is invoked when the tool is run. Which
		one is selected depends on the command line options. 
		*/
		protected abstract void GenerateFromFiles ();
		protected abstract void GenerateFromSolution ();
		/*
		Another method which the subclasses can override is the CreateBlockBuilder that is
		defined	below. This	method will create the [BlockBuilder](BlockBuilder.html) class 
		that is responsible for splitting the input files into blocks. The standard 
		implementation is sufficient for generating markdown output, but 
		[HtmlWeaver](HtmlWeaver.html) requires more sophisticated functionality provided by 
		the [HtmlBlockBuilder](HtmlBlockBuilder.html) class.
		*/
		protected virtual BlockBuilder CreateBlockBuilder ()
		{
			return new BlockBuilder (_options);
		}
		/*
		### Main Execution Method
		Main program will call the method below to run the document generation. The method
		determines whether the files are read from a solution or from an input folder. Based
		on the options it will call the appropriate virtual method. 
		*/
		public void GenerateDocumentation ()
		{
			if (_options.Solution != null)
			{
				ConsoleOut ("Creating documentation for solution: {0}", 
					_options.Solution);
				GenerateFromSolution ();
			}
			else
			{
				ConsoleOut ("Creating documentation for files in directory: {0}", 
					_options.InputFolder);
				GenerateFromFiles ();
			}
			ConsoleOut ("Done.");
		}
		/*
		### Verbose Output
		If verbose mode is active (`-v` option), information about the progress of
		the tool is outputted to the console.
		*/
		protected void ConsoleOut (string text, params object[] args)
		{
			if (_options.Verbose)
				Console.WriteLine (text, args);
		}
		/*
		### Output Path for a File
		For each input file we need to construct the output path. We take the base 
		path from command line options and join it with the relative file path of 
		the input file changing the extension simultaneously. We also check that the 
		output directory exists, and create it when needed.
		*/
		protected SplitPath CreateOutputPath (SplitPath codeFile, string extension)
		{
			var outputFile = _options.OutputPath + codeFile.ChangeExtension (extension);
			ConsoleOut ("Generating {0} from file '{1}' into file '{2}'",
				extension, codeFile.FilePath, outputFile.FilePath);
			DirHelpers.EnsureExists (outputFile.DirectoryName);
			return outputFile;
		}
		/*
		## Selecting Input Files
		The subclasses can use the following methods to retrieve paths of he input 
		files. The methods use LINQ to enumerate the SplitPath structures that refer
		to files in a directory or in a solution.

		### Enumerating Files in Input Folder
		First variant returns all the files in the input directory that match the 
		specified filters. The filters are regular expressions that are constructed 
		from the glob patterns by the methods shown below.
		*/
		protected Regex[] FilterRegexes ()
		{
			return (from filt in _options.Filters
					select new Regex (WildcardToRegex (filt)))
				   .ToArray ();
		}

		protected static string WildcardToRegex (string pattern) =>
			"^" +
			Regex.Escape (pattern.Replace (
				Path.DirectorySeparatorChar == '\\' ? '/' : '\\', Path.DirectorySeparatorChar))
			.Replace (@"\*\*", ".*")
			.Replace (@"\*", "[^\\\\/]*")
			.Replace (@"\?", ".")
			+ "$";
		/*
		Now we can enumerate the input files.
		*/
		protected IEnumerable<SplitPath> InputFiles ()
		{
			var filtRegexes = FilterRegexes ();
			return from file in DirHelpers.Dir (_options.InputPath.BasePath, "*",
						_options.Recursive)
				   let relPath = SplitPath.Split (_options.InputPath.BasePath, file)
				   where filtRegexes.Any (re => re.IsMatch (relPath.FilePath))
				   select relPath;
		}
		/*
		We define also helpers to enumerate just code or markdown files.
		*/
		protected IEnumerable<SplitPath> MarkdownFiles ()
		{
			return InputFiles ().Where (IsMarkdownFile);
		}

		protected IEnumerable<SplitPath> SourceFiles ()
		{
			return InputFiles ().Where (IsSourceFile);
		}
		/*
		### Enumerating Files in Solution
		When a solution file is used as an input, markdown and C# files are retrieved
		separately. This is because the markdown files are not visible in MSBuild 
		Workspace provided by Roslyn. Apparently Roslyn is only interested in the C# 
		source files. We need the classes provided by the `Microsoft.Build.Construction` 
		library to access all files in a solution.

		First let's collect the markdown files from a solution file. Note that the LINQ
		query assumes the "Build Action" property of the retrieved files to be either 
		"Content" or "None". Files having other build types are skipped.
		*/
		/*
		And then we can gather the C# source files. Most of the heavy lifting is
		delegated to the MSBuildHelper class. In addition to the file path we
		return also the Document object that contains syntactic and semantic
		information that the Roslyn compiler attaches to the source file.
		*/
		protected IEnumerable<Tuple<SplitPath, Document>> CSharpDocumentsInSolution ()
		{
			var amanager = new AnalyzerManager (_options.Solution, 
				new AnalyzerManagerOptions ()
				{
					LogWriter = Console.Out
				});
			foreach (var proj in amanager.Projects)
			{
				//proj.Value.SetGlobalProperty ("OutputPath", Path.GetTempPath ());
				ConsoleOut ("Compiling project {0}", proj.Key);
				proj.Value.Build ();
			}
			var solution = amanager.GetWorkspace ().CurrentSolution;
			var filtRegexes = FilterRegexes ();
			return from proj in CompileProjectsInSolution (solution)
				   from doc in proj.Documents
				   let relPath = SplitPath.Split (_options.InputPath.BasePath, doc.FilePath)
				   where filtRegexes.Any (re => re.IsMatch (relPath.FilePath))
				   select Tuple.Create (relPath, doc);
		}

		public IEnumerable<Project> CompileProjectsInSolution (Solution solution)
		{
			foreach (var proj in solution.Projects)
			{
				/*
				Finally we can compile the project and yield it out for enumeration. 
				If there are compilation errors, they will be outputted to the console 
				window. As noted above, getting referencing errors does not necessarily 
				mean that the required semantic information is not available in compiled 
				project.
				*/
				ConsoleOut ("Analyzing project {0}", proj.Name);
				Project p = SuppressWarnings (proj, "CS1701", "CS8019");
				var diag = p.GetCompilationAsync ().Result.GetDiagnostics ();
				foreach (var msg in diag)
					Console.Error.WriteLine (msg);
				yield return p;
			}
		}

		private static Project SuppressWarnings (Project proj, params string[] warnings)
		{
			var diagOpts = proj.CompilationOptions.SpecificDiagnosticOptions;
			for (int i = 0; i < warnings.Length; i++)
				diagOpts = diagOpts.Add (warnings[i], ReportDiagnostic.Suppress);
			return proj.WithCompilationOptions (
				proj.CompilationOptions.WithSpecificDiagnosticOptions (diagOpts));
		}
		/*
		We need some helper functions to determine the type of a file. The type of a file
		is deduced from its extension or from its name. 
		*/
		protected bool IsSourceFile (SplitPath file) =>
			file.Extension == _options.SourceExt;

		protected bool IsMarkdownFile (SplitPath file) =>
			file.Extension == _options.MarkdownExt;
		/*
		The following three helper functions construct a block list from a markdown file, 
		source file or Roslyn document. They are used by the subclasses.
		*/
		protected BlockList BlockListFromMarkdown (string mdFile) =>
			CreateBlockBuilder ().FromMdFile (mdFile);

		protected BlockList BlockListFromCode (string codeFile) =>
			CreateBlockBuilder ().FromCodeFile (codeFile);

		protected BlockList BlockListFromDocument (Document document) =>
			CreateBlockBuilder ().FromDocument (document);
	}
}