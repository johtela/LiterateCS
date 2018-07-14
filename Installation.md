# Installation

LiterateCS runs on top of the [.NET Core 2.1]. It is distributed as a [Nuget] package
and deployed as [.NET Core global tool]. This makes it easy to install, uninstall, and
update. The technology for distributing command line tools as Nuget packages is quite
new but seems to work well in practice. 

## Prerequisites

Before installing LiterateCS, make sure that you have .NET Core SDK version 2.1 or newer
installed. If you don't have it already, you can dowload it from [here][.NET Core 2.1].

## Installing

To install LiterateCS, open a command prompt (PowerShell or classic version) and
type:

    dotnet tool install -g literatecs

This download the tool from <https://nuget.org> and installs it tool in the folder 
`$HOME/.dotnet/tools/.store/literatecs` by default. It also creates an executable
into `$HOME/.dotnet/tools` that should be already in your search path.

To verify that LiterateCS is correctly installed you can type:

    literatecs

without any arguments. This should print the help screen showing the available 
command line options.

To see what other global tools you have installed, you can run:

    dotnet tool list -g

This shows you all the currently installed .NET core global tools.

**Note**: Contrary to what the name "global tools" suggests, they are actually
user-specific. They are not visible to other users of the computer. This is
easy to understand when you know that the installation directory is under the
home directory.

## Uninstalling

Uninstallation is easy. Just type:

    dotnet tool uninstall -g literatecs

This removes all the files previously installed.

## Updating

Updating to a new version is also a simple matter:

    dotnet tool update -g literatecs

This checks for the newer version of the tool in [Nuget] repository and if one is 
found, downloads and installs it.

[.NET Core 2.1]: https://www.microsoft.com/net/download/dotnet-core/2.1#sdk-2.1.300
[Nuget]: https://nuget.org
[.NET Core global tool]: https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools
