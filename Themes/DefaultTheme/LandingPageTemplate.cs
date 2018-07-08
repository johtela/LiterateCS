namespace DefaultTheme
{
	using LiterateCS.Theme;

	public partial class LandingPage : IPageTemplate
	{
		public PageParams Params { get; set; }

		public string Render ()
		{
			return TransformText ();
		}
	}
}
