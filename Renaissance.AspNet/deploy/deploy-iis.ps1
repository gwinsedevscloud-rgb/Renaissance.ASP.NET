# Deploy MedReach API + Web + Field into IIS site folders (self-hosted runner).
# Pattern matches adl-monitor/scripts/deploy_iis.ps1 and ECOMAN/scripts/deploy-iis.ps1.
param(
    [Parameter(Mandatory = $true)]
    [string]$WebSource,

    [Parameter(Mandatory = $true)]
    [string]$ApiSource,

    [string]$FieldSource = "",

    [string]$WebDest = "C:\inetpub\wwwroot\MedReach\web",
    [string]$ApiDest = "C:\inetpub\wwwroot\MedReach\api",
    [string]$FieldDest = "C:\inetpub\wwwroot\MedReach\field",
    [string]$WebAppPool = "MedReach",
    [string]$ApiAppPool = "MedReach-api",
    [string]$FieldAppPool = "MedReach-field",
    [string]$SiteName = "MedReach",
    [string]$HostName = "medreach.ecews.org",
    [switch]$PreserveServerAppSettings
)

$ErrorActionPreference = "Stop"

function Reset-NativeExitCode { cmd /c "exit 0" | Out-Null }

function Get-AppCmd {
    $appcmd = Join-Path $env:windir "System32\inetsrv\appcmd.exe"
    if (-not (Test-Path $appcmd)) { throw "appcmd.exe not found. Is IIS installed?" }
    return $appcmd
}

function Invoke-AppCmd {
    param([string[]]$AppCmdArgs)
    $appcmd = Get-AppCmd
    $output = & $appcmd @AppCmdArgs 2>&1
    $code = $LASTEXITCODE
    Reset-NativeExitCode
    $text = ($output | Out-String).Trim()
    if ($text) { Write-Host $text }
    return @{ Ok = ($code -eq 0); Text = $text; Code = $code }
}

function Ensure-AppPool([string]$Name) {
    $list = Invoke-AppCmd @("list", "apppool", "/text:name")
    if ($list.Text -notmatch "(?m)^$([regex]::Escape($Name))$") {
        Write-Host "Creating app pool $Name (No Managed Code)"
        Invoke-AppCmd @("add", "apppool", "/name:$Name", "/managedRuntimeVersion:", "/managedPipelineMode:Integrated") | Out-Null
    }
}

function Stop-AppPool([string]$Name) {
    Write-Host "Stopping app pool $Name"
    Invoke-AppCmd @("stop", "apppool", "/apppool.name:$Name") | Out-Null
}

function Start-AppPool([string]$Name) {
    Write-Host "Starting app pool $Name"
    Invoke-AppCmd @("start", "apppool", "/apppool.name:$Name") | Out-Null
}

function Sync-Folder([string]$Source, [string]$Dest) {
    if (-not (Test-Path $Source)) { throw "Source missing: $Source" }
    New-Item -ItemType Directory -Path $Dest -Force | Out-Null
    $args = @($Source, $Dest, "/E", "/IS", "/IT", "/NFL", "/NDL", "/NJH", "/NJS", "/NC", "/NS")
    if ($PreserveServerAppSettings) {
        $args += @("/XF", "appsettings.json", "appsettings.Production.json")
        $args += @("/XD", "App_Data")
    }
    Write-Host "Robocopy $Source -> $Dest"
    & robocopy @args
    $code = $LASTEXITCODE
    Reset-NativeExitCode
    if ($code -ge 8) { throw "robocopy failed with exit code $code" }
    Write-Host "Robocopy OK (exit $code)"
}

Write-Host "=== MedReach IIS deploy ==="
Write-Host "Runner: $([Security.Principal.WindowsIdentity]::GetCurrent().Name)"
Write-Host "Web: $WebSource -> $WebDest ($WebAppPool)"
Write-Host "API: $ApiSource -> $ApiDest ($ApiAppPool)"
if (-not [string]::IsNullOrWhiteSpace($FieldSource)) {
    Write-Host "Field: $FieldSource -> $FieldDest ($FieldAppPool)"
}
Write-Host "Site: $SiteName / host $HostName"

Ensure-AppPool $WebAppPool
Ensure-AppPool $ApiAppPool
if (-not [string]::IsNullOrWhiteSpace($FieldSource)) {
    Ensure-AppPool $FieldAppPool
}

Import-Module WebAdministration -ErrorAction Stop

if (-not (Get-Website -Name $SiteName -ErrorAction SilentlyContinue)) {
    Write-Host "Creating IIS site $SiteName"
    New-Item -ItemType Directory -Path $WebDest -Force | Out-Null
    New-Website -Name $SiteName -PhysicalPath $WebDest -ApplicationPool $WebAppPool -Port 80 -HostHeader $HostName | Out-Null
} else {
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name physicalPath -Value $WebDest
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name applicationPool -Value $WebAppPool
}

$apiApp = Get-WebApplication -Site $SiteName -Name "api" -ErrorAction SilentlyContinue
if (-not $apiApp) {
    Write-Host "Creating application $SiteName/api"
    New-Item -ItemType Directory -Path $ApiDest -Force | Out-Null
    New-WebApplication -Site $SiteName -Name "api" -PhysicalPath $ApiDest -ApplicationPool $ApiAppPool | Out-Null
} else {
    Set-ItemProperty "IIS:\Sites\$SiteName\api" -Name physicalPath -Value $ApiDest
    Set-ItemProperty "IIS:\Sites\$SiteName\api" -Name applicationPool -Value $ApiAppPool
}

if (-not [string]::IsNullOrWhiteSpace($FieldSource)) {
    $fieldApp = Get-WebApplication -Site $SiteName -Name "field" -ErrorAction SilentlyContinue
    if (-not $fieldApp) {
        Write-Host "Creating application $SiteName/field"
        New-Item -ItemType Directory -Path $FieldDest -Force | Out-Null
        New-WebApplication -Site $SiteName -Name "field" -PhysicalPath $FieldDest -ApplicationPool $FieldAppPool | Out-Null
    } else {
        Set-ItemProperty "IIS:\Sites\$SiteName\field" -Name physicalPath -Value $FieldDest
        Set-ItemProperty "IIS:\Sites\$SiteName\field" -Name applicationPool -Value $FieldAppPool
    }
}

Stop-AppPool $WebAppPool
Stop-AppPool $ApiAppPool
if (-not [string]::IsNullOrWhiteSpace($FieldSource)) {
    Stop-AppPool $FieldAppPool
}
Start-Sleep -Seconds 2

try {
    Sync-Folder $WebSource $WebDest
    Sync-Folder $ApiSource $ApiDest
    if (-not [string]::IsNullOrWhiteSpace($FieldSource)) {
        Sync-Folder $FieldSource $FieldDest
    }

    $appData = Join-Path $ApiDest "App_Data"
    New-Item -ItemType Directory -Path $appData -Force | Out-Null
    & icacls $appData /grant "IIS AppPool\${ApiAppPool}:(OI)(CI)M" | Out-Null
    Reset-NativeExitCode
}
finally {
    Start-AppPool $ApiAppPool
    Start-AppPool $WebAppPool
    if (-not [string]::IsNullOrWhiteSpace($FieldSource)) {
        Start-AppPool $FieldAppPool
    }
    Invoke-AppCmd @("start", "site", $SiteName) | Out-Null
}

if (-not (Test-Path (Join-Path $WebDest "Renaissance.Web.dll"))) {
    throw "Renaissance.Web.dll missing after deploy"
}
if (-not (Test-Path (Join-Path $ApiDest "Renaissance.Api.dll"))) {
    throw "Renaissance.Api.dll missing after deploy"
}
if (-not [string]::IsNullOrWhiteSpace($FieldSource)) {
    if (-not (Test-Path (Join-Path $FieldDest "index.html"))) {
        throw "Field index.html missing after deploy"
    }
}

Write-Host "Deploy complete."
Write-Host "  Web:   http://$HostName/"
Write-Host "  API:   http://$HostName/api/"
if (-not [string]::IsNullOrWhiteSpace($FieldSource)) {
    Write-Host "  Field: http://$HostName/field/"
}
Write-Host "Add HTTPS binding in IIS Manager if needed. Database is not modified."
$global:LASTEXITCODE = 0
exit 0
