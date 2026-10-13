# Setup

Phase 1 loads the SQL Server database. The ASP.NET Core API in `Backend/` connects to that database.

## Prerequisites

- .NET 8 SDK
- SQL Server 2019+ or SQL Server Express LocalDB
- `sqlcmd` (installed with SQL Server tools, or `winget install Microsoft.Sqlcmd`)
- Windows PowerShell 5.1 or PowerShell 7

Confirm the tools:

```powershell
dotnet --version
sqlcmd -?
```

## Create and load the database

Run from the `database` folder so relative `sqlcmd` paths resolve.

Windows authentication against a default local instance:

```powershell
cd database
.\run.ps1 -Server "localhost"
```

LocalDB:

```powershell
cd database
.\run.ps1 -Server "(localdb)\MSSQLLocalDB"
```

SQL authentication:

```powershell
cd database
.\run.ps1 -Server "localhost" -User "sa" -Password "<your-local-password>"
```

`run.ps1` executes, in order:

1. `000_create_database.sql` on `master` (creates `DesignFlowAI`, enables read-committed snapshot)
2. `01_schema` — one `CREATE TABLE` script per table
3. `02_constraints` — foreign keys, indexes, unique constraints, check constraints
4. `03_stored_procedures`
5. `04_views`
6. `05_seed_data`
7. `06_migrations` — baseline marker only; no further DDL

Equivalent manual call for a single file:

```powershell
sqlcmd -S localhost -E -b -d DesignFlowAI -i ".\01_schema\002_Users.sql"
```

Use `-b` so `sqlcmd` returns a failure exit code on a SQL error.

## Verify

```powershell
sqlcmd -S localhost -E -d DesignFlowAI -Q "SELECT Code, Status, ClientName, OpenIssueCount, OverdueTaskCount FROM dbo.vw_ProjectDashboard ORDER BY Code;"
```

Expected after a fresh seed:

| Code | What you should see |
| --- | --- |
| `NW-2BHK-001` | Status `NeedsAttention`, open issues created by the failed-validation procedure |
| `HB-LOFT-014` | Status `Active`, at least one overdue task |
| `LM-LOBBY-007` | Status `InReview` |

Proposed AI tasks for the Sharma residence stay `Proposed`:

```powershell
sqlcmd -S localhost -E -d DesignFlowAI -Q "SELECT Title, Status, Source FROM dbo.Tasks WHERE Source = N'AiProposed';"
```

## Development accounts

On first startup in Development, the API replaces the placeholder hash with an ASP.NET Identity hash for `DesignFlow123!`. Use that password only on a local database.

| Email | Role | Password |
| --- | --- | --- |
| `admin@designflow.ai` | Admin | `DesignFlow123!` |
| `maya.chen@designflow.ai` | ProjectManager | `DesignFlow123!` |
| `arjun.mehta@designflow.ai` | Designer | `DesignFlow123!` |
| `lena.ortiz@designflow.ai` | Designer | `DesignFlow123!` |
| `rahul.sharma@northwind.example` | Client | `DesignFlow123!` |
| `sofia.alvarez@harborandco.example` | Client | `DesignFlow123!` |

## Run the API

Load the database first, then from `backend/`:

```powershell
dotnet run --project src/DesignFlow.Api
```

Swagger opens at `http://localhost:5080/swagger`.

Login:

```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:5080/api/auth/login -ContentType "application/json" -Body '{"email":"maya.chen@designflow.ai","password":"DesignFlow123!"}'
```

The Development connection string points at `(localdb)\MSSQLLocalDB`. Override it without editing committed files:

```powershell
$env:ConnectionStrings__DesignFlow = "Server=localhost;Database=DesignFlowAI;Trusted_Connection=True;TrustServerCertificate=True"
```

Autodesk and AI keys stay in user-secrets or environment variables. Empty values keep the mock providers, so the API runs with no Autodesk account.

```powershell
cd src/DesignFlow.Api
dotnet user-secrets set "Jwt:Key" "a-local-key-at-least-32-characters"
dotnet user-secrets set "Ai:Provider" "OpenAI"
dotnet user-secrets set "Ai:ApiKey" "<your-key>"
dotnet user-secrets set "Autodesk:ClientId" "<client-id>"
dotnet user-secrets set "Autodesk:ClientSecret" "<client-secret>"
```

Equivalent environment variables: `Jwt__Key`, `Ai__ApiKey`, `Ai__Provider`, `Autodesk__ClientId`, `Autodesk__ClientSecret`.

`GET /api/system/integrations` (manager token) reports Mock or APS. It never returns secrets.

## Tests

```powershell
cd backend
dotnet test
```

## Later phases

The Angular app and the Revit add-in are not built yet.
