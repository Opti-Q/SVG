param([string]$version = "1.0.0")
$rootPath = $pwd;
$targetNugetExe = "$rootPath\nuget.exe"

if(-not (Test-Path $targetNugetExe)){
    $sourceNugetExe = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
    Invoke-WebRequest $sourceNugetExe -OutFile $targetNugetExe
}
Set-Alias nuget $targetNugetExe -Scope Global -Verbose



nuget pack Svg.Editor.nuspec /Version $version
#nuget pack "..\Svg.Editor.Core\Svg.Editor.Core.csproj"
#nuget pack "..\Svg.Editor.Core\Svg.Editor.Core.csproj" -Symbols