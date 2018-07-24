/*
# Main Program

The main program is very simple. It just parses the command line arguments and calls the 
[Weaver](Weaver.html) class that does the actual work.
*/
namespace LiterateCS
{
	using System;
	using CommandLine;
	using LiterateCS.Theme;

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
			settings. We use the settings read from the `defaults.yml` file to 
			initialize the Options object.
			
			If the parsing fails, the parser will output usage information 
			automatically after which we terminate the program with exit code 0. 
			*/
			try
			{
				var defaultOptions = new Lazy<Options> (Options.LoadFromDefaultsFile);
				cmdLineParser.ParseArguments (() => defaultOptions.Value, args)
					.WithParsed (GenerateDocumentation);
				return 0;
			}
			catch (LiterateException le)
			{
				/*
				If some parameters are wrong, we get a LiterateException for those errors.
				The exception can output itself cleanly by overriding the ToString method.
				We exit with error code 1.
				*/
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine (le);
				Console.ResetColor ();
				return 1;
			}
			catch (Exception e)
			{
				/*
				If an unexpected exception is thrown during the process, its error message 
				is outputted and the program is terminated with an error code 2.
				*/
				#region Main Error Handler

				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine ("Unexpected error in document generation:");
				Console.WriteLine (e.Message);
				Console.ResetColor ();
				Console.WriteLine (e.StackTrace);
				if (e.InnerException != null)
				{
					Console.WriteLine ("Inner exception:");
					Console.WriteLine (e.InnerException.Message);
					Console.WriteLine (e.InnerException.StackTrace);
				}
				return 2;
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
			/*
			First we output the effective options we are going to use. This helps 
			the user to see what options were picked form `defaults.yml` file.
			*/
			options.OutputEffectiveOptions ();
			/*
			Then we create a [Weaver](Weaver.html) object and call its `Generate` 
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
