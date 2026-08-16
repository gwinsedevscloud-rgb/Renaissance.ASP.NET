# Launch Renaissance for hospital LAN deployment.
# Binds API and Web to all network interfaces (0.0.0.0) so other PCs on the LAN can connect.
# Usage:
#   .\start-lan.ps1
#   .\start-lan.ps1 -Build

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

function Get-LanIpAddress {
    $addresses = Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue |
        Where-Object {
            $_.IPAddress -notlike "127.*" -and
            ($_.IPAddress -like "192.168.*" -or $_.IPAddress -like "10.*" -or $_.IPAddress -like "172.16.*" -or $_.IPAddress -like "172.17.*" -or $_.IPAddress -like "172.18.*" -or $_.IPAddress -like "172.19.*" -or $_.IPAddress -like "172.2*.*" -or $_.IPAddress -like "172.30.*" -or $_.IPAddress -like "172.31.*")
        } |
        Sort-Object InterfaceMetric |
        Select-Object -ExpandProperty IPAddress -First 1

    if ($addresses) { return $addresses }
    return "127.0.0.1"
}

function Start-LanWindow {
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

if (-not (Test-Path $ApiDir)) { throw "API project not found: $ApiDir" }
if (-not (Test-Path $WebDir)) { throw "Web project not found: $WebDir" }

if ($Build) {
    Write-Host "Building solution..."
    Push-Location $Root
    try {
        dotnet build
        if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE" }
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

$lanIp = Get-LanIpAddress

Write-Host "Starting Renaissance API (LAN)..."
Start-LanWindow -Title "Renaissance API (LAN)" -WorkingDirectory $ApiDir -Command "dotnet run --launch-profile lan"

Write-Host "Waiting for API to start..."
Start-Sleep -Seconds 4

Write-Host "Starting Renaissance Web UI (LAN)..."
Start-LanWindow -Title "Renaissance Web (LAN)" -WorkingDirectory $WebDir -Command "dotnet run --launch-profile lan"

Write-Host ""
Write-Host "Renaissance is running for LAN access."
Write-Host ""
Write-Host "  On this server:"
Write-Host "    Web UI: http://localhost:5281"
Write-Host "    API:    http://localhost:5280"
Write-Host ""
Write-Host "  From other computers on the hospital network:"
Write-Host "    Web UI: http://${lanIp}:5281"
Write-Host ""
Write-Host "Configure the staff URL under Admin -> Settings after first login."
Write-Host "Ensure Windows Firewall allows inbound TCP on ports 5280 and 5281."
Write-Host ""
Write-Host "Default login: admin / Admin@123"
Write-Host "Close the API and Web terminal windows to stop the app."
