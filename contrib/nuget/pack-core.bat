xcopy /Y ..\..\Compiler\skc5\bin\*.exe core\tools\
xcopy /Y ..\..\Compiler\skc5\bin\*.dll core\tools\
xcopy /Y ..\..\Compiler\skc5\bin\*.Tasks core\tools\
xcopy /Y ..\..\Compiler\skc5\bin\*.targets core\tools\

xcopy /Y ..\..\SDK\Defs\bin\SharpKit.Html.dll core\lib\
xcopy /Y ..\..\SDK\Defs\bin\SharpKit.Html.xml core\lib\

xcopy /Y ..\..\SDK\Defs\bin\SharpKit.JavaScript.dll core\lib\
xcopy /Y ..\..\SDK\Defs\bin\SharpKit.JavaScript.xml core\lib\

cd output
..\NuGet.exe pack ..\core\metadata.nuspec
cd ..
