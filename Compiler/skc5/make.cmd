@cd /D %~dp0
@call ../../scripts/set-variables

IF not "%1" == "release" (

dotnet msbuild skc5.csproj

) ELSE (

dotnet msbuild skc5.csproj /p:Configuration=Release /p:DebugSymbols=false /p:DebugType=None

)

