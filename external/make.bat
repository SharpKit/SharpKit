call ../scripts/set-variables.bat

%msbuild% NRefactory/ICSharpCode.NRefactory.CSharp/ICSharpCode.NRefactory.CSharp.csproj
%msbuild% corex/src/corex/corex.csproj
%msbuild% AjaxMin/AjaxMinDll/AjaxMinDll.csproj
%msbuild% aws-sdk-net/AWSSDK_DotNet45/AWSSDK_DotNet45.csproj
