# Install / update MedReach on the IIS server from the GitHub release "iis-latest".
# Run in elevated PowerShell on the Windows Server (RDP).
#
# Example:
#   Set-ExecutionPolicy Bypass -Scope Process -Force
#   .\Install-MedReachFromGitHub.ps1
#   .\Install-MedReachFromGitHub.ps1 -Repo "gwinsedevscloud-rgb/Renaissance.ASP.NET" -SitePath "C:\inetpub\MedReach"

param(
    [string]$Repo = "gwinsedevscloud-rgb/Renaissance.ASP.NET",
    [string]$Tag = "iis-latest",
    [string]$SitePath = "C:\inetpub\MedReach",
    [string]$SiteName = "MedReach",
    [string]$HostName = "medreach.ecews.org",
    [string]$JwtKey = "HLwsqGur39c8mV1PtfjyaglJITdAiNxCRX75BSFhOQvM2UEn"
)

$ErrorActionPreference = "Stop"

function Assert-Admin {
    $id = [Security.Principal.WindowsIdentity]::GetCurrent()
    $p = New-Object Security.Principal.WindowsPrincipal($id)
    if (-not $p.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Run this script as Administrator."
    }
}

Assert-Admin

Write-Host "Downloading release $Tag from $Repo ..."
$api = "https://api.github.com/repos/$Repo/releases/tags/$Tag"
$release = Invoke-RestMethod -Uri $api -Headers @{ "User-Agent" = "MedReach-IIS-Installer" }
$asset = $release.assets | Where-Object { $_.name -match "\.zip$" } | Select-Object -First 1
if (-not $asset) { throw "No zip asset found on release $Tag." }

$temp = Join-Path $env:TEMP "MedReach-IIS-$(Get-Date -Format 'yyyyMMddHHmmss')"
New-Item -ItemType Directory -Path $temp -Force | Out-Null
$zip = Join-Path $temp "MedReach-IIS.zip"
Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $zip -UseBasicParsing
Expand-Archive -Path $zip -DestinationPath $temp -Force

$apiSrc = Join-Path $temp "api"
$webSrc = Join-Path $temp "web"
if (-not (Test-Path $apiSrc) -or -not (Test-Path $webSrc)) {
    throw "Zip must contain api/ and web/ folders."
}

Import-Module WebAdministration -ErrorAction Stop

# Ensure IIS features exist (best-effort)
try {
    Install-WindowsFeature Web-Server, Web-Asp-Net45, Web-Mgmt-Console -ErrorAction SilentlyContinue | Out-Null
} catch { }

$webPath = Join-Path $SitePath "web"
$apiPath = Join-Path $SitePath "api"
New-Item -ItemType Directory -Force -Path $webPath, $apiPath | Out-Null

Write-Host "Stopping site/app pools if present..."
if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
    Stop-Website -Name $SiteName -ErrorAction SilentlyContinue
}
if (Test-Path "IIS:\AppPools\$SiteName") {
    Stop-WebAppPool -Name $SiteName -ErrorAction SilentlyContinue
}
if (Test-Path "IIS:\AppPools\$SiteName-api") {
    Stop-WebAppPool -Name "$SiteName-api" -ErrorAction SilentlyContinue
}
Start-Sleep -Seconds 2

Write-Host "Copying published files to $SitePath ..."
robocopy $webSrc $webPath /MIR /NFL /NDL /NJH /NJS /nc /ns /np | Out-Null
robocopy $apiSrc $apiPath /MIR /NFL /NDL /NJH /NJS /nc /ns /np | Out-Null

function Ensure-AppPool([string]$Name) {
    if (-not (Test-Path "IIS:\AppPools\$Name")) {
        New-WebAppPool -Name $Name | Out-Null
    }
    Set-ItemProperty "IIS:\AppPools\$Name" -Name managedRuntimeVersion -Value ""
    Set-ItemProperty "IIS:\AppPools\$Name" -Name startMode -Value AlwaysRunning
}

Ensure-AppPool $SiteName
Ensure-AppPool "$SiteName-api"

# Environment variables for ASP.NET Core
function Set-PoolEnv([string]$PoolName, [hashtable]$Vars) {
    $envPath = "IIS:\AppPools\$PoolName"
    $existing = Get-ItemProperty $envPath -Name environmentVariables -ErrorAction SilentlyContinue
    # Prefer web.config env via applicationHost is complex; write to web.config instead below.
}

function Ensure-AspNetCoreEnv([string]$AppRoot, [hashtable]$Vars) {
    $webConfig = Join-Path $AppRoot "web.config"
    if (-not (Test-Path $webConfig)) {
        Write-Warning "web.config missing in $AppRoot"
        return
    }
    [xml]$xml = Get-Content $webConfig
    $aspNet = $xml.configuration.location.'system.webServer'.aspNetCore
    if (-not $aspNet) {
        $aspNet = $xml.SelectSingleNode("//aspNetCore")
    }
    if (-not $aspNet) {
        Write-Warning "aspNetCore node not found in $webConfig"
        return
    }
    $envVars = $aspNet.environmentVariables
    if (-not $envVars) {
        $envVars = $xml.CreateElement("environmentVariables")
        [void]$aspNet.AppendChild($envVars)
    }
    foreach ($key in $Vars.Keys) {
        $node = $envVars.environmentVariable | Where-Object { $_.name -eq $key } | Select-Object -First 1
        if (-not $node) {
            $node = $xml.CreateElement("environmentVariable")
            $node.SetAttribute("name", $key)
            [void]$envVars.AppendChild($node)
        }
        $node.SetAttribute("value", [string]$Vars[$key])
    }
    $xml.Save($webConfig)
}

Ensure-AspNetCoreEnv $webPath @{
    ASPNETCORE_ENVIRONMENT = "Production"
}
Ensure-AspNetCoreEnv $apiPath @{
    ASPNETCORE_ENVIRONMENT = "Production"
    RENAISSANCE_JWT_KEY    = $JwtKey
}

if (-not (Get-Website -Name $SiteName -ErrorAction SilentlyContinue)) {
    New-Website -Name $SiteName -PhysicalPath $webPath -ApplicationPool $SiteName -Port 80 -HostHeader $HostName | Out-Null
} else {
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name physicalPath -Value $webPath
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name applicationPool -Value $SiteName
}

# Ensure /api application
$apiApp = Get-WebApplication -Site $SiteName -Name "api" -ErrorAction SilentlyContinue
if (-not $apiApp) {
    New-WebApplication -Site $SiteName -Name "api" -PhysicalPath $apiPath -ApplicationPool "$SiteName-api" | Out-Null
} else {
    Set-ItemProperty "IIS:\Sites\$SiteName\api" -Name physicalPath -Value $apiPath
    Set-ItemProperty "IIS:\Sites\$SiteName\api" -Name applicationPool -Value "$SiteName-api"
}

# Permissions for App_Data
$appData = Join-Path $apiPath "App_Data"
New-Item -ItemType Directory -Force -Path $appData | Out-Null
icacls $appData /grant "IIS AppPool\${SiteName}-api:(OI)(CI)M" | Out-Null

Start-WebAppPool -Name $SiteName -ErrorAction SilentlyContinue
Start-WebAppPool -Name "$SiteName-api" -ErrorAction SilentlyContinue
Start-Website -Name $SiteName -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "MedReach deployed to $SitePath"
Write-Host "  Web: http://$HostName/  (add HTTPS binding in IIS Manager if not present)"
Write-Host "  API: http://$HostName/api/"
Write-Host "Ensure .NET 10 ASP.NET Core Hosting Bundle is installed, then run: iisreset"
Write-Host "Database is not modified by this script."
