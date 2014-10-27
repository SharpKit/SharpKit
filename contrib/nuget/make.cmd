@cd /D %~dp0
@call ../../scripts/set-variables

powershell -File get-nuget.ps1

call pack-core.bat
call pack-jquery.bat
call pack-jquery-ui.bat
