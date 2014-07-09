@cd /D  %~dp0
@call scripts/set-variables

%git% config core.filemode false

%git% submodule init
%git% submodule update
