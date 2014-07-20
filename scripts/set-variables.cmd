@echo off
if exist "C:\Program Files (x86)\Git\bin\git.exe" (
	set git="c:\Program Files (x86)\Git\bin\git.exe"
) else (
	set git="git"
)

set msbuild="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
set ngen="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\ngen.exe"
@echo on
