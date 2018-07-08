namespace DefaultTheme
{
	using LiterateCS.Theme;

	public partial class DefaultPage : IPageTemplate
	{
		public PageParams Params { get; set; }

		public string Render ()
		{
			return TransformText ();
		}
	}
}
