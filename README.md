[![Build status](https://ci.appveyor.com/api/projects/status/vv7v46pprias7hel?svg=true)](https://ci.appveyor.com/project/johtela/literateprogramming)

# Literate Programming in C#

[Literate Programming](https://en.wikipedia.org/wiki/Literate_programming) is a 
programming methodology that helps writing and maintaining software by uniting 
code and documentation. It makes writing documentation less arduous task, and 
improves the quality and utility of the written documentation. Some argue that
it also improves the quality of the produced code by forcing the author to 
present it in such way that it is understandable to the readers.

This project is an attempt to make literate programming easy and fun for C#
programmers. The full description of the project is available in: 

[https://johtela.github.io/LiterateProgramming/](https://johtela.github.io/LiterateProgramming/)

Please browse through the web site to get an idea what literate programming 
is and how the `csweave` tool helps you practicing it.

## Installation

Download the latest version of the `csweave` tool from the 
[releases page](https://github.com/johtela/LiterateProgramming/releases).

The software is provided as a zip package that contains everything inside a
single folder. Just extract the package to any directory. Typically the 
executables reside in the local application data directory, that is, 
`C:\Users\<username>\AppData\Local\csweave`.

To make running the tool easier, add the executable directory to your $PATH 
environment variable. You can do this from the Control Panel, or temporarily 
amend the search path in command prompt:
```
set PATH=%PATH%;C:\Users\<username>\AppData\Local\csweave
```
In PowerShell the same can be accomplished with:
```
$env:Path += ";C:\Users\<username>\AppData\Local\csweave"
```
Now you can run the tool without specifying the executable path. Most likely
you want to run it inside your solution directory.

## Bug Reports & Feedback

If you find a bug or want to suggest a feature, you can submit an 
[issue](https://github.com/johtela/LiterateProgramming/issues). Just
set the label reflecting the type of the issue accordingly.

If you want to contribute to the development, let me know. All help
is appreciated.