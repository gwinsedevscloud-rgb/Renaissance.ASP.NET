# Renaissance (ASP.NET Core)

Clean Architecture split into **backend** layers and a **frontend** MVC app.

```
Renaissance.AspNet/
  backend/
    Renaissance.Domain/          # Entities + enums (no dependencies)
    Renaissance.Application/     # Use cases, DTOs, I*Service, IApplicationDbContext
    Renaissance.Infrastructure/  # EF Core DbContext, migrations, ClientNumberGenerator
    Renaissance.Api/             # Thin controllers + Swagger host
  frontend/
    Renaissance.Web/             # Blazor Web App (Interactive Server) → calls the API
    Renaissance.Angular/         # Angular 19 + Fuse → calls the API
```

## Prerequisites

- .NET 10 SDK
- SQL Server (`MSI\SQLEXPRESS` by default)

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

## Run (three terminals — pick one UI)

```powershell
# Terminal 1 — API (required)
cd Renaissance.AspNet\backend\Renaissance.Api
dotnet run

# Terminal 2a — Blazor UI
cd Renaissance.AspNet\frontend\Renaissance.Web
dotnet run

# Terminal 2b — Angular + Fuse UI
cd Renaissance.AspNet\frontend\Renaissance.Angular
npm install
npm start
```

- API / Swagger: http://localhost:5280/swagger  
- Blazor UI: http://localhost:5281  
- Angular UI: http://localhost:5282  

Sign in on the Blazor UI. Default accounts (password pattern `Role@123`):

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

**Stakeholders dashboard:** `/stakeholders` on both UIs · API: `GET /api/stakeholders-dashboard`

## Build

```powershell
cd Renaissance.AspNet
dotnet build
```

## Sample data (Akwa Ibom)

On first API startup (empty database), the app seeds **7 sample clients** from Akwa Ibom with Ibibio, Annang, Oron, and Efik names — Okon Bassey, Etim Akpan, Nkoyo Udo, Ita Ekpo, Mmeyene John, Bassey Effiong, and Etieno Sunday — each with records across triage, consultation, laboratory, pharmacy, and other modules following the normal clinical flow. Names appear in the client **Address** field (e.g. `Okon Bassey, Uyo LGA, Akwa Ibom State`).

To re-seed, clear the `patient` table (and related records) or drop `RenaissanceDb`, then restart the API.
