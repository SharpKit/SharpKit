#!/bin/pwsh

Set-Location -Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)
#& ../scripts/set-variables.ps1

if ($IsWindows) {

    if ($args[0] -ne "release") {
        dotnet msbuild cecil/Mono.Cecil.csproj                         -p:Configuration=net_4_0_Release -verbosity:minimal
        dotnet msbuild NRefactory/NRefactory.sln                       -p:Configuration=net_4_5_Debug   -verbosity:minimal
        dotnet msbuild corex/corex.sln                                                                  -verbosity:minimal
        dotnet msbuild AjaxMin/AjaxMinDll/AjaxMinDll.sln                                                -verbosity:minimal
        dotnet msbuild SharpZipLib/src/ICSharpCode.SharpZLib.csproj                                     -verbosity:minimal
        dotnet msbuild octokit.net/Octokit/Octokit-Mono.csproj                                          -verbosity:minimal
    } else {
        dotnet msbuild cecil/Mono.Cecil.csproj                         -p:Configuration=net_4_0_Release -p:DebugSymbols=false -p:DebugType=None -verbosity:minimal
        dotnet msbuild NRefactory/NRefactory.sln                       -p:Configuration=net_4_5_Release -p:DebugSymbols=false -p:DebugType=None -verbosity:minimal
        dotnet msbuild corex/corex.sln                                 -p:Configuration=Release         -p:DebugSymbols=false -p:DebugType=None -verbosity:minimal
        dotnet msbuild AjaxMin/AjaxMinDll/AjaxMinDll.sln               -p:Configuration=Release         -p:DebugSymbols=false -p:DebugType=None -verbosity:minimal
        dotnet msbuild SharpZipLib/src/ICSharpCode.SharpZLib.csproj    -p:Configuration=Release         -p:DebugSymbols=false -p:DebugType=None -verbosity:minimal
        dotnet msbuild octokit.net/Octokit/Octokit-Mono.csproj         -p:Configuration=Release         -p:DebugSymbols=false -p:DebugType=None -verbosity:minimal
    }

} else {

    nuget restore corex/corex.sln

    if ($args[0] -ne "release") {
        msbuild cecil/Mono.Cecil.csproj                         -p:Configuration=net_4_0_Release -verbosity:minimal
        msbuild NRefactory/NRefactory.sln                       -p:Configuration=net_4_5_Debug   -verbosity:minimal
        msbuild corex/corex.sln                                                                  -verbosity:minimal
        msbuild AjaxMin/AjaxMinDll/AjaxMinDll.sln                                                -verbosity:minimal
        xbuild SharpZipLib/src/ICSharpCode.SharpZLib.csproj                                     -verbosity:minimal
        msbuild octokit.net/Octokit/Octokit-Mono.csproj                                          -verbosity:minimal
    } else {
        msbuild cecil/Mono.Cecil.csproj                         -p:Configuration=net_4_0_Release -p:DebugSymbols=false -p:DebugType=None -verbosity:minimal
        msbuild NRefactory/NRefactory.sln                       -p:Configuration=net_4_5_Release -p:DebugSymbols=false -p:DebugType=None -verbosity:minimal
        msbuild corex/corex.sln                                 -p:Configuration=Release         -p:DebugSymbols=false -p:DebugType=None -verbosity:minimal
        msbuild AjaxMin/AjaxMinDll/AjaxMinDll.sln               -p:Configuration=Release         -p:DebugSymbols=false -p:DebugType=None -verbosity:minimal
        xbuild SharpZipLib/src/ICSharpCode.SharpZLib.csproj    -p:Configuration=Release         -p:DebugSymbols=false -p:DebugType=None -verbosity:minimal
        msbuild octokit.net/Octokit/Octokit-Mono.csproj         -p:Configuration=Release         -p:DebugSymbols=false -p:DebugType=None -verbosity:minimal
    }
}