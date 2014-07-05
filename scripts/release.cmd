SET OutputDir=c:\temp\a
SET CustomTargetsFile=%CD%\custom.targets
del %OutputDir%\ /q /s
rd %OutputDir%\ /q /s
SET MSBuildExtraParams=/p:CustomBeforeMicrosoftCSharpTargets="%CustomTargetsFile%" /verbosity:minimal
rem  /p:DebugSymbols=false /p:DebugType=None
cd ..

cd external

cd AjaxMin\AjaxMinDll
call msbuild4 AjaxMinDll.sln /p:Configuration=Release /verbosity:minimal /p:DebugSymbols=false /p:DebugType=None
cd ..\..\

cd Corex
call msbuild4 Corex.sln /p:Configuration=Release /verbosity:minimal /p:DebugSymbols=false /p:DebugType=None
cd..

cd cecil
call msbuild4 Mono.Cecil.sln /p:Configuration=net_4_0_release /p:DontCopyLocalReferences=true /p:DebugSymbols=false /p:DebugType=None %MSBuildExtraParams%
cd..

cd NRefactory
call msbuild4 NRefactory.sln /p:Configuration=net_4_5_release /p:DontCopyLocalReferences=true /p:DebugSymbols=false /p:DebugType=None %MSBuildExtraParams%
cd..

cd..

@echo Building Compiler
cd Compiler
call msbuild4 Compiler.sln /p:Configuration=Release /p:DebugSymbols=false /p:DebugType=None %MSBuildExtraParams%
cd ..


cd SDK

@echo Building SDK\Defs
cd Defs
call msbuild4 Defs.sln %MSBuildExtraParams%
cd ..


@echo Building SDK\Frameworks
cd Frameworks
call msbuild4 Frameworks.sln /p:DontCopyLocalReferences=true %MSBuildExtraParams%
cd ..

cd ..


@echo Building SDK\Frameworks
md %OutputDir%\zip\Files\NET\
md %OutputDir%\zip\Files\Application\Assemblies\
xcopy Compiler\skc5\bin\*         %OutputDir%\zip\Files\NET\
xcopy Compiler\MSBuild\bin\*      %OutputDir%\zip\Files\NET\
xcopy SDK\Defs\bin\*              %OutputDir%\zip\Files\Application\Assemblies\ /s
xcopy SDK\Frameworks\JsClr\bin\*  %OutputDir%\zip\Files\Application\JsClr\bin\ /s
xcopy SDK\Frameworks\JsClr\res\*  %OutputDir%\zip\Files\Application\JsClr\res\ /s
del %OutputDir%\zip\*.pdb /q /s

cd scripts
pushd %OutputDir%
cd zip
call 7z a ..\Files.zip
cd..
rem rd zip /q /s
rem rd bin /q /s
popd

cd ..
copy %OutputDir%\Files.zip Installer\Installer\res\
cd Installer\Installer\
call msbuild4 Installer.csproj /p:Configuration=Release /p:DebugSymbols=false /p:DebugType=None /verbosity:minimal
cd ..\..
copy Installer\Installer\bin\SharpKitSetup.exe %OutputDir%\SharpKitSetup.exe
cd scripts

