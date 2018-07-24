/*
# Reporting Errors

There are several cases when an exception might be thrown because of invalid input
data or some user-related error. For these cases, we define a custom Exception class. 
When thrown, it will be reported differently than the other "unexpected" exceptions 
caught in the main program. We try to give as much information as possible about the 
error, so that user knows how to fix the problem.
*/
namespace LiterateCS.Theme
{
	using System;
	using System.Text;

	public class LiterateException : Exception
    {
		/*
		The errors always refer to some file we are processing, so we can define it
		as a mandatory field of the exception.
		*/
		public readonly string FileName;
		/*
		The other fields that we require are the actual error message, the link to
		the documentation that helps the user to understand what the application 
		expects, and the inner exception which can be null. Inner exception explains 
		the actual problem; the outer exception gives the context what the application 
		was doing when it encountered the error.
		*/
		public LiterateException (string message, string file, string helpLink, 
			Exception innerException) : base (message, innerException)
		{
			FileName = file;
			HelpLink = helpLink;
		}

		public LiterateException (string message, string file, string helpLink)
			: this (message, file, helpLink, null) { }
		/*
		We format the fields of the exception to a readable layout by overriding the
		ToString method.
		*/
		public override string ToString ()
		{
			var res = new StringBuilder ();
			res.AppendFormat ("Error occurred while accessing file: {0}\n\n", 
				FileName);
			res.AppendLine ("Description:");
			res.AppendLine ("------------");
			res.AppendLine (Message);
			if (InnerException != null)
			{
				res.AppendLine ();
				res.AppendLine ("Detailed Description:");
				res.AppendLine ("---------------------");
				res.AppendLine (InnerException.Message);
			}
			res.AppendLine ();
			res.AppendFormat ("For more information see: {0}", HelpLink);
			return res.ToString ();
		}
	}
}
