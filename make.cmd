@echo off
cd /D %~dp0
call scripts/set-variables

cd external
call make %1
cd ..

cd Compiler/skc5
call make %1
cd ../..

cd Compiler/CSharp.Tasks
call make %1
cd ../..

cd SDK
call make %1
cd ..

cd Integration/MonoDevelop
call make %1
cd ../..
