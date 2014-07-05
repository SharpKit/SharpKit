@cd /D %~dp0
@call ../../scripts/set-variables

IF not "%1" == "release" (

%msbuild% skc5.csproj

) ELSE (

%msbuild% /p:Configuration=Release skc5.csproj

)

