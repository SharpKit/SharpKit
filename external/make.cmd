@cd /D %~dp0
@call ../scripts/set-variables

IF not "%1" == "release" (

%msbuild% /p:Configuration=net_4_0_Release cecil/Mono.Cecil.csproj
%msbuild% NRefactory/NRefactory.sln
%msbuild% corex/corex.sln
%msbuild% AjaxMin/AjaxMinDll/AjaxMinDll.sln
%msbuild% aws-sdk-net/AWSSDK_DotNet45/AWSSDK_DotNet45.sln
%msbuild% SharpZipLib/src/ICSharpCode.SharpZLib.csproj

) ELSE (

%msbuild% /p:Configuration=net_4_0_Release cecil/Mono.Cecil.csproj
%msbuild% /p:Configuration=net_4_5_Release NRefactory/NRefactory.sln
%msbuild% /p:Configuration=Release corex/corex.sln
%msbuild% /p:Configuration=Release AjaxMin/AjaxMinDll/AjaxMinDll.sln
%msbuild% /p:Configuration=Release aws-sdk-net/AWSSDK_DotNet45/AWSSDK_DotNet45.sln
%msbuild% /p:Configuration=Release SharpZipLib/src/ICSharpCode.SharpZLib.csproj

)
