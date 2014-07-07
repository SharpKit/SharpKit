@cd /D %~dp0
@call ../scripts/set-variables

IF not "%1" == "release" (

%msbuild% /p:Configuration=net_4_0_Release cecil/Mono.Cecil.csproj
%msbuild% NRefactory/NRefactory.sln
%msbuild% corex/corex.sln
%msbuild% AjaxMin/AjaxMinDll/AjaxMinDll.sln
rem %msbuild% aws-sdk-net/AWSSDK_DotNet35/AWSSDK_DotNet35.sln
%msbuild% SharpZipLib/src/ICSharpCode.SharpZLib.csproj

) ELSE (

%msbuild% cecil/Mono.Cecil.csproj                         /p:Configuration=net_4_0_Release /p:DebugSymbols=false /p:DebugType=None
%msbuild% NRefactory/NRefactory.sln                       /p:Configuration=net_4_5_Release /p:DebugSymbols=false /p:DebugType=None
%msbuild% corex/corex.sln                                 /p:Configuration=Release         /p:DebugSymbols=false /p:DebugType=None
%msbuild% AjaxMin/AjaxMinDll/AjaxMinDll.sln               /p:Configuration=Release         /p:DebugSymbols=false /p:DebugType=None
rem %msbuild% aws-sdk-net/AWSSDK_DotNet35/AWSSDK_DotNet35.sln /p:Configuration=Release         /p:DebugSymbols=false /p:DebugType=None
%msbuild% SharpZipLib/src/ICSharpCode.SharpZLib.csproj    /p:Configuration=Release         /p:DebugSymbols=false /p:DebugType=None

)
