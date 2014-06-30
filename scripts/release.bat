SET OutputDir=c:\temp\a
SET CustomTargetsFile=%CD%\custom.targets
del %OutputDir%\ /q /s
rd %OutputDir%\ /q /s
SET MSBuildExtraParams=/p:CustomBeforeMicrosoftCSharpTargets="%CustomTargetsFile%" /verbosity:minimal
cd ..

@echo Building Compiler
cd Compiler
msbuild.lnk Compiler.sln /p:OutputDir="%OutputDir%\Compiler" %MSBuildExtraParams%
cd ..

pause

cd SDK

@echo Building SDK\Defs
cd Defs
msbuild.lnk Defs.sln /p:OutputDir="%OutputDir%\SDK\Defs" /p:DontCopyLocalReferences=true %MSBuildExtraParams%
cd ..

pause

@echo Building SDK\Frameworks
cd Frameworks
msbuild.lnk Frameworks.sln /p:OutputDir="%OutputDir%\SDK\Frameworks" /p:DontCopyLocalReferences=true %MSBuildExtraParams%
cd ..

cd ..


cd scripts
pause

@echo Building SDK\Frameworks
cd %OutputDir%
md zip\Files\NET
copy Compiler\skc5\* zip\Files\NET\
copy Compiler\MSBuild\* zip\Files\NET\
copy SDK\Defs\* zip\Application\Assemblies\ /s


pause