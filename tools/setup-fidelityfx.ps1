param(
    [string]$Destination = "native/fidelityfx"
)

$ErrorActionPreference = "Stop"
$version = "1.1.4"
$url = "https://github.com/GPUOpen-LibrariesAndSDKs/FidelityFX-SDK/releases/download/v$version/FidelityFX-SDK-v$version.zip"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$destinationPath = Join-Path $root $Destination
$cache = Join-Path $root ".cache/fidelityfx-$version"
$zip = "$cache.zip"

New-Item -ItemType Directory -Force -Path $destinationPath | Out-Null

Write-Host "[FidelityFX] Downloading official SDK v$version..."
Invoke-WebRequest -Uri $url -OutFile $zip

if (Test-Path $cache) {
    Remove-Item -Recurse -Force $cache
}
Expand-Archive -Path $zip -DestinationPath $cache -Force

$dll = Get-ChildItem -Path $cache -Recurse -Filter "amd_fidelityfx_vk.dll" |
    Select-Object -First 1
if (-not $dll) {
    throw "Official SDK archive did not contain amd_fidelityfx_vk.dll."
}

Copy-Item -Force $dll.FullName (Join-Path $destinationPath "amd_fidelityfx_vk.dll")
Write-Host "[FidelityFX] Installed Vulkan runtime: $($dll.FullName)"
Write-Host "[FidelityFX] Target: $destinationPath"

Remove-Item -Force $zip
