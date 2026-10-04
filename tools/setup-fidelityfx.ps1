param(
    [string]$Destination = "native/fidelityfx"
)

$ErrorActionPreference = "Stop"
$version = "1.1.4"
$url = "https://raw.githubusercontent.com/GPUOpen-LibrariesAndSDKs/FidelityFX-SDK/v$version/PrebuiltSignedDLL/amd_fidelityfx_vk.dll"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$destinationPath = Join-Path $root $Destination
$target = Join-Path $destinationPath "amd_fidelityfx_vk.dll"

New-Item -ItemType Directory -Force -Path $destinationPath | Out-Null

Write-Host "[FidelityFX] Downloading official signed Vulkan runtime v$version..."
Invoke-WebRequest -Uri $url -OutFile $target

if (-not (Test-Path $target)) {
    throw "FidelityFX Vulkan runtime download failed."
}

$size = (Get-Item $target).Length
if ($size -lt 1000000) {
    Remove-Item -Force $target
    throw "Downloaded FidelityFX Vulkan runtime is unexpectedly small."
}

Write-Host "[FidelityFX] Installed official amd_fidelityfx_vk.dll ($size bytes)"
Write-Host "[FidelityFX] Target: $target"
Write-Host "[FidelityFX] Build the game again, then select UPSCALER -> FSR3 in the graphics menu. SLAVICGAME_FSR3=1 remains a developer override."
