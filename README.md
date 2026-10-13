# DesignFlow AI

Intelligent interior-design CRM and Revit automation platform for design firms, BIM teams, designers, and project managers.

A web CRM, an AI assistant, and a Revit desktop add-in share one SQL Server database. Client briefs become tracked requirements and tasks. Model checks open issues, assign the project designer, and notify the team. AI task plans stay **Proposed** until a project manager approves them.

## Repository layout

```text
DesignFlowAI/
├── Backend/        ASP.NET Core 8 Web API
├── Frontend/       Angular CRM studio
├── database/       SQL Server scripts
├── revit-addin/    Revit 2025/2026 plugins
├── docs/           Architecture and setup
├── .gitignore
└── README.md
```

## Run the backend

Load the database, then from `Backend/`:

```powershell
dotnet run --project src/DesignFlow.Api
```

Swagger: `http://localhost:5080/swagger`. Sign in as `maya.chen@designflow.ai` / `DesignFlow123!` after the API has replaced the seed password hashes. Full steps are in [docs/SETUP.md](docs/SETUP.md).

SQL Server 2019 or newer, or SQL Server LocalDB. Details and the `sqlcmd` order are in [database/README.md](database/README.md) and [docs/SETUP.md](docs/SETUP.md).

From `database/` in PowerShell:

```powershell
.\run.ps1 -Server "localhost"
```

LocalDB:

```powershell
.\run.ps1 -Server "(localdb)\MSSQLLocalDB"
```

The scripts are idempotent. In Development, the API replaces the placeholder password hash with an Identity hash for `DesignFlow123!` the first time it starts.

## Run the frontend

```powershell
cd Frontend
npm install
npm start
```

Open http://localhost:4200 and sign in as `maya.chen@designflow.ai` / `DesignFlow123!`. The API must be running at http://localhost:5080.

## Workflow rules already encoded in SQL

- `dbo.usp_CreateIssuesFromFailedValidation` — a **Failed** validation run opens an issue per failed check, assigns the project’s lead designer, sets the project to `NeedsAttention`, and writes a notification.
- `dbo.usp_ApproveProposedTasks` — AI tasks in status `Proposed` become `ToDo` only after an Admin or ProjectManager approves them. Until then they are not active work.
- `dbo.usp_RejectProposedTasks` — the same rows become `Rejected` and never enter the board.

The seed calls the failed-validation procedure for the Sharma residence model, so issues and the notification are produced by that rule.

## Credentials

Autodesk APS and AI provider keys stay in backend user-secrets or environment variables. They are not committed and they do not belong in the frontend. Empty values keep the mock providers.
