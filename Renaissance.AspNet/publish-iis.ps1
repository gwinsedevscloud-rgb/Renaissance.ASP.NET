# Publish MedReach API + Web UI for IIS hosting (version from Directory.Build.props).
# Usage:
#   .\publish-iis.ps1
#   .\publish-iis.ps1 -OutputRoot "D:\Deploy\MedReach"
#   .\publish-iis.ps1 -SelfContained   # no .NET Hosting Bundle required on server

param(
    [string]$OutputRoot = "",
    [switch]$SelfContained,
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"
$Root = $PSScriptRoot
$Version = "1.0.0"

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path (Split-Path $Root -Parent) "publish\MedReach-$Version"
}

$ApiOut = Join-Path $OutputRoot "api"
$WebOut = Join-Path $OutputRoot "web"
$ApiProj = Join-Path $Root "backend\Renaissance.Api\Renaissance.Api.csproj"
$WebProj = Join-Path $Root "frontend\Renaissance.Web\Renaissance.Web.csproj"

$publishArgs = @(
    "-c", "Release",
    "-r", $Runtime,
    "--self-contained", ($(if ($SelfContained) { "true" } else { "false" })),
    "/p:PublishSingleFile=false",
    "/p:DebugType=None",
    "/p:DebugSymbols=false"
)

Write-Host "Publishing MedReach $Version for IIS ($Runtime, self-contained=$SelfContained)..."
Write-Host "Output: $OutputRoot"
Write-Host ""

if (Test-Path $OutputRoot) {
    Write-Host "Cleaning previous output..."
    Remove-Item -Recurse -Force $OutputRoot
}

New-Item -ItemType Directory -Path $ApiOut -Force | Out-Null
New-Item -ItemType Directory -Path $WebOut -Force | Out-Null

Write-Host "Publishing API..."
dotnet publish $ApiProj @publishArgs -o $ApiOut
if ($LASTEXITCODE -ne 0) { throw "API publish failed." }

Write-Host "Publishing Web UI..."
dotnet publish $WebProj @publishArgs -o $WebOut
if ($LASTEXITCODE -ne 0) { throw "Web publish failed." }

$readme = @"
# MedReach $Version - IIS deployment for medreach.ecews.org

Published: $(Get-Date -Format "yyyy-MM-dd HH:mm")
Runtime: $Runtime
Self-contained: $SelfContained

## Target URLs

| Role | Public URL |
|------|------------|
| Web UI | ``https://medreach.ecews.org`` |
| API | ``https://medreach.ecews.org/api`` |

Field PWA is **not** included in this package.

## Server prerequisites

1. Windows Server with **IIS** + HTTPS certificate for ``medreach.ecews.org``
2. **ASP.NET Core Hosting Bundle** for .NET 10 (required unless ``-SelfContained``)
   - https://dotnet.microsoft.com/download/dotnet/10.0
3. SQL Server reachable from the IIS machine
4. After installing the Hosting Bundle, run ``iisreset``

## Recommended IIS layout

1. Site bound to ``medreach.ecews.org`` (443) -> ``web`` folder (No Managed Code pool)
2. Application ``api`` under that site -> ``api`` folder
3. App pool / web.config environment:
   - ``ASPNETCORE_ENVIRONMENT`` = ``Production``
   - ``RENAISSANCE_JWT_KEY`` = long random secret (32+ characters)

## Config to set on the server

- ``api``: SQL ``ConnectionStrings:DefaultConnection`` + JWT key (file or ``RENAISSANCE_JWT_KEY``)
- ``web``: already uses ``ApiSettings:BaseUrl`` = ``https://medreach.ecews.org/api``
- CORS already allows ``https://medreach.ecews.org`` and ``https://www.medreach.ecews.org``

## Smoke test

1. ``https://medreach.ecews.org``
2. Login: ``admin`` / ``Admin@123`` (change after first deploy)

## Notes

- Both folders include ``web.config`` for ASP.NET Core Module (ANCM).
- Grant API app-pool **Modify** on ``api\App_Data``.
"@

Set-Content -Path (Join-Path $OutputRoot "IIS-README.md") -Value $readme -Encoding UTF8

Write-Host ""
Write-Host "Publish complete: $OutputRoot"
Write-Host "  api -> $ApiOut"
Write-Host "  web -> $WebOut"
Write-Host "See IIS-README.md in the output folder."
