$ErrorActionPreference = "Stop"

$Root = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$Venv = Join-Path $Root ".venv-tts"
$Python = if ($env:PYTHON) { $env:PYTHON } else { "py" }

if ($Python -eq "py") {
    & py -3.11 -c "import sys; assert sys.version_info[:2] == (3,11)"
    & py -3.11 -m venv $Venv
} else {
    & $Python -c "import sys; assert sys.version_info[:2] == (3,11), 'Chatterbox setup currently expects Python 3.11'"
    & $Python -m venv $Venv
}

$VenvPython = Join-Path $Venv "Scripts/python.exe"
& $VenvPython -m pip install --upgrade pip
& $VenvPython -m pip install chatterbox-tts

Write-Host ""
Write-Host "Local Chatterbox TTS installed."
Write-Host "The first spoken line downloads the open model weights; later use can work from the local cache."
Write-Host "Optional: add legally usable WAV references in assets/voice/reference/ for character/emotion voices."
