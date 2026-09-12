# MedReach (ASP.NET Core)

Clean Architecture split into **backend** layers and **frontend** apps.

```
Renaissance.AspNet/
  backend/
    Renaissance.Domain/          # Entities + enums (no dependencies)
    Renaissance.Application/     # Use cases, DTOs, I*Service, IApplicationDbContext
    Renaissance.Infrastructure/  # EF Core DbContext, migrations, ClientNumberGenerator
    Renaissance.Api/             # Thin controllers + Swagger host
  frontend/
    Renaissance.Web/             # Blazor Web App (Interactive Server) → calls the API
    MedReach.Field/              # Blazor WASM PWA — offline outreach on Android tablets
  start-dev.ps1                # Start API + Blazor UI (local dev)
  start-lan.ps1                # Start API + Blazor UI (hospital LAN)
  start-dev.bat                # Wrapper for start-dev.ps1
  Launcher.ps1                 # Start dev servers and open browser
```

## Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB, Express, or full instance — see **Configuration** below)

## Start from terminal

### Option A — Quick start (recommended)

From the repo root or this folder, run the dev launcher. It opens **two new PowerShell windows** (API + Blazor UI) and frees ports 5280/5281 if they are already in use.

```powershell
cd Renaissance.AspNet
.\start-dev.ps1
```

| Flag | Description |
|------|-------------|
| `-Restart` | Stop existing listeners on 5280/5281/5121 before starting (**default**) |
| `-Build` | Run `dotnet build` before starting |
| `-Field` | Also start the **MedReach Field** offline PWA on port 5121 |

Examples:

```powershell
.\start-dev.ps1 -Build          # build, then start
.\start-dev.ps1 -Field          # API + Web + Field PWA
.\start-dev.ps1 -Restart:$false # start without killing existing processes
.\start-dev.bat                 # same script from CMD or Explorer
```

**URLs after startup**

| Service | URL |
|---------|-----|
| Blazor UI | http://localhost:5281 |
| Field PWA | http://localhost:5121 (with `-Field`) |
| API | http://localhost:5280 |
| Swagger | http://localhost:5280/swagger |

**Default login:** `admin` / `Admin@123`

### Field PWA (offline tablets)

`MedReach.Field` is a Progressive Web App for community outreach when internet is unreliable:

1. Start with `.\start-dev.ps1 -Field` (API must be reachable for first login + settings pull).
2. Sign in once while online; outreach settings are cached on the tablet.
3. Register patients / triage offline — records queue in IndexedDB.
4. When signal returns, open **Sync queue** (or wait for auto-sync) to push to `api/field-sync`.
5. On Android Chrome: menu → **Install app** / **Add to Home Screen** (HTTPS required in production; localhost works for testing).

Set the API URL in `frontend/MedReach.Field/wwwroot/appsettings.json` (`ApiBaseUrl`).

**Stop the app:** close the **MedReach API** and **MedReach Web** terminal windows, or press `Ctrl+C` in each.

### Option B — Manual (two terminals)

Use this when you want full control or are not on Windows PowerShell.

```powershell
# Terminal 1 — API (required)
cd Renaissance.AspNet\backend\Renaissance.Api
dotnet run

# Terminal 2 — Blazor UI
cd Renaissance.AspNet\frontend\Renaissance.Web
dotnet run
```

On first run, the API applies EF Core migrations and seeds sample data if the database is empty.

### Option C — Hospital LAN deployment

Binds the API and Blazor UI to all network interfaces so other PCs on the hospital network can connect.

```powershell
cd Renaissance.AspNet
.\start-lan.ps1
```

- **This PC:** http://localhost:5281  
- **Other PCs on LAN:** `http://<server-ip>:5281` (the script prints your LAN IP)  
- Allow inbound TCP **5280** and **5281** in Windows Firewall  
- Configure the staff URL under **Admin → Settings** after first login  

### Option E — Launcher (start + open browser)

Double-click `Launcher.bat` or run:

```powershell
cd Renaissance.AspNet
.\Launcher.ps1
```

Starts the dev servers via `start-dev.ps1`, waits for the UI, then opens http://localhost:5281 in your default browser.

## Troubleshooting

| Problem | Fix |
|---------|-----|
| `dotnet build` fails — file locked by `Renaissance.Web` | Stop running app instances: `.\start-dev.ps1 -Restart` or close the API/Web terminal windows |
| Cannot connect to SQL Server | Update `backend/Renaissance.Api/appsettings.json` → `ConnectionStrings:DefaultConnection` |
| Port already in use | Run `.\start-dev.ps1 -Restart` or find and stop the process using 5280/5281 |
| PowerShell script blocked | Run once: `Set-ExecutionPolicy -Scope CurrentUser RemoteSigned` |

## Configuration

**Backend** `backend/Renaissance.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=MSI\\SQLEXPRESS;Initial Catalog=RenaissanceDb;Integrated Security=True;Max Pool Size=50000;Pooling=True;Connect Timeout=30;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
```

**Frontend** `frontend/Renaissance.Web/appsettings.json`:

```json
"ApiSettings": { "BaseUrl": "http://localhost:5280" }
```

## Default accounts

Sign in on the **Web UI** or **Field PWA**. Password pattern: `Role@123`.

| Username | Role | Modules |
|----------|------|---------|
| `admin` | Administrator | All modules, users, and roles |
| `nurse` | Nurse | Clients, Triage, Client Dashboard |
| `clinician` | Clinician | Clients, Triage, Consultations, Pharmacy, Laboratory, Client Dashboard |
| `pharmacist` | Pharmacist | Clients, Pharmacy, Client Dashboard |
| `lab` | Lab Technician | Clients, Laboratory, Client Dashboard |
| `dental` | Dental Officer | Clients, Dental, Client Dashboard |
| `eyecare` | Eye Care | Clients, Optometrists, Ophthalmologists, Client Dashboard |
| `stakeholder` | Stakeholder | Stakeholders dashboard only |

Administrators manage **Users** and **Roles & modules** (`/admin/users`, `/admin/roles`) to decide which modules a role can open. API endpoints enforce the same permissions.

**Stakeholders dashboard:** `/stakeholders` · API: `GET /api/stakeholders-dashboard`

## Build

```powershell
cd Renaissance.AspNet
dotnet build
```

### Publish for IIS (v1.1.0)

Produces `publish/MedReach-1.1.0/` with `api/`, `web/`, and `field/` folders (Field PWA at `/field/`):

```powershell
cd Renaissance.AspNet
.\publish-iis.ps1
# or self-contained (no Hosting Bundle on server):
.\publish-iis.ps1 -SelfContained
```

**Production host:** `https://medreach.ecews.org` (Web), `https://medreach.ecews.org/api` (API), and `https://medreach.ecews.org/field` (Field PWA).

See `publish/MedReach-1.1.0/IIS-README.md` after publish for IIS setup steps. Requires the **.NET 10 ASP.NET Core Hosting Bundle** on the server unless `-SelfContained` is used. Set `RENAISSANCE_JWT_KEY` (32+ chars) and the SQL connection string on the server.

## Sample data (Akwa Ibom)

On first API startup (empty database), the app seeds **7 sample clients** from Akwa Ibom with Ibibio, Annang, Oron, and Efik names — Okon Bassey, Etim Akpan, Nkoyo Udo, Ita Ekpo, Mmeyene John, Bassey Effiong, and Etieno Sunday — each with records across triage, consultation, laboratory, pharmacy, and other modules following the normal clinical flow. Names appear in the client **Address** field (e.g. `Okon Bassey, Uyo LGA, Akwa Ibom State`).

To re-seed, clear the `patient` table (and related records) or drop `RenaissanceDb`, then restart the API.
