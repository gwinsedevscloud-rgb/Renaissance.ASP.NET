# Renaissance Angular (Fuse)

Angular 19 + Fuse frontend for the Renaissance ASP.NET Core API — **feature parity with the Blazor UI** (`Renaissance.Web`).

## Prerequisites

- Node.js 20+
- .NET 10 SDK (for the API backend)
- Renaissance API running at `http://localhost:5280`

## Install & run

```powershell
cd Renaissance.AspNet\frontend\Renaissance.Angular
npm install
npm start
```

With the full stack:

```powershell
cd Renaissance.AspNet
.\start-dev.ps1
# Blazor: http://localhost:5281
# Angular: http://localhost:5282
```

- API / Swagger: http://localhost:5280/swagger
- Angular UI: http://localhost:5282

Default login: `admin` / `Admin@123`  
LAN access password: `Network@2026`

## Features

- JWT authentication, module-based guards, session restore
- Home dashboard with referral queue stats
- **Patients** — list, register, edit, detail, clinical dashboard with journey
- **Clinical modules** — Triage, Consultations, Pharmacy (incl. dispense), Laboratory, Dental, Ancillary, Optometrists, Ophthalmologists (index, create, edit)
- **Referrals** — queue panels, post-save referral dialog, header inbox bell
- **Stakeholders** KPI dashboard
- **Admin** — Users, Roles, Settings, LAN access, Backup, Export
- **Error pages** — access denied, not found

## Configuration

Edit `src/environments/environment.ts`:

```typescript
export const environment = {
    production: false,
    apiUrl: 'http://localhost:5280'
};
```

## Project structure

```
src/app/
  core/           # API client, auth, models, guards, interceptors
  features/       # home, auth, clients, clinical modules, admin, stakeholders
  shared/         # page-header, patient search, referral UI, forms
  layout/         # Fuse classy shell
```
