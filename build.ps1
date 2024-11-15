#!/bin/pwsh

Set-Location -Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

cd external
& ./build.ps1 $args[0]
cd ..

cd Compiler
& ./build.ps1 $args[0]
cd ..

cd SDK
& ./build.ps1 $args[0]
cd ..
