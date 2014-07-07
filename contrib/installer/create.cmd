@echo off
cd /D %~dp0

cd ../..

call make release
call Installer/make release

cd ../contrib/installer
