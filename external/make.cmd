@cd /D %~dp0
@call ../scripts/set-variables

IF not "%1" == "release" (

dotnet msbuild 												    /p:Configuration=net_4_0_Release cecil/Mono.Cecil.csproj    /verbosity:minimal
dotnet msbuild NRefactory/NRefactory.sln                       	/p:Configuration=net_4_5_Debug 								/verbosity:minimal
dotnet msbuild corex/corex.sln 																								/verbosity:minimal
dotnet msbuild AjaxMin/AjaxMinDll/AjaxMinDll.sln 																			/verbosity:minimal
dotnet msbuild SharpZipLib/src/ICSharpCode.SharpZLib.csproj 																/verbosity:minimal
dotnet msbuild octokit.net/Octokit/Octokit-Mono.csproj 																		/verbosity:minimal
 
) ELSE (

dotnet msbuild cecil/Mono.Cecil.csproj                          /p:Configuration=net_4_0_Release /p:DebugSymbols=false /p:DebugType=None /verbosity:minimal
dotnet msbuild NRefactory/NRefactory.sln                        /p:Configuration=net_4_5_Release /p:DebugSymbols=false /p:DebugType=None /verbosity:minimal
dotnet msbuild corex/corex.sln                                  /p:Configuration=Release         /p:DebugSymbols=false /p:DebugType=None /verbosity:minimal
dotnet msbuild AjaxMin/AjaxMinDll/AjaxMinDll.sln                /p:Configuration=Release         /p:DebugSymbols=false /p:DebugType=None /verbosity:minimal
dotnet msbuild SharpZipLib/src/ICSharpCode.SharpZLib.csproj     /p:Configuration=Release         /p:DebugSymbols=false /p:DebugType=None /verbosity:minimal
dotnet msbuild octokit.net/Octokit/Octokit-Mono.csproj	        /p:Configuration=Release         /p:DebugSymbols=false /p:DebugType=None /verbosity:minimal

)
