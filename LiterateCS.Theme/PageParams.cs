/*
# Page Parameters

All the data LiterateCS will pass to the theme is contained inside a
PageParams object. This class defines the built-in parameters that are
always available, and the custom parameters specified in front matter.
*/
namespace LiterateCS.Theme
{
	using System.Collections.Generic;
	using System.IO;

	public class PageParams
	{
		/*
		## Custom Parameters

		You can specify any parameters inside front matter. How these will
		be used is up to the theme. The parameters are simple key-value pairs
		with string both as the key and value type. The parameters are stored
		in a dictionary that is initialized when a PageParams object is created.
		*/
		private Dictionary<string, string> _parameters;

		public PageParams ()
		{
			_parameters = new Dictionary<string, string> ();
		}
		/*
		The parameters are converted to lowercase before they are added to the
		dictionary. Parameter names are case-insensitive, so we need to do this
		in all the methods accessing the dictionary. If a parameter with the 
		same name already exists in the dictionary, its value is updated.
		*/
		public void Add (string name, string value)
		{
			name = name.ToLower ();
			if (_parameters.ContainsKey (name))
				_parameters[name] = value;
			else
				_parameters.Add (name, value);
		}
		/*
		Removing a parameter is also possible, although themes should not 
		generally do so.
		*/
		public void Remove (string name)
		{
			_parameters.Remove (name.ToLower ());
		}
		/*
		Two indexer methods can be used to access the custom parameters. The 
		first one takes two arguments: name and default value. If a parameter
		with a given name is not found, the default value is returned.
		*/
		public string this[string name, string defaultValue]
		{
			get
			{
				return _parameters.TryGetValue (name.ToLower (), out string result) ?
					result : defaultValue;
			}
		}
		/*
		The second version takes just name of the parameter, and returns it back, 
		if the parameter is not found.
		*/
		public string this[string name] =>
			this[name, name];
		/*
		## Built-in Properties

		The rest of the properties defined in the PageParams class are updated 
		for	each generated page. These are page-level parameters which are always 
		available.

		The Root property gives the relative path from the page location to the
		root directory of the website. For example, if the page we are generating 
		resides under directory `source\code\`, then Root would contain path 
		`..\..\`. Say you want to add a link to another page which resides under 
		directory `doc\`. Now you can to refer to it by adding the value of	the 
		Root property at the beginning of the path. The resulting path would be 
		`..\..\doc\`.
		
		So, the Root property allows us to create relative links between pages
		that work regardless of where the site resides on disk.	This is 
		quite handy since now the links work correctly when pages are viewed
		locally from disk as well as when they are accessed from a web server.
		*/
		public string Root { get; set; }
		/*
		The name of the currently processed file is stored in the property
		below. The name does not include a file extension.
		*/
		public string Filename { get; set; }
		/*
		Perhaps the most important data passed to the theme is the contents
		of the page which is provided in the property below. The property
		contains the documentation extracted from the source file in HTML
		format. Typically the theme will just insert this in the appropriate
		place on a page.
		*/
		public string Contents { get; set; }
		/*
		### TOC Related Properties

		The table of contents object is accessible through the Toc property.
		This object is always initialized, even when the user does not provide
		a TOC file. In that case the TOC is empty, and there are no sections
		in it.
		*/
		public Toc Toc { get; set; }
		/*
		When a TOC file is provided, and the current page can be found in it,
		then the following property contains a reference to the Section object 
		we are on currently. This information can be used to highlight which 
		section	in the TOC we are viewing.
		*/
		public Section CurrentSection { get; set; }
		/*
		The SectionPath helper function returns the relative path to section in
		the TOC from the current page. It is handy in generating links to TOC. 
		The method also changes the source file path to a relative URL by
		changing the extension to "html" and replacing backslashes with
		forward slashes.
		*/
		public string SectionPath (Section entry) =>
			entry.File == null ? 
				null :
				Path.Combine (Root, Path.ChangeExtension (entry.File, "html"))
					.Replace ('\\', '/');
	}
}
