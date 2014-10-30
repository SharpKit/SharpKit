
# Need to load MSBuild assembly if it's not loaded yet.
Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'

function  debug ($text) {
	#[System.IO.File]::AppendAllText("c:\test.txt","$text | ")
}

function  register ($project, $package, $toolsPath) {
	unregister($project)
	
    # Grab the loaded MSBuild project for the project
    $msbuild = getmsbuild $project

    # Add the import and save the project
	$relativePath = "`$(SolutionDir)\packages\SharpKit." + $package.Version.Version.ToString(3) + "\tools\SharpKit.Build.targets"
    $msbuild.Xml.AddImport($relativePath) | out-null
}

function getmsbuild ($project) {
	return [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($project.FullName) | Select-Object -First 1
}

function  unregister ($project) {
	$msbuild = getmsbuild $project

	foreach($import in $msbuild.Xml.Imports) {
		$el = $null
		if($import.Project.Contains("SharpKit")) {
			$el = $import
		}
	}
	
	if($el -ne $null){
		$msbuild.Xml.RemoveChild($el)
	}
	
}
