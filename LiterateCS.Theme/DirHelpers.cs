/*
# Directory Utilities

This class contains utility functions needed by theme assemblies as well as the
`csweave` tool. It is an assorted collection static functions, mostly related to
directory manipulation, that do not logically belong to any other class.
*/
namespace LiterateCS.Theme
{
	using System.IO;
	using System.Linq;

	public static class DirHelpers
	{
		/*
		The following method copies all subdirectories of a given input directory 
		to a target directory. **Note** that it only copies the directory structure,
		not the files inside those directories.
		
		You can specify a glob filter for the input directories. Only directories 
		with names matching the filter will be copied. If you want all the 
		directories copied, give `*` as the filter.
		
		You can	also specify with the `recurse` parameter whether the whole 
		directory tree is copied, or just the top level directories.
		*/
		public static void CopySubDirectories (string inputRoot, string outputRoot, 
			string filter, bool recurse)
		{
			foreach (string subDir in Directory.GetDirectories (inputRoot, filter,
				GetSearchOption (recurse)))
				EnsureExists (subDir.Replace (inputRoot, outputRoot));
		}
		/*
		The method below is used to make sure that a given directory exists. Most 
		of the file operations fail, if a target directory does not exist.
		*/
		public static void EnsureExists (string dir)
		{
			if (!Directory.Exists (dir))
				Directory.CreateDirectory (dir);
		}
		/*
		The following methods are just shorthands for GetFiles and Copy methods 
		defined in the System.IO namespace. They define a more convenient 
		signature for the wrapped methods.
		*/
		public static string[] Dir (string path, string filter, bool recurse) =>
				Directory.GetFiles (path, filter, GetSearchOption (recurse));

		public static void Copy (string file, string targetFolder, bool overwrite)
		{
			File.Copy (file, Path.Combine (targetFolder, Path.GetFileName (file)), overwrite);
		}

		public static SearchOption GetSearchOption (bool recurse) =>
			recurse ?
				SearchOption.AllDirectories :
				SearchOption.TopDirectoryOnly;
	}
}
