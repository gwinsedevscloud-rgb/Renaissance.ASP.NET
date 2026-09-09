# Double-click Launcher.bat (or "Start MedReach.bat" in the repo root) to start:
#   - MedReach API      (:5280)
#   - MedReach Web UI   (:5281)
#   - MedReach Field    (:5121)
# Then opens the Web UI in your browser.

$ErrorActionPreference = "Stop"

$Root = $PSScriptRoot
$StartScript = Join-Path $Root "start-dev.ps1"
$WebUrl = "http://localhost:5281"
$FieldUrl = "http://localhost:5121"
$ApiUrl = "http://localhost:5280/swagger"
$MaxWaitSeconds = 120

if (-not (Test-Path $StartScript)) {
    Write-Host "Could not find start-dev.ps1 in: $Root" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Starting MedReach (API + Web + Field)..." -ForegroundColor Cyan
Write-Host ""

& $StartScript -Restart -Field

Write-Host ""
Write-Host "Waiting for the Web UI..." -ForegroundColor Cyan

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
    Write-Host "MedReach is running." -ForegroundColor Green
    Write-Host "  Web UI: $WebUrl"
    Write-Host "  Field:  $FieldUrl"
    Write-Host "  API:    $ApiUrl"
    Write-Host ""
    Write-Host "Default login: admin / Admin@123"
    Write-Host "Close the API, Web, and Field terminal windows to stop the app."
}
else {
    Write-Host "Servers were started, but the Web UI did not respond in time." -ForegroundColor Yellow
    Write-Host "Open manually: $WebUrl"
    Write-Host "Field:         $FieldUrl"
    exit 1
}
