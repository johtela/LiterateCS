/*
# Introduction

This is a simple example on how to create code with [literate comments]
(https://johtela.github.io/LiterateCS/Introduction.html#documentation-in-comments).
You can interleave documentation with the code and `literatecs` will compile a
HTML page from it.
*/
namespace LiterateLib
{
	/*
	Documentation can be added to any place in the code. Use multi-line comments
	delimited by `/*` and <code>&ast;/</code>. All the formatting options of markdown
	are available.
	*/
    public class Class1
    {
		/*
		You can also create macros by embedding code into regions. You can refer
		to the code snippets in the [markdown files](index.html).
		*/
		#region Add function
		public int Add (int x, int y) => x + y;
		#endregion
	}
}
