@cd /D %~dp0
@call ../../scripts/set-variables

IF not "%1" == "release" (

dotnet build

) ELSE (

dotnet build /p:Configuration=Release CSharp.Tasks.csproj

)

