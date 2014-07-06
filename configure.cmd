@cd /D  %~dp0
@call scripts/set-variables

%git% submodule init
%git% submodule update
