If (!(Test-Path "nuget.exe")){
    $client = new-object System.Net.WebClient
    $client.DownloadFile( "http://nuget.org/nuget.exe", "nuget.exe" )
}