/*
# Table of Contents Data Structure

TOC is provided by the user in a YAML file. When it is read, a data structure
is created  that represents the TOC data. This data structure is defined below.
It is a basic hierarchical list structure that consist of nodes that correspond
to sections in the documentation.
*/
namespace LiterateCS.Theme
{
	using System.Collections.Generic;
	/*
	## Table of Contents Class

	Table of contents has two lists inside it. The first one contains the
	top-level sections in a hierarchical structure. The second one is a computed
	field that contains the same sections in a flat list. The flattened list
	helps us find quickly the previous and next entry for a given section.
	*/
	public class Toc
	{
		private List<Section> _contents;
		private List<Section> _flattened;
		/*
		The method that flattens the contents traverses the data structure
		recursively and add each section to the flat list.
		*/
		private void FlattenContents (List<Section> sections, List<Section> flattened)
		{
			foreach (var section in sections)
			{
				if (section.File != null)
					flattened.Add (section);
				if (section.Subs != null)
					FlattenContents (section.Subs, flattened);
			}
		}
		/*
		The Contents property can be set outside the class. When it is set, 
		the flattened list is recomputed. The Flattened property is read-only.
		*/
		public List<Section> Contents
		{
			get { return _contents; }
			set
			{
				_contents = value;
				_flattened = new List<Section> ();
				FlattenContents (_contents, _flattened);
			}
		}

		public List<Section> Flattened
		{
			get { return _flattened; }
		}
		/*
		When we have the flattened list of topics available, we can find
		the next and previous section in the list easily. We by just search for
		the index of the section and return the next or previous based on the
		index. If we cannot find the section, or we are at the beginning or end 
		of the list, we return `null`.
		*/
		public Section NextSection (Section current)
		{
			var i = _flattened.IndexOf (current);
			return i < 0 || i >= _flattened.Count - 1 ?
				null :
				_flattened[i + 1];
		}

		public Section PreviousSection (Section current)
		{
			var i = _flattened.IndexOf (current);
			return i <= 0 ?	null : _flattened[i - 1];
		}
	}
	/*
	## Section Class

	The Section object represents an individual entry in TOC. There might be a
	file that corresponds to the section, but not necessarily. A section can be
	used also to logically group pages under a same topic without providing a 
	separate page for it. In this case you cannot jump to the section, but you 
	can still see it in TOC.
	*/
	public class Section
	{
		/*
		The Page property is the title of the section. It is a short string 
		containing usually the first heading in the file.
		*/
		public string Page { get; set; }
		/*
		The relative path to the _source_ file is stored in the File property. 
		It is naturally unique within TOC. There cannot be two sections pointing 
		to the same file. It can be `null`, though. As explained above, there
		can be sections without files.
		*/
		public string File { get; set; }
		/*
		You can provide additional description about the section. This data is
		optional and may be omitted when showing the TOC.
		*/
		public string Desc { get; set; }
		/*
		Subsections are stored in a list. If there are no subsections under the
		current section the property is `null`.
		*/
		public List<Section> Subs { get; set; }
		/*
		Custom string conversion is provided mainly for debugging purposes.
		*/
		public override string ToString ()
		{
			return string.Format ("{0} -> {1}", Page, File);
		}
	}
}