call set-variables.bat

cd ../external
%git% submodule foreach git pull origin master
cd ../scripts

@echo.
@echo please use "-am" option, when commiting.
@echo Example:
@echo git commit -am "Updated submodules"