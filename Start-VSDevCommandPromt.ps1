param (
    [Switch]$vs2019,
    [Switch]$vs2022
)

# By default look for vs2022
$version = "[17.0,18.0)"
if ($vs2019) { $version = "[15.0,16.0)" }

$displayName = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -version $version -property displayname
if (!$displayName) {
    Write-Host "Microsoft Visual Studio $version not found. These are the versions available:" -ForegroundColor Red
    & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -property displayname
    return
}
else {
    Write-Host $displayName
}

$installPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -version $version -property installationpath
if ($installPath) {
    Write-Host $installPath
}
else {
    Write-Host "Couldn't determine the Visual Studio path" -Foregroundcolor Red
    return
}

# The powershell module was first itroduced in vs2019
$ModulePath = Join-Path $installPath "Common7\Tools\Microsoft.VisualStudio.DevShell.dll"
Import-Module $ModulePath

Write-Host "DevShell: $displayName | $arch" -ForegroundColor Green
$null = Enter-VsDevShell -VsInstallPath $installPath -SkipAutomaticLocation -DevCmdArguments "-arch=amd64"