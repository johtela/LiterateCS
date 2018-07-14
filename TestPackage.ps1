function Assert ([scriptblock] $condition)
{
    if (!(& $condition))
    {
        throw "Condition {$condition} does not hold."
    }
}

$rootdir = split-path -parent $MyInvocation.MyCommand.Definition
    
Write-Host "Installing the package."
dotnet tool install -g LiterateCS --add-source $rootdir\LiterateCS\bin\Release

Write-Host "Generating HTML documentation for LiterateCS."
Set-Location $rootdir
[Environment]::CurrentDirectory = $rootdir
& {
    $ErrorActionPreference = "SilentlyContinue"
    & literatecs LiterateCS\*.cs LiterateCS.Theme\*.cs *.md -s LiterateCS.sln -o docs -f html -tv
}
Assert { $LASTEXITCODE -eq 0 }
Assert { Test-Path docs }
Assert { Test-Path docs\*.html }
Assert { Test-Path docs\LiterateCS\*.html }
Assert { Test-Path docs\LiterateCS.Theme\*.html }
Assert { Test-Path docs\bootstrap }
Assert { Test-Path docs\css\*.min.css }
Assert { Test-Path docs\font-awesome }
Assert { Test-Path docs\Images\*.png }
Assert { Test-Path docs\mermaid\*.css }
Assert { Test-Path docs\sidebar\*.min.css }
Assert { Test-Path docs\syntax-highlight\*.min.css }

Write-Host "Uninstalling the package."
dotnet tool uninstall -g LiterateCS

Pop-Location
