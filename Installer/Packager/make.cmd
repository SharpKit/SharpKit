@cd /D %~dp0
@call ../../scripts/set-variables

IF not "%1" == "release" (

%msbuild%

) ELSE (

%msbuild% /p:Configuration=Release

)

