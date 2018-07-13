---
Template: landing
_Jumbotron: >
    # Literate Programming in C#

    Produce stylish, interactive documentation for your C# projects using
    [literate programming](https://en.wikipedia.org/wiki/Literate_programming). 
    Write your documentation using [markdown](https://en.wikipedia.org/wiki/Markdown) 
    and compile it to a fully functional web site that can be published on 
    [GitHub](https://github.com).
---

<div class="row">
<div class="col-md-6">

## Step 1: Write Your Documentation

Embed your documentation inside code comments or put it into separate markdown 
files. All the formatting features of markdown are available to you. 
[Markdig](https://github.com/lunet-io/markdig) library is used to convert the 
markdown to HTML. It offers a lot of useful extensions from 
[MathJax](https://www.mathjax.org/) formulas to 
[mermaid](https://knsv.github.io/mermaid/) diagrams. 

</div>
<div class="col-md-6">
<p><img src="images/Diagram.png" class="img-responsive center-block" /></p>
</div>
</div>

<div class="row">
<div class="col-md-6">
<p><img src="images/FrontMatter.png" class="img-responsive center-block" /></p>
</div>
<div class="col-md-6">

## Step 2: Customize the Output

Generate either raw markdown files or standalone web sites. Using themes 
and styles you can customize the appearance of the pages. Include a [YAML](http://yaml.org/) 
front matter in your source files to pass parameters to the site generator.

</div>
</div>

<div class="row">
<div class="col-md-6">

## Step 3: Add Table of Contents

A table of contents file defines the structure of your documentation. A TOC 
file can be automatically generated and updated. The outputted web pages 
include a navigation pane and navigation buttons to jump from one page to 
another.

</div>
<div class="col-md-6">
<p><img src="images/Navigation.png" class="img-responsive center-block" /></p>
</div>
</div>

<div class="row">
<div class="col-md-6">
<p><img src="images/Code.png" class="img-responsive center-block" /></p>
</div>
<div class="col-md-6">

## Step 4: Generate the Documentation

The documentation "weaver" uses [Roslyn](https://github.com/dotnet/roslyn) to 
parse and analyze your source code. Syntactic and semantic information provided 
by Roslyn is used for syntax-highlighting, and for adding cross-references and 
type information to the code blocks. You can jump to the definition of a symbol 
by clicking it, or inspect its type by hovering over it.

</div>
</div>

<div class="row">
<div class="col-md-6">

## Step 5: Publish Your Docs

If your code resides under GitHub, you can publish it under 
[GitHub Pages](https://pages.github.com/) by turning on a single option in the 
project settings. Just generate your documentation under the `docs` folder,
and turn it on. Done!

</div>
<div class="col-md-6">
<p><img src="images/GitHubPages.png" class="img-responsive center-block" /></p>
</div>
</div>

<div class="row">
<div class="col-md-3">
<i class="fa fa-cloud-download fa-5x pull-right"></i>
</div>
<div class="col-md-6">

## Give It a Try!

The tool is packaged as a NuGet package and is installed as a global tool. Installation
instructions can be found... . You can also clone the
[repository](https://github.com/johtela/LiterateProgramming) and build the tool
from the sources.

<a class="btn btn-default" href="https://github.com/johtela/LiterateProgramming/releases" role="button">Download &raquo;</a>
</div>
</div>
