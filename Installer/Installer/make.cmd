@cd /D %~dp0
@call ../../scripts/set-variables

IF not "%1" == "release" (

%msbuild% Installer.csproj

) ELSE (

%msbuild% Installer.csproj /p:Configuration=Release

)

