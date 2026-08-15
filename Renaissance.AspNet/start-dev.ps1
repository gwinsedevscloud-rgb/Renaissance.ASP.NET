# Launch Renaissance API + Blazor UI for local development.
# Usage:
#   .\start-dev.ps1           # open API and Web in new terminal windows
#   .\start-dev.ps1 -Restart  # stop anything on 5280/5281 first (default)
#   .\start-dev.ps1 -Build    # dotnet build before starting

param(
    [switch]$Restart = $true,
    [switch]$Build
)

$ErrorActionPreference = "Stop"

$Root = $PSScriptRoot
$ApiDir = Join-Path $Root "backend\Renaissance.Api"
$WebDir = Join-Path $Root "frontend\Renaissance.Web"

function Stop-PortListener {
    param([int]$Port)

    Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue |
        ForEach-Object {
            Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
        }
}

function Start-DevWindow {
    param(
        [string]$Title,
        [string]$WorkingDirectory,
        [string]$Command
    )

    Start-Process powershell.exe -WorkingDirectory $WorkingDirectory -ArgumentList @(
        "-NoExit",
        "-Command",
        "`$Host.UI.RawUI.WindowTitle = '$Title'; $Command"
    ) | Out-Null
}

if (-not (Test-Path $ApiDir)) {
    throw "API project not found: $ApiDir"
}

if (-not (Test-Path $WebDir)) {
    throw "Web project not found: $WebDir"
}

if ($Build) {
    Write-Host "Building solution..."
    Push-Location $Root
    try {
        dotnet build
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet build failed with exit code $LASTEXITCODE"
        }
    }
    finally {
        Pop-Location
    }
}

if ($Restart) {
    Write-Host "Stopping existing listeners on ports 5280 and 5281..."
    Stop-PortListener 5280
    Stop-PortListener 5281
    Start-Sleep -Seconds 1
}

Write-Host "Starting Renaissance API..."
Start-DevWindow -Title "Renaissance API" -WorkingDirectory $ApiDir -Command "dotnet run --launch-profile http"

Write-Host "Waiting for API to start..."
Start-Sleep -Seconds 4

Write-Host "Starting Renaissance Web UI..."
Start-DevWindow -Title "Renaissance Web" -WorkingDirectory $WebDir -Command "dotnet run --launch-profile http"

Write-Host ""
Write-Host "Renaissance is launching."
Write-Host "  API:    http://localhost:5280/swagger"
Write-Host "  Web UI: http://localhost:5281"
Write-Host ""
Write-Host "Default login: admin / Admin@123"
Write-Host "Close the API and Web terminal windows to stop the app."
