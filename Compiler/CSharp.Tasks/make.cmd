@cd /D %~dp0
@call ../../scripts/set-variables

IF not "%1" == "release" (

%msbuild% CSharp.Tasks.csproj

) ELSE (

%msbuild% /p:Configuration=Release CSharp.Tasks.csproj

)

