debug:
	xbuild /p:Configuration=net_4_5_Debug NRefactory/NRefactory.sln
	xbuild corex/corex.sln
	xbuild AjaxMin/AjaxMinDll/AjaxMinDll.sln
	xbuild SharpZipLib/src/ICSharpCode.SharpZLib.csproj

release:
	xbuild /p:Configuration=net_4_5_Release NRefactory/NRefactory.sln
	xbuild /p:Configuration=Release corex/corex.sln
	xbuild /p:Configuration=Release AjaxMin/AjaxMinDll/AjaxMinDll.sln
	xbuild /p:Configuration=Release SharpZipLib/src/ICSharpCode.SharpZLib.csproj

