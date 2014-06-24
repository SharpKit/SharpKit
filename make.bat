call scripts/set-variables.bat

cd external
call make.bat
cd ..

cd Compiler/skc5
call make.bat
cd ../..

cd Compiler/MSBuild
call make.bat
cd ../..
