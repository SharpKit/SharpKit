#!/bin/pwsh

Set-Location -Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

if($IsLinux) {

    if ($args[0] -ne "release") {
        msbuild skc5.csproj
    } else {
        msbuild skc5.csproj /p:Configuration=Release /p:DebugSymbols=false /p:DebugType=None
    }

} else {

    if ($args[0] -ne "release") {
        dotnet msbuild skc5.csproj
    } else {
        dotnet msbuild skc5.csproj /p:Configuration=Release /p:DebugSymbols=false /p:DebugType=None
    }

}


