@echo off
if exist "C:\Program Files (x86)\Git\bin\git.exe" (
	set git="c:\Program Files (x86)\Git\bin\git.exe"
) else (
	set git="git"
)

set ngen="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\ngen.exe"

if exist "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" (
	set msbuild="C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
) else (
    set msbuild="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
)


@echo on
