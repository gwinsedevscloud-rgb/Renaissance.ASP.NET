# Deploy MedReach to IIS from GitHub

## What GitHub Actions does
Workflow `.github/workflows/publish-iis.yml` publishes **API + Web** (no database) and uploads:
- Actions artifact `MedReach-IIS`
- GitHub Release tag **`iis-latest`** (`MedReach-IIS.zip`)

## What you run on the Windows / IIS server (once per deploy)

1. Install **.NET 10 ASP.NET Core Hosting Bundle**, then `iisreset`
2. Open **elevated PowerShell**
3. Download and run the installer:

```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force
Invoke-WebRequest `
  -Uri "https://raw.githubusercontent.com/gwinsedevscloud-rgb/Renaissance.ASP.NET/main/Renaissance.AspNet/deploy/Install-MedReachFromGitHub.ps1" `
  -OutFile "$env:TEMP\Install-MedReachFromGitHub.ps1"
& "$env:TEMP\Install-MedReachFromGitHub.ps1"
```

Or download `MedReach-IIS.zip` from the `iis-latest` release, extract, and run `deploy\Install-MedReachFromGitHub.ps1`.

The script:
- Downloads `iis-latest` from GitHub
- Deploys to `C:\inetpub\MedReach\web` and `...\api`
- Creates IIS site `MedReach` (host `medreach.ecews.org`) + `/api` application
- Sets `ASPNETCORE_ENVIRONMENT=Production`
- Does **not** touch SQL / the database

## Manual HTTPS
In IIS Manager, add an HTTPS binding for `medreach.ecews.org` with your certificate.
