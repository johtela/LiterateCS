namespace DefaultTheme
{
	using LiterateCS.Theme;

	interface IPageTemplate
	{
		PageParams Params { get; set; }
		string Render ();
	}
}
