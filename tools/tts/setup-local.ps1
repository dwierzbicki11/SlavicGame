$ErrorActionPreference = "Stop"

$Root = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$Venv = Join-Path $Root ".venv-tts"
$ChatterboxVersion = if ($env:CHATTERBOX_VERSION) { $env:CHATTERBOX_VERSION } else { "0.1.7" }

function Resolve-Python {
    if ($env:PYTHON) {
        return [pscustomobject]@{ Exe = $env:PYTHON; Prefix = @() }
    }

    if (Get-Command py -ErrorAction SilentlyContinue) {
        foreach ($minor in @("3.13", "3.12", "3.11", "3.10")) {
            & py "-$minor" -c "import sys" 2>$null
            if ($LASTEXITCODE -eq 0) {
                return [pscustomobject]@{ Exe = "py"; Prefix = @("-$minor") }
            }
        }
    }

    if (Get-Command python -ErrorAction SilentlyContinue) {
        return [pscustomobject]@{ Exe = "python"; Prefix = @() }
    }

    throw "Python 3.10-3.13 was not found."
}

$Python = Resolve-Python

function Invoke-SelectedPython {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments)

    & $Python.Exe @($Python.Prefix) @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Python command failed."
    }
}

Invoke-SelectedPython -c @"
import sys
v = sys.version_info
assert (3, 10) <= v[:2] < (3, 14), (
    f"Unsupported Python {v.major}.{v.minor}.{v.micro}. "
    "SlavicGame local TTS supports Python 3.10-3.13."
)
print(f"[tts] Using Python {v.major}.{v.minor}.{v.micro}")
"@

$SelectedMinor = (& $Python.Exe @($Python.Prefix) -c "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')").Trim()

$VenvPython = Join-Path $Venv "Scripts/python.exe"
if (Test-Path $VenvPython) {
    $VenvMinor = (& $VenvPython -c "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')").Trim()
    if ($VenvMinor -ne $SelectedMinor) {
        Write-Host "[tts] Recreating .venv-tts: old Python=$VenvMinor, selected Python=$SelectedMinor"
        Remove-Item -Recurse -Force $Venv
    }
}

if (-not (Test-Path $VenvPython)) {
    Write-Host "[tts] Creating virtual environment at $Venv"
    Invoke-SelectedPython -m venv $Venv
}

& $VenvPython -m pip install --upgrade pip setuptools wheel
if ($LASTEXITCODE -ne 0) { throw "Could not update pip/setuptools/wheel." }

$PyMinor = (& $VenvPython -c "import sys; print(sys.version_info.minor)").Trim()
if ([int]$PyMinor -lt 13) {
    & $VenvPython -m pip install --upgrade "numpy>=1.24,<2"
} else {
    & $VenvPython -m pip install --upgrade "numpy>=2"
}
if ($LASTEXITCODE -ne 0) { throw "Could not install NumPy." }

Write-Host "[tts] Installing chatterbox-tts==$ChatterboxVersion ..."
& $VenvPython -m pip install --upgrade "chatterbox-tts==$ChatterboxVersion"
if ($LASTEXITCODE -ne 0) { throw "Could not install Chatterbox TTS." }

Write-Host "[tts] Verifying runtime imports..."
& $VenvPython -c @"
import sys
from importlib.metadata import version
import numpy
import torch
import torchaudio
from chatterbox.mtl_tts import ChatterboxMultilingualTTS
print(f'[tts] Python:      {sys.version.split()[0]}')
print(f'[tts] Chatterbox:  {version("chatterbox-tts")}')
print(f'[tts] NumPy:       {numpy.__version__}')
print(f'[tts] Torch:       {torch.__version__}')
print(f'[tts] Torchaudio:  {torchaudio.__version__}')
print(f'[tts] CUDA:        {torch.cuda.is_available()}')
print('[tts] Import test: OK')
"@
if ($LASTEXITCODE -ne 0) { throw "Chatterbox import verification failed." }

Write-Host ""
Write-Host "Local Chatterbox TTS is ready."
Write-Host "The first spoken spell downloads the open model weights; later lines use the local model/cache."
Write-Host "Optional voice references: assets/voice/reference/"
