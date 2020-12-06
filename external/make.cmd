@cd /D %~dp0
@call ../scripts/set-variables

IF not "%1" == "release" (

dotnet build 												    /p:Configuration=net_4_0_Release cecil/Mono.Cecil.csproj    /verbosity:minimal
dotnet build NRefactory/NRefactory.sln                       	/p:Configuration=net_4_5_Debug 								/verbosity:minimal
dotnet build corex/corex.sln 																								/verbosity:minimal
dotnet build AjaxMin/AjaxMinDll/AjaxMinDll.sln 				    															/verbosity:minimal
dotnet build SharpZipLib/src/ICSharpCode.SharpZLib.csproj 		    														/verbosity:minimal
dotnet build octokit.net/Octokit/Octokit-Mono.csproj 																		/verbosity:minimal
 
) ELSE (

dotnet build cecil/Mono.Cecil.csproj                          /p:Configuration=net_4_0_Release /p:DebugSymbols=false /p:DebugType=None /verbosity:minimal
dotnet build NRefactory/NRefactory.sln                        /p:Configuration=net_4_5_Release /p:DebugSymbols=false /p:DebugType=None /verbosity:minimal
dotnet build corex/corex.sln                                  /p:Configuration=Release         /p:DebugSymbols=false /p:DebugType=None /verbosity:minimal
dotnet build AjaxMin/AjaxMinDll/AjaxMinDll.sln                /p:Configuration=Release         /p:DebugSymbols=false /p:DebugType=None /verbosity:minimal
dotnet build SharpZipLib/src/ICSharpCode.SharpZLib.csproj     /p:Configuration=Release         /p:DebugSymbols=false /p:DebugType=None /verbosity:minimal
dotnet build octokit.net/Octokit/Octokit-Mono.csproj	      /p:Configuration=Release         /p:DebugSymbols=false /p:DebugType=None /verbosity:minimal

)
