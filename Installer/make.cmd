@echo off
cd /D %~dp0
call ../scripts/set-variables

cd Packager
call make %1
cd ..

call Packager\bin\Packager.exe
