@cd /D %~dp0
@call ../../scripts/set-variables

%msbuild% skc5.csproj
