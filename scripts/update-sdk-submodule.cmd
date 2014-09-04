@cd /D %~dp0
@call set-variables

@cd ..
%git% submodule foreach git pull origin master
@cd scripts

@echo.
@echo submodules are now updated.
@echo please use "-am" option, when commiting.
@echo Example:
@echo git commit -am "Updated submodules"
