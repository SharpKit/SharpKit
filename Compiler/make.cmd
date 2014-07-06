@echo off
cd /D %~dp0
call ../scripts/set-variables

cd skc5
call make %1
cd ..

cd CSharp.Tasks
call make %1
cd ..
