param([string]$version = "1.0.0")
$rootPath = $pwd;
$targetNugetExe = "$rootPath\nuget.exe"

if(-not (Test-Path $targetNugetExe)){
    $sourceNugetExe = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
    Invoke-WebRequest $sourceNugetExe -OutFile $targetNugetExe
}
Set-Alias nuget $targetNugetExe -Scope Global -Verbose

nuget pack Svg.Editor.Views.nuspec /Version $version

#nuget pack "..\Svg.Editor.Views.Droid\Svg.Editor.Views.Droid.csproj"
#nuget pack "..\Svg.Editor.Views.Droid\Svg.Editor.Views.Droid.csproj" -Symbols
#nuget pack "..\Svg.Editor.Views.iOS\Svg.Editor.Views.iOS.csproj"
#nuget pack "..\Svg.Editor.Views.iOS\Svg.Editor.Views.iOS.csproj" -Symbols
#nuget pack "..\Svg.Editor.Views.UWP\Svg.Editor.Views.UWP.csproj"
#nuget pack "..\Svg.Editor.Views.UWP\Svg.Editor.Views.UWP.csproj" -Symbols