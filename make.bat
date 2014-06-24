@echo off
cd /D %~dp0
call scripts/set-variables

cd external
call make
cd ..

cd Compiler/skc5
call make
cd ../..

cd Compiler/MSBuild
call make
cd ../..

cd Integration/MonoDevelop
call make
cd ../..
