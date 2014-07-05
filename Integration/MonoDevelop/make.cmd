@cd /D %~dp0
@call ../../scripts/set-variables

%msbuild% MonoDevelop.SharpKit.csproj
