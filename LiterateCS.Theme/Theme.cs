/*
# Theme Base Class

Theme assemblies have to include a class which inherits from the Theme class 
and implements a few abstract methods. The Theme base class defines the 
interface through which the HtmlGenerator class can use the theme.
*/
namespace LiterateCS.Theme
{
	using System.IO;

	public abstract class Theme
	{
		/*
		The first property that needs to be implemented returns the list of page 
		templates available in the theme. A theme can have as many templates
		as it likes, but it must define at least one: the default template. The
		name of the default template should be "default".
		*/
		public abstract string[] AvalailablePageTemplates { get; }
		/*
		The second property that themes must implement is the path to the asset
		directory. LiterateCS copies all the assets automatically to the output
		directory. If you want to customize this behavior, you can override the
		CopyAssets method below.
		*/
		public abstract string AssetDir { get; }
		/*
		The third method to implement performs the rendering of the page using
		the template specified. The	default template is used, if `pageTemplate`
		is `null`. Page parameters are set by the HtmlGerenator and passed to 
		the render method.
		*/
		public abstract string RenderPage (string pageTemplate, PageParams pageParams);
		/*
		Theme class provides a default implementation for copying the auxiliary
		files needed by the HTML pages. The default implementation copies all the
		files under the asseet directory.
		*/
		public virtual void CopyAssets (string assetDir, string outputDir)
		{
			DirHelpers.CopySubDirectories (assetDir, outputDir, "*", true);
			foreach (var file in DirHelpers.Dir (assetDir, "*", true))
				File.Copy (file, file.Replace (assetDir, outputDir), true);
		}
	}
}