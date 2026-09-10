# Publish MedReach API + Web UI + Field PWA for IIS hosting.
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
$FieldOut = Join-Path $OutputRoot "field"
$ApiProj = Join-Path $Root "backend\Renaissance.Api\Renaissance.Api.csproj"
$WebProj = Join-Path $Root "frontend\Renaissance.Web\Renaissance.Web.csproj"
$FieldProj = Join-Path $Root "frontend\MedReach.Field\MedReach.Field.csproj"

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
New-Item -ItemType Directory -Path $FieldOut -Force | Out-Null

Write-Host "Publishing API..."
dotnet publish $ApiProj @publishArgs -o $ApiOut
if ($LASTEXITCODE -ne 0) { throw "API publish failed." }

Write-Host "Publishing Web UI..."
dotnet publish $WebProj @publishArgs -o $WebOut
if ($LASTEXITCODE -ne 0) { throw "Web publish failed." }

Write-Host "Publishing Field PWA (base href /field/)..."
dotnet publish $FieldProj `
    -c Release `
    /p:BaseHref=/field/ `
    /p:PublishTrimmed=false `
    /p:RunAOTCompilation=false `
    /p:DebugType=None `
    /p:DebugSymbols=false `
    -o $FieldOut
if ($LASTEXITCODE -ne 0) { throw "Field publish failed." }

# Ensure IIS SPA web.config is present (Blazor WASM static host)
$fieldWebConfig = Join-Path $FieldOut "wwwroot\web.config"
$fieldRootWebConfig = Join-Path $FieldOut "web.config"
if (Test-Path $fieldWebConfig) {
    Copy-Item $fieldWebConfig $fieldRootWebConfig -Force
} elseif (-not (Test-Path $fieldRootWebConfig)) {
    $srcConfig = Join-Path $Root "frontend\MedReach.Field\wwwroot\web.config"
    if (Test-Path $srcConfig) {
        Copy-Item $srcConfig $fieldRootWebConfig -Force
    }
}

# Blazor WASM publish layout is wwwroot contents at output root for Sdk.BlazorWebAssembly
# Ensure index.html exists at FieldOut root
if (-not (Test-Path (Join-Path $FieldOut "index.html"))) {
    $www = Join-Path $FieldOut "wwwroot"
    if (Test-Path $www) {
        Get-ChildItem $www | ForEach-Object {
            Move-Item $_.FullName -Destination (Join-Path $FieldOut $_.Name) -Force
        }
        Remove-Item $www -Recurse -Force -ErrorAction SilentlyContinue
    }
}

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
| Field PWA | ``https://medreach.ecews.org/field`` |

## Server prerequisites

1. Windows Server with **IIS** + HTTPS certificate for ``medreach.ecews.org``
2. **ASP.NET Core Hosting Bundle** for .NET 10 (required unless ``-SelfContained``)
3. **URL Rewrite** module (for Field SPA fallback)
4. SQL Server reachable from the IIS machine
5. After installing the Hosting Bundle, run ``iisreset``

## Recommended IIS layout

1. Site bound to ``medreach.ecews.org`` (443) -> ``web`` folder (No Managed Code pool)
2. Application ``api`` under that site -> ``api`` folder
3. Application ``field`` under that site -> ``field`` folder (static / No Managed Code)
4. App pool / web.config environment:
   - ``ASPNETCORE_ENVIRONMENT`` = ``Production``
   - ``RENAISSANCE_JWT_KEY`` = long random secret (32+ characters)

## Config to set on the server

- ``api``: SQL ``ConnectionStrings:DefaultConnection`` + JWT key (file or ``RENAISSANCE_JWT_KEY``)
- ``web``: already uses ``ApiSettings:BaseUrl`` = ``https://medreach.ecews.org/api``
- ``field``: ``appsettings.Production.json`` uses ``ApiBaseUrl`` = ``https://medreach.ecews.org/api/``
- CORS already allows ``https://medreach.ecews.org`` and ``https://www.medreach.ecews.org``

## Smoke test

1. ``https://medreach.ecews.org``
2. ``https://medreach.ecews.org/field``
3. Login: ``admin`` / ``Admin@123`` (change after first deploy)
"@

Set-Content -Path (Join-Path $OutputRoot "IIS-README.md") -Value $readme -Encoding UTF8

Write-Host ""
Write-Host "Publish complete: $OutputRoot"
Write-Host "  api   -> $ApiOut"
Write-Host "  web   -> $WebOut"
Write-Host "  field -> $FieldOut"
Write-Host "See IIS-README.md in the output folder."
