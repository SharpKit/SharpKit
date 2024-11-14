@echo off
cd /D %~dp0
call scripts/set-variables

cd external
call build.ps1 %1
cd ..

cd Compiler
call build.ps1 %1
cd ..

cd SDK
call make %1
cd ..

cd Integration/MonoDevelop
call make %1
cd ../..
