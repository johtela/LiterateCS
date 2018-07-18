/*
# Main Program

The main program is very simple. It just parses the command line arguments and calls the 
[Weaver](Weaver.html) class that does the actual work.
*/
namespace LiterateCS
{
	using System;
	using CommandLine;
	/* 
	The main class is imaginatively named as `Program`. 
	*/
	class Program
	{
		static int Main (string[] args)
		{
			/* 
			## Command Line Parsing
			First we create a command line parser and configure it to be case 
			insensitive.
			*/
			var cmdLineParser = new Parser (settings =>
			{
				settings.CaseSensitive = false;
				settings.HelpWriter = Console.Out;
			});
			/*
			Then we parse the command line options into an object that contains the 
			settings. If the parsing fails, the parser will output usage information 
			automatically after which we terminate the program with exit code 0.
			*/
			try
			{
				var defaultOptions = new Lazy<Options> (Options.LoadFromDefaultsFile);
				cmdLineParser.ParseArguments (() => defaultOptions.Value, args)
					.WithParsed (GenerateDocumentation);
				return 0;
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
		}
		/*
		## Generating Documentation
		If the parsing succeeds, CommandLineParser library will call the 
		GenerateDocumentation method with the parsed options.
		*/
		private static void GenerateDocumentation (Options options)
		{
			options.OutputEffectiveOptions ();
			/*
			We create a [Weaver](Weaver.html) object and call its `Generate` 
			method to generate the documentation according to the options.
			*/
			var weaver = options.Format == OutputFormat.html ?
				new HtmlWeaver (options) :
				(Weaver)new MdWeaver (options);
			weaver.GenerateDocumentation ();
		}
	}
}
/*
That's all! To see how the documentation generation actually works, jump to the 
[Weaver](Weaver.html) class.
*/
