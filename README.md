# MedReach

Clinical outreach / EMR data-collection app (ASP.NET Core).

## Apps

| App | Stack | Role |
|-----|--------|------|
| **API** | ASP.NET Core | Backend + EF Core + SQL Server |
| **Web UI** | Blazor Server | Hospital / clinic desktop UI |
| **Field PWA** | Blazor WASM | Offline outreach on Android tablets |

Everything lives under [`Renaissance.AspNet/`](Renaissance.AspNet/README.md).

## Quick start

Double-click **`Start MedReach.bat`**, or:

```powershell
cd Renaissance.AspNet
.\start-dev.ps1 -Field
```

| Service | URL |
|---------|-----|
| Web UI | http://localhost:5281 |
| Field PWA | http://localhost:5121 |
| API | http://localhost:5280/swagger |

**Login:** `admin` / `Admin@123`

Full setup, LAN deploy, and troubleshooting: [`Renaissance.AspNet/README.md`](Renaissance.AspNet/README.md).
