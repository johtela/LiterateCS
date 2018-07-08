namespace DefaultTheme
{
	using System;
	using LiterateCS.Theme;
	using System.Collections.Generic;
	using System.Linq;

	public class DefaultTheme : Theme
	{
		private static Dictionary<string, Func<IPageTemplate>> _templates =
			new Dictionary<string, Func<IPageTemplate>>
			{
				{"default", () => new DefaultPage () },
				{"landing", () => new LandingPage () }
			};

		public override string[] AvalailablePageTemplates =>
			_templates.Keys.ToArray ();

		public override string RenderPage (string pageTemplate, PageParams pageParams)
		{
			var templName = pageTemplate ?? "default";
			if (!_templates.ContainsKey (templName))
				throw new ArgumentException ("Unsupported page template: " + pageTemplate);
			var template = _templates[templName] ();
			template.Params = pageParams;
			return template.Render();
		}
	}
}
