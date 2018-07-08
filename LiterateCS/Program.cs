/*
# Main Program

The main program is quite simple. It just checks the command line arguments and calls the 
[Weaver](Weaver.html) class that does the actual work.

## Dependencies

There are no special dependencies in the main program. Only reference is to the System 
namespace.
*/
namespace LiterateProgramming
{
	using McMaster.Extensions.CommandLineUtils;
	/* 
	The main class imaginatively named as `Program`. 
	*/
	class Program
	{
		static int Main (string[] args)
		{
			/* 
			## Command Line Parsing

			First we create a command line parser that we got from the 
			[NuGet](https://commandline.codeplex.com/) library. We configure the command line options 
			to be case insensitive.

			Then we parse the command line options into an object that contains the settings. If the
			parsing fails, the parser will output usage information automatically after which we terminate
			the program.
			*/
			return CommandLineApplication.Execute<Options> (args);
		}
	}
}
/*
And that's it! If you want to know how the document generation actually works, jump to the 
documentation of the [Weaver](Weaver.html) class.
*/
