/*
# Working with MSBuild Files

Loading and compiling a solution with Roslyn involves quite a lot of steps
making the task somewhat complicated. Therefore, we encapsulate the complex 
stuff to a static helper class that hides the intermediate steps.

**Side note:** I am not entirely sure that the way we are managing project
references is the preferred approach. The documentation of Roslyn and especially
[MSBuildWorkspace](http://www.coderesx.com/roslyn/html/334B7980.htm) is really 
lacking on this topic. Anyhow, the current implementation is the only way I 
could get the assembly references at least mostly working. The compiler still
complains about missing references, but the semantic information seems to be
available nevertheless.
*/
namespace LiterateProgramming
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.IO;
	using Microsoft.CodeAnalysis;
	using Microsoft.Build.Construction;
	using LiterateCS.Theme;

	public static class MSBuildHelpers
	{
		/*
		## Opening a Solution

		Parsing the solution file reads it into memory allowing us to access
		the information contained in it.
		*/
		public static SolutionFile OpenSolution (string filename)
		{
			return SolutionFile.Parse (Path.GetFullPath (filename));
		}
		/*
		## Loading and Compiling Projects

		Once we have opened a solution we can load and compile the projects 
		inside it. For that we need both the `Solution` object defined by
		Roslyn as well as the `SolutionFile` object defined in 
		`Microsoft.Build.Construction`.
		*/
		public static IEnumerable<Project> LoadProjectsInSolution (Solution solution, 
			SolutionFile solutionFile)
		{
			/*
			First we enumerate the projects in solution using Roslyn.
			*/
			foreach (var proj in solution.Projects)
			{
				/*
				For the current project we will find the corresponding entry
				in the `SolutionFile`. This is done by a helper function defined
				below.
				*/
				var projFile = FindProjectInSolution (proj, solutionFile);
				/*
				The project root directory is the folder where the project file
				resides.
				*/
				var projRoot = ProjectRootElement.Open (projFile.AbsolutePath);
				/*
				Next we need to get the .NET framework version the project is
				compiled against. This is needed to locate the folder where the
				reference assemblies are. They contain facades for .NET framework 
				classes that are loaded automatically. Roslyn needs the facades 
				to access the metadata associated to the classes.
				*/
				//var frameworkVersion = ProjectProperty (projRoot, "TargetFrameworkVersion");
				//var refAssyDir = Path.Combine (
				//	Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86),
				//	@"Reference Assemblies\Microsoft\Framework\.NETFramework\",
				//	frameworkVersion);
				/*
				Solution directory is the current directory when a project is
				compiled. If there are assembly references with relative file 
				paths, we need to concatenate them to the solution directory to 
				get their absolute paths.
				*/
				var solutionDir = Path.GetDirectoryName (solution.FilePath);
				/*
				Now we can add references to the facade assemblies.
				*/
				//var p = AddFacadeReferences (proj, refAssyDir);
				/*
				And then we can add the "normal" references.
				*/
				//p = AddReferences (p, projRoot, refAssyDir);
				/*
				Finally we can compile the project and yield it out for enumeration. 
				If there are compilation errors, they will be outputted to the console 
				window. As noted above, getting referencing errors does not necessarily 
				mean that the required semantic information is not available in compiled 
				project.
				*/
				var p = proj;
				var diag = p.GetCompilationAsync ().Result.GetDiagnostics ();
				foreach (var msg in diag)
					Console.Error.WriteLine (msg);
				yield return p;
			}
		}
		/*
		## Adding References

		References that are not loaded automatically by the .NET framework are listed 
		in a project file. We need to find items in the file with type "Reference" and 
		determine where the corresponding assemblies are located on disk.
		*/
		private static Project AddReferences (Project proj, ProjectRootElement projRoot, 
			string refAssyDir)
		{
			var references = projRoot.Items.Where (i => i.ItemType == "Reference");
			var projectDir = Path.GetDirectoryName (proj.FilePath);
			foreach (var reference in references)
			{
				/*
				If there is an attribute name "HintPath" for the metadata, we can
				use it to get the assembly file path. If the hint path is relative, 
				we concatenate it with the solution root path. Otherwise we use it 
				as-is.
				*/
				var hintPath = reference.Metadata.FirstOrDefault (pme =>
					pme.Name == "HintPath");
				if (hintPath != null)
					AddMetadataReference (ref proj,
						Path.IsPathRooted (hintPath.Value) ?
							hintPath.Value :
							Path.Combine (projectDir, hintPath.Value));
				/* 
				When no hint path is defined, we assume that the assembly
				is part of .NET framework and can be found in the reference 
				assembly directory.
				*/
				else
					AddMetadataReference (ref proj,
						Path.Combine (refAssyDir, reference.Include + ".dll"));
			}
			return proj;
		}
		/*
		Facade assemblies hide under a subdirectory of the reference assembly
		path. For good measure, we load all the facades and the mscorlib.dll.
		*/
		private static Project AddFacadeReferences (Project proj, string refAssyDir)
		{
			var facadesDir = Path.Combine (refAssyDir, "Facades");
			AddMetadataReference (ref proj, Path.Combine (refAssyDir, "mscorlib.dll"));
			foreach (var dllPath in DirHelpers.Dir (facadesDir, "*.dll", false))
				AddMetadataReference (ref proj, dllPath);
			return proj;
		}
		/*
		One common gotcha with Roslyn is that most functions that modify a project 
		return a new copy of the project instead of mutating it. So we need to 
		update the variable pointing to project every time we add a new reference.

		The function below makes adding a reference bit terser in the code.
		*/
		private static void AddMetadataReference (ref Project proj, string dllPath)
		{
			proj = proj.AddMetadataReference (MetadataReference.CreateFromFile (dllPath));
		}
		/*
		Another helper function finds a projects in a solution file by its name.
		*/
		public static ProjectInSolution FindProjectInSolution (Project project, 
			SolutionFile solutionFile) =>
			solutionFile.ProjectsInOrder.First (pis => pis.ProjectName == project.Name);
		/*
		And the last helper function finds a specific property in a project file.
		*/
		public static string ProjectProperty (ProjectRootElement root, string propertyName) =>
			root.Properties.First (ppe => ppe.Name == propertyName).Value;
	}
}