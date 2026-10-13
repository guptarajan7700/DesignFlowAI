# Backend

ASP.NET Core 8 Web API for DesignFlow AI. Clean Architecture, EF Core against the existing SQL Server database, JWT roles, Swagger, and a background job worker.

```text
Backend/
├── DesignFlow.sln
└── src/
    ├── DesignFlow.Api/              Controllers, JWT, Swagger, exception handling
    ├── DesignFlow.Application/      Services, DTOs, FluentValidation, workflow rules
    ├── DesignFlow.Domain/           Entities and enums matching database/
    ├── DesignFlow.Infrastructure/   EF Core, AI providers, Autodesk clients, job worker
    └── DesignFlow.Tests/            xUnit tests for workflow and AI rules
```

## Run

```powershell
dotnet run --project src/DesignFlow.Api
```

- Swagger: `http://localhost:5080/swagger`
- Development database: `(localdb)\MSSQLLocalDB` / `DesignFlowAI`
- Development login: `maya.chen@designflow.ai` / `DesignFlow123!`

The API replaces the seed placeholder password hash the first time it starts in Development.

```powershell
dotnet test
```

## HTTP surface

| Method | Path | Who |
| --- | --- | --- |
| POST | `/api/auth/login` | Anonymous |
| POST | `/api/auth/register` | Anonymous (Client or Designer) |
| GET, POST | `/api/clients` | Staff; create is manager |
| GET, POST | `/api/projects` | Authenticated; create is manager |
| GET | `/api/projects/{id}` | Authenticated |
| POST | `/api/projects/{id}/requirements` | Staff |
| POST | `/api/projects/{id}/plan` | Staff; saves tasks as Proposed |
| POST | `/api/projects/{id}/task-plans/approve` | Manager |
| POST | `/api/projects/{id}/task-plans/reject` | Manager |
| GET | `/api/projects/{id}/reports` | Authenticated |
| GET | `/api/issues` | Authenticated |
| POST | `/api/issues/{id}/assign` | Manager |
| POST | `/api/revit/models/sync` | Staff |
| POST | `/api/revit/validation-runs` | Staff |
| POST | `/api/automation/jobs` | Staff |
| GET | `/api/automation/jobs/{id}` | Staff |
| POST | `/api/ai/requirements/analyze` | Staff |
| POST | `/api/ai/commands/preview` | Staff |
| POST | `/api/ai/commands/execute` | Staff, `confirm: true` required |
| POST | `/api/ai/ask` | Staff |
| POST | `/api/webhooks/aps` | APS callback |
| GET | `/api/system/integrations` | Manager |

A failed validation run opens issues, assigns the project's lead designer, sets the project to `NeedsAttention`, and writes a notification. AI task plans stay `Proposed` until a manager approves them. Revit commands must be on the whitelist (`FindDoorsMissingFireRating`, `FindRoomsWithoutNames`, `FindUnplacedRooms`, `FindMissingSheetParameters`). Execute reads stored validation rows and does not invent elements.

## Credentials

`appsettings.Development.json` has placeholders only. Set real values with user-secrets or environment variables:

- `Jwt:Key` / `Jwt__Key`
- `Ai:Provider`, `Ai:ApiKey` (`OpenAI` or `AzureOpenAI`)
- `Autodesk:ClientId`, `Autodesk:ClientSecret`

Empty Autodesk and AI keys select the mock clients. Data Management, Design Automation, and webhook registration are stubs. APS token auth and Model Derivative translation are implemented and used when credentials are present.
