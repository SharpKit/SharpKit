#!/bin/pwsh

Set-Location -Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

# call ../scripts/set-variables

cd skc5
& ./build.ps1 $args[0]
cd ..

cd CSharp.Tasks
& ./build.ps1 $args[0]
cd ..