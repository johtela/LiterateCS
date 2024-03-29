# Installation

LiterateCS runs on top of the [.NET 6]. It is distributed as a [Nuget] package
and deployed as [.NET global tool]. This makes it easy to install, uninstall, and
update. 

## Prerequisites

Before installing LiterateCS, make sure that you have **.NET SDK version 6.0 or 
newer** installed. If you don't have it already, you can dowload it from 
[here][.NET 6].

## Installing

To install LiterateCS, open a command prompt (PowerShell or classic version) and
type:

    > dotnet tool install -g literatecs

This downloads the tool from [nuget.org][Nuget] and installs it in the folder 
`$HOME/.dotnet/tools/.store/literatecs` by default. It also creates an executable
into `$HOME/.dotnet/tools` that should be already in your search path.

To verify that LiterateCS is correctly installed you can type:

    > literatecs --help

This should print the help screen showing the available command line options.

To see what other global tools you have installed, you can run:

    > dotnet tool list -g

This shows you all the currently installed .NET core global tools.

**Note**: Contrary to what the name "global tools" suggests, they are actually
user-specific. They are not visible to other users of the computer. This is
obvious when you know that the installation directory is under the home directory.

## Uninstalling

Uninstallation is easy. Just type:

    > dotnet tool uninstall -g literatecs

This removes all the files previously installed.

## Updating

Updating to a new version is also a simple matter:

    > dotnet tool update -g literatecs

This checks for the newer version of the tool in [Nuget] repository and if one is 
found, downloads and installs it.

## Installing Project Template

Optionally, you can also install the [project template] for creating C# projects that
readily contain the metadata used by LiterateCS. To install the project template
use the following command:

    > dotnet new -i LiterateCS.Templates.LiterateLib.CSharp

After that you can create a new literate project with:

    > dotnet new literatelib

If you don't like the project template, you can uninstall with the `-u` option.

    > dotnet new -u LiterateCS.Templates.LiterateLib.CSharp

[.NET 6]: https://dotnet.microsoft.com/en-us/download/dotnet/6.0
[Nuget]: https://nuget.org
[.NET global tool]: https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools
[project template]: https://github.com/johtela/LiterateCS.Templates