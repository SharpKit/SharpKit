#!/bin/pwsh

Set-Location -Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

if($IsLinux) {

    if ($args[0] -ne "release") {
        msbuild CSharp.Tasks.csproj
    } else {
        msbuild CSharp.Tasks.csproj /p:Configuration=Release
    }

} else {

    if ($args[0] -ne "release") {
        dotnet msbuild CSharp.Tasks.csproj
    } else {
        dotnet msbuild CSharp.Tasks.csproj /p:Configuration=Release
    }

}


