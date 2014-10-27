xcopy /Y ..\..\SDK\Defs\bin\SharpKit.jQuery.dll jquery\lib\
xcopy /Y ..\..\SDK\Defs\bin\SharpKit.jQuery.xml jquery\lib\

cd output
..\NuGet.exe pack ..\jquery\metadata.nuspec
cd ..
