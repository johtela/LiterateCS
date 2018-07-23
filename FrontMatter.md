# Front Matter

All of the input files may contain a special block called _front matter_ 
at the very beginning of them. The front matter is a [YAML](http://yaml.org/)
block that contains metadata related to the input file or to the whole project.
It is a collection of string properties that is passed to the theme assembly 
for controlling the HTML generation.

Typically the front matter looks something like this:

    ---
    ProjectName: LiterateCS
    GitHub: https://github.com/johtela/LiterateCS
    Footer: Copyright (c) 2018 Tommi Johtela
    ShowDescriptionsInToc: true
    SyntaxHighlight: son-of-obsidian
    ---

It is delimited by three hyphens `---` and it must start on the first line of a 
markdown or C# file. No other text may appear before it, excluding the comment 
start token `/*` when it appears in a C# file.

## Default Settings

For some of the properties you probably want to set a same value for all files.
You don't need to duplicate these settings in every input file. Rather, define 
them in the `defaults.yml` file, so they will be processed before any of the 
input files are read. It is still possible to override properties on individual 
files, if you like.

The format of the `defaults.yml` file is exactly the same as for the front 
matter including the three hyphens `---` at the beginning and the end. These 
separate the [YAML documents](http://yaml.org/spec/1.2/spec.html#id2800132) 
inside the file.

## Available Properties

The list of available properties depends on the theme used. The properties 
available in the default theme are listed below. Property names and values 
are case-insensitive, so they can be typed using any convention: lowercase, 
uppercase, Camel case, Pascal case, etc.

### `Template` (string)

The name of the template used for producing the HTML page. This setting
is theme-dependent. In the default theme, the possible choices are:
* **Default** - Default template for documentation pages.
* **Landing** - Template for the landing page.

Depending on the template used, a property might be relevant or not. The list 
of supported parameters available in the default theme is shown in the table 
below. The columns indicate if a parameter is supported by a specific template.

| Parameter             | Default   | Landing  |
| --------------------- |:---------:|:--------:|
| ProjectName           |     *     |     *    |
| Logo                  |     *     |     *    |
| GitHub                |     *     |     *    |
| Download              |     *     |     *    |
| NuGet                 |     *     |     *    |
| License               |     *     |     *    |
| Footer                |     *     |     *    |
| MarkdownStyle         |     *     |          |
| ShowDescriptionInToc  |     *     |          |
| SyntaxHighlight       |     *     |          |
| UseDiagrams           |     *     |          |
| DiagramStyle          |     *     |          |
| UseMath               |     *     |          |
| Jumbotron             |           |     *    |

### `ProjectName` (string)

The name of the project that is shown in the toolbar of the HTML pages.

### `Logo` (string)

A relative path to the image logo that is shown along with the project name. The
image is scaled so that its height is 24 pixels.

### `GitHub` (URL)

The URL of the project in GitHub. The link is shown as a button in the toolbar.

### `Download` (URL)

The URL of the project download page. The link is shown as a button in the 
toolbar.

### `NuGet` (URL)

The URL of the project NuGet page. The link is shown as a button in the 
toolbar.

### `License` (string)

The name of the license file with the relative path and extension. The license 
file is assumed to be located under the project directory. The link is shown as 
a button in the toolbar.

### `Footer` (string)

The text shown in the footer of the HTML pages.

### `BootstrapStyle` (string)

The name of the stylesheet used with the Bootstrap library. The available 
styles in the default theme are (click a style to see its preview):
* [cerulean](https://bootswatch.com/3/cerulean/)
* [cosmo](https://bootswatch.com/3/cosmo/)
* [default](https://bootswatch.com/3/default/) - Default style.
* [flatly](https://bootswatch.com/3/flatly/)
* [journal](https://bootswatch.com/3/journal/)
* [lumen](https://bootswatch.com/3/lumen/)
* [paper](https://bootswatch.com/3/paper/)
* [readable](https://bootswatch.com/3/readable/)
* [sandstone](https://bootswatch.com/3/sandstone/)
* [simplex](https://bootswatch.com/3/simplex/)
* [spacelab](https://bootswatch.com/3/spacelab/)
* [united](https://bootswatch.com/3/united/)
* [yeti](https://bootswatch.com/3/yeti/)

### `MarkdownStyle` (string)

The name of the CSS style sheet that is used for markdown formatting. 
The available styles in the default theme are:
* **book** - The default style with serif fonts.
* **modern** - Uses the Lucida font family.
* **plain** - Simplistic style with sans serif fonts.

### `ShowDescriptionInToc` (true/false)

Control whether the description of a page is shown along with the page 
title in the table of contents shown in the side bar. Off by default.

### `SyntaxHighlight` (string)

The name of the CSS style sheet that is used for syntax highlighting. 
The available syntax highlighting schemes in the default theme are:

* **monokai** (default) - Dark theme used in many text editors. 
* **coding-horror** - Light theme inspired by the 
  [Coding Horror](https://blog.codinghorror.com/) blog.
* **solarized-light** - Solarized theme used in many text editors.
* **son-of-obsidian** - The most popular color scheme in 
  [studiostyles](https://studiostyl.es/).

### `UseDiagrams` (true/false)

Diagram support is off by default. This is because enabling it increases page 
loading times. If you want to include diagrams in your documentation, set this 
property to `true`. For more information refer to the
[Tips & Tricks](TipsAndTricks.html) page.

### `DiagramStyle` (string)

The name of CSS style file used by the [mermaid](http://knsv.github.io/mermaid/)
diagramming library. The default theme includes all style files that
come out-of-the-box with the library:

* **mermaid** (default) - Default light blue theme.
* **mermaid.dark** - A slightly darker blue theme.
* **mermaid.forest** - Green-colored theme.

### `UseMath` (true/false)

As with diagrams, support for mathematical formulas is disabled by default. The 
default theme uses the [MathJax](https://www.mathjax.org/) library to render
the formulas, and since it is a big library, loading it takes some time. To 
enable the math support, set this property to `true`. For more information refer 
to the [Tips & Tricks](TipsAndTricks.html) page.

### `_Jumbotron` (markdown)
The contents of the jumbotron pane of the landing page. All the parameters 
that start with underscore `_` will be translated to HTML automatically.
