SET OutputDir=c:\temp\a
SET CustomTargetsFile=%CD%\custom.targets
del %OutputDir%\ /q /s
rmdir %OutputDir%\ /q /s

cd ..

@echo Building external libraries
cd external
call make release
cd..

@echo Building Compiler
cd Compiler
call make release
cd ..

@echo Building SDK
cd SDK
call make release
cd ..

@echo Building File Structure
mkdir %OutputDir%\zip\Files\NET\
mkdir %OutputDir%\zip\Files\Application\Assemblies\
xcopy Compiler\skc5\bin\*         %OutputDir%\zip\Files\NET\
xcopy Compiler\CSharp.Tasks\bin\*      %OutputDir%\zip\Files\NET\
xcopy SDK\Defs\bin\*              %OutputDir%\zip\Files\Application\Assemblies\ /s
xcopy SDK\Frameworks\JsClr\bin\*  %OutputDir%\zip\Files\Application\JsClr\bin\ /s
xcopy SDK\Frameworks\JsClr\res\*  %OutputDir%\zip\Files\Application\JsClr\res\ /s
del %OutputDir%\zip\*.pdb /q /s

@echo Creating ZIP-File
cd scripts
pushd %OutputDir%
cd zip
call 7z a ..\Files.zip
cd..
rem rmdir zip /q /s
rem rmdir bin /q /s
popd

cd ..
copy %OutputDir%\Files.zip Installer\Installer\res\
cd Installer\Installer\
call msbuild4 Installer.csproj /p:Configuration=Release /p:DebugSymbols=false /p:DebugType=None /verbosity:minimal
cd ..\..
copy Installer\Installer\bin\SharpKitSetup.exe %OutputDir%\SharpKitSetup.exe
cd scripts

