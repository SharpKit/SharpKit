param($installPath, $toolsPath, $package, $project)
#[Reflection.Assembly]::Load([System.IO.FILE]::ReadAllBytes("C:\Users\sebastian.sharpkit\Documents\Visual Studio 2010\Projects\psSharpKit\bin\Debug\SharpKit.NuGet.dll"))
#[SharpKit.NuGet.Installer]::Register($project.FullName, $package.Version.Version);

. "$toolsPath/lib.ps1"

register $project $package $toolsPath
