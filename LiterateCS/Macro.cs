/*
# Macros

Macros are sequences of blocks that can be inserted inside markdown files. They 
correspond to C# [regions](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives/preprocessor-region). 
To create a macro, a block of code should be surrounded with the `#region` 
directive. The name of the region is used as the macro name. Since macro names 
should be unique, it is disallowed to have two regions with the same name.

The Macro class defines the data structure that is used to store macros. It 
implements the `IEnumerable<BlockList>` interface to allow iterating though
the blocks belonging to a macro.
*/
namespace LiterateProgramming
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	public class Macro : IEnumerable<BlockList>
	{
		/*
		## Fields of a Macro
		Macros are stored in a linked list in the same way as [BlockList](BlockList.html)
		data structures. The only pieces of data we need to store for a macro are 
		its name, the first block belonging to it `Start`, and the end marker block 
		`End`. The end marker is the first block that does _not_ belong to the macro. 
		If the end marker is `null`, then the macro includes all the blocks until 
		the end of a file.
		*/
		public string Name { get; private set; }
		public BlockList Start { get; set; }
		public BlockList End { get; set; }
		/*
		Macros are stored in a static dictionary, whose keys are macro names.
		*/
		private static Dictionary<string, Macro> _macros =
			new Dictionary<string, Macro> ();
		/*
		The constructor is private. Macros should be created with the static 
		`Add` method.
		*/
		private Macro (string name, BlockList start, BlockList end)
		{
			Name = name;
			Start = start;
			End = end;
		}
		/* 
		## IEnumerable Implementation
		Iterator is used to implement `IEnumerable<BlockList>` interface.
		*/
		public IEnumerator<BlockList> GetEnumerator ()
		{
			for (var block = Start; 
				block != null & block != End; 
				block = block.Next)
				yield return block;
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		/*
		## Creating a Macro
		The `Add` method is used to create a new macro. The method throws an
		exception if a macro with the given name is already registered.
		*/
		public static Macro Add (string name, BlockList start, BlockList end)
		{
			name = name.Trim ();
			if (_macros.ContainsKey (name))
				throw new ArgumentException (string.Format (
					"Macro '{0}' already exists.", name));
			var result = new Macro (name, start, end);
			_macros.Add (name, result);
			return result;
		}
		/*
		## Retrieving a Macro
		The `Get` method is used to find and return a macro. It throws
		an exception, if a macro with the given name is not found.
		*/
		public static Macro Get (string name)
		{
			name = name.Trim ();
			if (!_macros.ContainsKey (name))
				throw new ArgumentException (string.Format (
					"Macro '{0}' not found.", name));
			return _macros[name];
		}
	}
}
