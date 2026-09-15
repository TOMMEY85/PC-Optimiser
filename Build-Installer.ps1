$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$app = Join-Path $root 'PCOptimizer.App'
$installer = Join-Path $root 'Installer\PCOptimizer.iss'
$iconDir = Join-Path $app 'Assets'
New-Item -ItemType Directory -Force -Path $iconDir | Out-Null
$iconPath = Join-Path $iconDir 'PCOptimizer-v8.ico'
$iconBase64 = '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'
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
