# Renaissance Angular (Fuse)

Angular 19 + Fuse (`@mattae/angular-shared`) frontend for the Renaissance ASP.NET Core API.

## Prerequisites

- Node.js 20+
- .NET 10 SDK (for the API backend)
- Renaissance API running at `http://localhost:5280`

## Install

```powershell
cd Renaissance.AspNet\frontend\Renaissance.Angular
npm install
```

## Run

```powershell
# Terminal 1 — API
cd Renaissance.AspNet\backend\Renaissance.Api
dotnet run

# Terminal 2 — Angular UI
cd Renaissance.AspNet\frontend\Renaissance.Angular
npm start
```

- API / Swagger: http://localhost:5280/swagger
- Angular UI: http://localhost:5282

## Configuration

Edit `src/environments/environment.ts`:

```typescript
export const environment = {
    production: false,
    apiUrl: 'http://localhost:5280'
};
```

## Stack

- **Angular 19** — standalone components, lazy routes
- **Fuse** — classy vertical layout via `@mattae/angular-shared`
- **Angular Material** — forms, tables, buttons
- **Tailwind CSS** — utility styling with Fuse theme plugins

## Features

- Home dashboard with module overview
- Client list with search
- Client registration and edit
- Client detail view
- Client clinical dashboard (triage, consultations, pharmacy, lab, etc.)

## Project structure

```
src/app/
  core/           # API service, models, Fuse providers
  layout/         # Fuse classy layout shell
  features/       # home, clients, dashboard pages
```
