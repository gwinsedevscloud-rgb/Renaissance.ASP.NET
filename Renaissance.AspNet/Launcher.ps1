# Double-click Launcher.bat to start Renaissance and open the web UI in your browser.
# Uses the existing start-dev.ps1 script without modifying it.

$ErrorActionPreference = "Stop"

$Root = $PSScriptRoot
$StartScript = Join-Path $Root "start-dev.ps1"
$WebUrl = "http://localhost:5281"
$MaxWaitSeconds = 90

if (-not (Test-Path $StartScript)) {
    Write-Host "Could not find start-dev.ps1 in: $Root" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Starting Renaissance..." -ForegroundColor Cyan
Write-Host ""

& $StartScript -Restart

Write-Host ""
Write-Host "Waiting for the web UI..." -ForegroundColor Cyan

$deadline = (Get-Date).AddSeconds($MaxWaitSeconds)
$ready = $false

while ((Get-Date) -lt $deadline) {
    try {
        $response = Invoke-WebRequest -Uri $WebUrl -UseBasicParsing -TimeoutSec 3 -ErrorAction Stop
        if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 500) {
            $ready = $true
            break
        }
    }
    catch {
        # Server not ready yet.
    }

    Start-Sleep -Seconds 1
}

Write-Host ""

if ($ready) {
    Start-Process $WebUrl
    Write-Host "Renaissance is running." -ForegroundColor Green
    Write-Host "  Web UI: $WebUrl"
    Write-Host "  API:    http://localhost:5280/swagger"
    Write-Host ""
    Write-Host "Default login: admin / Admin@123"
    Write-Host "Close the API and Web terminal windows to stop the app."
}
else {
    Write-Host "Servers were started, but the web UI did not respond in time." -ForegroundColor Yellow
    Write-Host "Open manually: $WebUrl"
    exit 1
}
