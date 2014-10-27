param($installPath, $toolsPath, $package, $project)
#[Reflection.Assembly]::Load([System.IO.FILE]::ReadAllBytes("C:\Users\sebastian.sharpkit\Documents\Visual Studio 2010\Projects\psSharpKit\bin\Debug\SharpKit.NuGet.dll"))
#[SharpKit.NuGet.Installer]::Unregister($project.FullName);

. "$toolsPath/lib.ps1"
unregister $project