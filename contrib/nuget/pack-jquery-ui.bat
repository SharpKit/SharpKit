xcopy /Y ..\..\SDK\Defs\bin\SharpKit.jQueryUI.dll jquery-ui\lib\
xcopy /Y ..\..\SDK\Defs\bin\SharpKit.jQueryUI.xml jquery-ui\lib\

cd output
..\NuGet.exe pack ..\jquery-ui\metadata.nuspec
cd ..
