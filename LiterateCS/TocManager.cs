/*
# TOC Manager

Although the TOC data structure is defined in the [LiterateCS.Theme](../LiterateCS.Theme/Toc.html) 
assembly, the code that loads and saves TOC can be found here, under the TocManager class. 
Themes only need to read the table of contents; loading and updating it is handled by the 
main application.

## Dependencies

TocManager is a helper class that groups together the functionality related 
to the file format and serialization. It uses [YamlDotNet](https://github.com/aaubry/YamlDotNet)
library to parse and deserialize the TOC file. The format of the file is described
on a [separate page](../TableOfContents.html).
*/
namespace LiterateCS
{
	using LiterateCS.Theme;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using YamlDotNet.Serialization;
	using YamlDotNet.Serialization.NamingConventions;

	public class TocManager
	{
		/*
		## TOC Lifespan
		
		When a TocManager is created, a filename and the actual Toc object are
		provided. These are kept in private fields for the other methods to access. 
		If new sections are added, they are be kept in a separate list to keep the 
		Toc	object immutable during the operation of the program.
		*/
		private Toc _toc;
		private string _filename;
		private List<Section> _addedSections;
		/*
		The users of the class naturally need to access the Toc object. This is 
		possible through a read-only property.
		*/
		public Toc Toc => _toc;
		/*
		## Constructing TOC Manager

		There are two ways to create a TocManager. When a TOC file already exist, it
		is loaded and initialized automatically.
		*/
		public TocManager (Toc toc, string filename)
		{
			if (toc == null)
				throw new ArgumentNullException (nameof (toc));
			_toc = toc;
			_filename = filename;
			_addedSections = new List<Section> ();
		}
		/*
		If there is no file available, a parameterless constructor can be called to 
		create an empty TOC. In this case the TOC does not need to be updated, so 
		the `_addedSections` field can be left uninitialized.
		*/
		public TocManager ()
		{
			_toc = new Toc ();
			_toc.Contents = new List<Section> ();
		}
		/*
		## Loading TOC

		TOC file is loaded using the deserialization feature provided by YamlDotNet.
		We configure the deserializer so that it converts the names of the data items
		into camel case and then maps them to the properties of the Toc class. Then 
		we just open the file and feed it to the deserializer.
		*/
		public static TocManager Load (string yamlFile)
		{
			var deserializer = new DeserializerBuilder ()
				.WithNamingConvention (new CamelCaseNamingConvention ())
				.Build ();
			try
			{
				using (var input = File.OpenText (yamlFile))
					return new TocManager (deserializer.Deserialize<Toc> (input), 
						yamlFile);
			}
			/*
			The most complicated part of the loading is the error reporting, 
			which tries to give as much information for the error as possible.
			*/
			catch (Exception e)
			{
				var errorMsg = new StringBuilder ();
				errorMsg.AppendFormat ("Error while loading TOC file '{0}'\n", 
					yamlFile);
				errorMsg.AppendLine ("Error detail:");
				errorMsg.AppendLine (e.Message);
				if (e.InnerException != null)
					errorMsg.AppendLine (e.InnerException.Message);
				throw new InvalidOperationException (errorMsg.ToString ());
			}
		}
		/*
		## Adding New Sections

		Instead of adding a new entry to TOC while the documentation is being
		generated, we add it to a temporary list. This list is concatenated
		to the TOC before it is saved. If there is no file associated to the 
		TOC (in which case the temporary list is null), then we don't need to
		do anything.
		*/
		public void AddToToc (SplitPath path)
		{
			if (_addedSections == null)
				return;
			var section = FindSectionForFile (path.FilePath, Toc.Contents) ??
				_addedSections.FirstOrDefault (s => s.File == path.FilePath);
			if (section == null)
			{
				_addedSections.Add (new Section ()
				{
					Page = path.FileNameWithoutExtension,
					File = path.FilePath,
					Desc = "TODO! Add page description."
				});
			}
		}
		/*
		## Saving TOC

		We could use the serialization functionality of YamlDotNet to write the TOC
		back to a file, but it is actually simpler just add the new entries as text.
		Serialization has its own quirks which we should work around, if we would
		like to use it. The new entries are always at the end of the file, so we
		can just open the file in the append mode and write the sections into it.
		*/
		public void Save ()
		{
			if (_filename == null || _addedSections == null || 
				_addedSections.Count == 0)
				return;
			using (var output = File.AppendText (_filename))
				foreach (var section in _addedSections)
				{
					output.WriteLine ();
					output.WriteLine ("  - page: " + section.Page);
					output.WriteLine ("    file: " + section.File);
					output.WriteLine ("    desc: " + section.Desc);
				}
		}
		/*
		## Checking If a Section Exists

		To check whether a file is not in TOC already we need to traverse the TOC tree
		recursively. The function below does just that. If a given file is not found 
		in the tree, `null` is returned.
		*/
		public static Section FindSectionForFile (string file, List<Section> sections)
		{
			foreach (var section in sections)
			{
				if (section.File == file)
					return section;
				if (section.Subs != null)
				{
					var subSection = FindSectionForFile (file, section.Subs);
					if (subSection != null)
						return subSection;
				}
			}
			return null;
		}
	}
}
