#########################
###### PSAKE BUILD ######
#########################

# psake build definition
properties {
	$base_dir = resolve-path .
	$build_dir = "$base_dir\build"
	$source_dir = "$base_dir"
    $nuget_packages_dir = "$up\.nuget\packages"
	$global:config = "debug"
}

$up = [System.Environment]::ExpandEnvironmentVariables("%UserProfile%")
$msbuild = "C:\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe"

function getNunit3TestRunner(){
    $testRunners = @(gci $nuget_packages_dir -rec -filter nunit3-console.exe)
    if ($testRunners.Length -ne 1)
    {
        throw "Expected to find 1 nunit3-console.exe, but found $($testRunners.Length)."
    }

    $testRunner = $testRunners[0].FullName
    $testRunner
}

task default -depends local
task local -depends compile
task reset {
    get-childitem -include bin,obj,project.lock.json -recurse -force |remove-item -recurse -force
}
task clean {
    rd "$base_dir\build" -recurse -force  -ErrorAction SilentlyContinue | out-null
}

# SVG BUILD
task configrelease {
    $global:config = "release"
}
task installNuget {
    $targetNugetExe = "$base_dir\Nuget\nuget.exe"

    if(-not (Test-Path $targetNugetExe)){
        $sourceNugetExe = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
        Invoke-WebRequest $sourceNugetExe -OutFile $targetNugetExe
    }
    Set-Alias nuget $targetNugetExe -Scope Global -Verbose  
}
task compileSvg -depends clean, installNuget {
    exec { & $base_dir\nuget\Nuget.exe restore $source_dir\svg.sln }
    exec { & $msbuild /t:Clean /t:Build /p:Configuration=$config /v:q /p:NoWarn=1591 /nologo $source_dir\svg.sln }
}
task compileEditor -depends clean, installNuget {
    exec { & $base_dir\nuget\Nuget.exe restore $source_dir\svg.editor.sln }
    exec { & $msbuild /t:Clean /t:Build /p:Configuration=$config /v:q /p:NoWarn=1591 /nologo $source_dir\svg.editor.sln }
}
task packageSvg -depends compileSvg, configrelease {
    &"$pwd\Nuget\package_svg.ps1"
}
task packageEditor -depends compileEditor, configrelease {
    &"$pwd\Nuget\package_editor.ps1"
    &"$pwd\Nuget\package_editor_views.ps1"
    &"$pwd\Nuget\package_editor_forms.ps1"
}

# BUILD
task compile -depends compileEditor
task package -depends packageSvg, packageEditor