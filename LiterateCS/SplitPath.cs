/*
# SplitPath Structure

`csweave` operates with three base directories:

* input directory which contains the files to be processed,
* output directory where the generated documentation will be placed, and
* theme directory which stores the HTML rendering library and its auxiliary files.

When accessing files in these directories it is often necessary to segregate
the base directory and the relative path inside that directory. For this purpose, 
we define a helper data structure that encapsulates a file path, but stores it in 
two parts: base path and file path. We call this data structure SplitPath and 
define it as follows.
*/
namespace LiterateProgramming
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Text;

	public struct SplitPath
	{
		public readonly string BasePath;
		public readonly string FilePath;
		/*
		## Two Parts of SplitPath

		One benefit of splitting a path is that we can make sure that the parts
		are always in correct format. The base part is extended so that it contains 
		an absolute folder path, and it is ensured that the last character is a 
		backslash. Conversely, the file path is checked to be relative path without 
		a root folder. If that is not the case, the constructor throws an exception.
		*/
		public SplitPath (string basePath, string filePath)
		{
			if (filePath != null && Path.IsPathRooted (filePath))
				throw new ArgumentException ("filePath has to be a relative path.");
			BasePath = WithLastBackslash (
				string.IsNullOrEmpty (basePath) ?
					Environment.CurrentDirectory :
					Path.GetFullPath (basePath));
			FilePath = filePath;
		}

		private static string WithLastBackslash (string dirPath) =>
			dirPath[dirPath.Length - 1] != Path.DirectorySeparatorChar ?
				dirPath + Path.DirectorySeparatorChar : dirPath;
		/*
		## Constructing a SplitPath

		There is a handful of constructor methods that help building a 
		SplitPath in various ways.

		First of all, it is possible to create a partial SplitPath by omitting 
		either part of the path. Partial paths can be later glued together to 
		create a valid path. The following two methods create a SplitPath given 
		either just a base path or a file path.
		*/
		public static SplitPath Base (string basePath) =>
			new SplitPath (basePath, null);

		public static SplitPath File (string file) =>
			new SplitPath (null, file);
		/*
		Another way to construct a SplitPath is to give a complete file path
		and a base path. The base path will be excluded from the full path to
		create the relative file path.
		*/
		public static SplitPath Split (string basePath, string fullPath) =>
			new SplitPath (basePath, fullPath.Substring (basePath.Length));
		/*
		Changing either part is also easy. Note that the structure is immutable,
		so it is never modified in-place. Instead a new structure is returned.
		*/
		public SplitPath WithBase (string basePath) =>
			new SplitPath (basePath, FilePath);

		public SplitPath WithFile (string filePath) =>
			new SplitPath (BasePath, filePath);
		/*
		It is possible to change the file extension as well.
		*/
		public SplitPath ChangeExtension (string newExtension) =>
			new SplitPath (BasePath, Path.ChangeExtension (FilePath, newExtension));
		/*
		## SplitPath Features

		If both parts of a SplitPath are null, it is considered to be empty.
		*/
		public bool IsEmpty =>
			BasePath == null && FilePath == null;
		/*
		The following properties can be used to test if one part of the path
		is missing.
		*/
		public bool IsBase =>
			BasePath != null && FilePath == null;

		public bool IsFile =>
			BasePath == null && FilePath != null;
		/*
		We are also wrapping some of the methods defined in `System.IO.Path` class
		to access other parts of the path: file name, extension, or directory name.
		*/
		public string FileName =>
			Path.GetFileName (FilePath);

		public string FileNameWithoutExtension =>
			Path.GetFileNameWithoutExtension (FilePath);

		public string Extension =>
			Path.GetExtension (FilePath);

		public string DirectoryName =>
			Path.GetDirectoryName (ToString ());
		/*
		In some cases it is necessary to know how deep the file path part is;
		for example, to create a relative file path inside a base directory.
		The method below counts how many directory separator characters there
		are in a file path, thus returning its depth.
		*/
		public int FilePathDepth =>
			FilePath.Count (c => c == Path.DirectorySeparatorChar);
		/*
		When we know the depth of a file path, we can construct a relative
		path pointing from the file to the base directory. This comes in handy 
		when we are creating links between generated HTML files.

		The following code just concatenates as many parent directory pointers
		(`../`) together as the depth of the file path indicates to construct
		a relative path to the base directory.
		*/
		public string RelativeFileRoot
		{
			get
			{
				var depth = FilePathDepth;
				var result = new StringBuilder ();
				for (int i = 0; i < depth; i++)
					result.Append (@"../");
				return result.ToString ();
			}
		}
		/*
		The overloaded `!` operator is a shorthand to the `ToString` method
		which concatenates the two parts of a SplitPath to form a full path 
		as a string.
		*/
		public override string ToString () =>
			BasePath + FilePath;

		public static string operator ! (SplitPath splitPath) =>
			splitPath.ToString ();
		/*
		Finally, we can combine two SplitPaths by taking the base path from the 
		first one and the file path from the second one. To make this operation
		more convenient to use it is implemented as the `+` operator.
		*/
		public static SplitPath operator + (SplitPath basePath, SplitPath filePath)
		{
			return new SplitPath (basePath.BasePath, filePath.FilePath);
		}
	}
}
