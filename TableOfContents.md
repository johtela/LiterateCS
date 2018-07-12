# Table of Contents File

To make it easier for the reader to find a specific topic the pages should 
be organized in a logical structure. The structure of the documentation is 
defined in the table of contents, or _TOC_, file. This file is used only when 
HTML output is selected. Markdown output  does not currently utilize the TOC.

## File Format

TOC is stored in a special file called `TOC.yml`. It is defined in [YAML](http://yaml.org/) 
format, as is the [front matter](FrontMatter.html). The contents of the file
is laid out in the following way:

    contents:
      - page: <title>
        file: <relative file path>
        desc: <description of the topic>

      - page: <title>
        desc: <description of the topic>
        subs:
          - page: <subtopic title>
            file: <relative file path>
            desc: <description of the topic>
            ...

      - page: <title>
        file: <relative file path>
        desc: <description of the topic>
        ...

The format is hierarchical. The first keyword is `contents:` under which the 
topics are listed. For each topic there is `page:` which contains the title
of the topic. `file:` specifies the relative file path to the source file
that contains the documentation. This information is optional, so it can be 
left out, if there is no file associated to the topic.

`desc:` contains a description string that can be optionally used in the
TOC. This field can also be left out, if it is not needed.

Subtopics can be added to any level by inserting them under the `subs:` keyword.
Exactly same fields are available to subtopics as for the main level topics.

## Example

Below is an excerpt from the TOC of this project. It serves as an example on
how a TOC file typically looks like:

    contents:
      - page: Introduction
        file: Index.md
        desc: Introduction to literate programming and to the `csweave` tool.

      - page: Command Line Options
        file: src\Options.cs
        desc: Lists the available command line options and input parameters.

      - page: Main Program
        file: src\Program.cs
        desc: Main program that initiates the document generation.

      - page: Themes
        desc: Customizing the HTML output.
        subs:
          - page: Directory Utilities
            file: CSWeave.Theme\DirHelpers.cs
            desc: Functions that help file/directory management.

          - page: Theme Base Class
            file: CSWeave.Theme\Theme.cs
            desc: Base class for themes to inherit from.

      - page: Front Matter
        file: FrontMatter.md
        desc: Defining document metadata.
