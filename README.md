# Renaissance Plugin

Original stack (Java + Angular module federation) remains in this repository root.

## ASP.NET Core redesign

Split into separate folders under **[`Renaissance.AspNet/`](Renaissance.AspNet/README.md)**:

| Folder | Role |
|--------|------|
| `backend/Renaissance.Api` | ASP.NET Core Web API + EF Core + SQL Server |
| `frontend/Renaissance.Web` | Blazor Web App (Interactive Server) calling the API |

```powershell
# API
cd Renaissance.AspNet\backend\Renaissance.Api
dotnet run

# UI (separate terminal)
cd Renaissance.AspNet\frontend\Renaissance.Web
dotnet run
```

- API: http://localhost:5280  
- UI: http://localhost:5281  

Sign in as `admin` / `Admin@123`. Administrators assign roles and module access under **Admin → Roles & modules**.

## Legacy (Java / Angular)

- ``yarn install``
- ``yarn start``
- Navigate to http://localhost:7463

Module Federation related information can be found in [webpack.common.js](webpack/webpack.common.js) and [plugin.yml](src/main/resources/plugin.yml).
[plugin-config-doc.yml](src/main/resources/plugin-config-doc.yml) provides additional information on how [plugin.yml](src/main/resources/plugin.yml) is organized.
