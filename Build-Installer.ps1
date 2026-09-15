$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$app = Join-Path $root 'PCOptimizer.App'
$installer = Join-Path $root 'Installer\PCOptimizer.iss'
$iconDir = Join-Path $app 'Assets'
New-Item -ItemType Directory -Force -Path $iconDir | Out-Null
$iconPath = Join-Path $iconDir 'PCOptimizer-v8.ico'
$iconBase64 = 'AAABAAEAICAAAAAAIAAiAQAAFgAAAIlQTkcNChoKAAAADUlIRIAAAAgAAAAIAgGAAAAc3p69AAAAOlJREFUeJzFl00WgyAMhJHnwp7NI7XLXsmz6c6u8KURAiQTnC2Y+RIiP9PyPs/woObSwLFijZatEQBtzONykDk3KTcRBXCs/7Ej0nz/lsdoTOoVpYkocyl2pESe5hwied4qMFpmgJ7s4QDc/PUZCGDN3AyAkgoAlX0IwlmgMS+NSb3RVQFN5rXGdO2Blr+iGQC57t0AHqVPamrCUjBN03HBN6Le3RDahEO3YpRgAJrs1QCIU9AEgDI3A1jNLwB+T/MUv3/eKlCDSOuvyT4X+wIo3dtzsppTr4k/Tr2XQXya0QmPPU5rH6D1A925SMrwN9LEAAAAAElFTkSuQmCC'
[IO.File]::WriteAllBytes($iconPath, [Convert]::FromBase64String($iconBase64))
Write-Host "Icône v8 générée : $iconPath" -ForegroundColor DarkGray


Write-Host '=== PC Optimizer v8.0.1 - Publication Windows x64 ===' -ForegroundColor Cyan
Push-Location $app
try {
    dotnet restore
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false
}
finally {
    Pop-Location
}

$isccCandidates = @(
    'C:\Program Files (x86)\Inno Setup 6\ISCC.exe',
    'C:\Program Files\Inno Setup 6\ISCC.exe',
    'C:\Program Files\Inno Setup 7\ISCC.exe',
    'C:\Program Files (x86)\Inno Setup 7\ISCC.exe'
)
$iscc = $isccCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $iscc) {
    Write-Host ''
    Write-Host 'Inno Setup est introuvable.' -ForegroundColor Yellow
    Write-Host 'Installe Inno Setup puis relance Build-Installer.bat.' -ForegroundColor Yellow
    exit 2
}

Write-Host "Inno Setup : $iscc" -ForegroundColor DarkGray
$publish = Join-Path $app 'bin\Release\net8.0-windows\win-x64\publish'
Copy-Item $iconPath (Join-Path $publish 'PCOptimizer-v8.ico') -Force
Write-Host "=== Création du programme d'installation ===" -ForegroundColor Cyan
& $iscc $installer

if ($LASTEXITCODE -ne 0) {
    throw "La compilation Inno Setup a échoué (code $LASTEXITCODE)."
}

$out = Join-Path $root 'Installer\Output\PCOptimizer-Setup-v8.0.1.exe'
Write-Host ''
Write-Host 'Installation créée :' -ForegroundColor Green
Write-Host $out -ForegroundColor Green
Start-Process explorer.exe -ArgumentList ("/select,""" + $out + """")
