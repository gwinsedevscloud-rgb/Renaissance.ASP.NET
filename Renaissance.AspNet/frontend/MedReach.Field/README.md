# MedReach Field (offline PWA)

Blazor WebAssembly Progressive Web App for Android tablets in communities with intermittent internet.

## What it does

- Sign in once while online (hospital MedReach account)
- Cache active outreach / care-program settings **and clinical catalogs**
- Register patients and capture triage, lab, consultation, pharmacy, dental, and eye visits **offline** (IndexedDB queue)
- Push to hospital API (`api/field-sync`) when connectivity returns
- Device labeling + hospital Admin → Field devices visibility

## Local run

```powershell
cd Renaissance.AspNet
.\start-dev.ps1 -Field
```

- Field UI: http://localhost:5121  
- API: http://localhost:5280 (must be running)
- Web Admin → Field devices: http://localhost:5281/admin/field-devices

Configure API base URL in `wwwroot/appsettings.json`.

## Install on Android

1. Open the Field URL in Chrome (HTTPS in production; localhost OK for testing).
2. Menu → **Install app** or **Add to Home Screen**.
3. Set a device label under **Device** (e.g. `Tablet-North-Camp-01`).
4. Sign in once online, tap **Refresh settings**, then work offline.

## Phase 2–3 scope (current)

| Area | Status |
|------|--------|
| Lab / consultation / pharmacy / dental / eye offline | Yes |
| Secondary outreach (deworming / ITN) offline | Yes |
| Cached catalogs + local patients | Yes |
| JWT refresh (14-day grace) + background sync | Yes |
| Conflict UI (retry / discard / details) | Yes |
| Device label + Admin field devices | Yes |

## Branding

Field uses the same `site.css` design system as `Renaissance.Web`. After changing hospital web styles, copy:

```powershell
Copy-Item .\frontend\Renaissance.Web\wwwroot\css\site.css .\frontend\MedReach.Field\wwwroot\css\site.css -Force
Copy-Item .\frontend\Renaissance.Web\wwwroot\images\login\* .\frontend\MedReach.Field\wwwroot\images\login\ -Force
```

## Production URL

After IIS publish/deploy, Field is available at:

`https://medreach.ecews.org/field`
