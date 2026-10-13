# Architecture

DesignFlow AI connects three clients to one database.

```text
Angular CRM  ----\                     /---- Revit add-in
                 >-- ASP.NET Core API --
AI provider  ----/         |           \---- Autodesk APS
                           |
                     SQL Server
```

## Boundaries

| Folder | Responsibility |
| --- | --- |
| `database/` | Schema, constraints, procedures, views, seed, migrations. Source of truth for tables. |
| `backend/` | Clean Architecture API. DTOs cross the HTTP boundary; entities stay inside. JWT roles: Admin, ProjectManager, Designer, Client. |
| `frontend/` | Standalone Angular app. `environment` holds `apiUrl` only. |
| `revit-addin/` | Ribbon commands, validation checks, modeless WPF via `IExternalEventHandler`. Calls the API with JWT. |

## Domain rules

1. A failed validation run creates issues, assigns the project lead designer, moves the project to `NeedsAttention`, and notifies that designer (or the project manager when no designer is set).
2. AI task plans are stored as `Tasks.Status = Proposed` and `Tasks.Source = AiProposed`. They become `ToDo` only through `usp_ApproveProposedTasks`, which requires Admin or ProjectManager.
3. Natural-language Revit actions are limited to a server-side command whitelist. The API previews a command and executes it only after an explicit confirm. Every step is written to `AiCommandLogs`.
4. Design-review text may summarize rows in `ValidationResults` only. The assistant must not invent defects.
5. APS and model-provider credentials never leave the API host.

## Frontend direction (Phase 4)

Premium SaaS shell: collapsible sidebar, command palette, notification menu, light/dark theme persisted locally.

- Primary `#4F46E5`, accent `#14B8A6`, Inter and Plus Jakarta Sans, 12–16px radius, glass cards.
- Angular standalone components and signals, Tailwind, Angular Material / CDK, ECharts or ApexCharts.
- Lazy routes: login, dashboard, clients, projects, kanban tasks, issues, model viewer, AI assistant, automation, reports, settings.
- Reusable kit first: kpi card, status chip, data table, page header, empty state, skeleton, confirm dialog, avatar group, progress ring, command palette.
- Feature data goes through interfaces with mock providers until the API is wired.

## Integration phasing

Autodesk work starts with 2-legged and 3-legged APS auth plus Model Derivative. Data Management, Design Automation, and webhooks stay behind interfaces. Mock clients keep the API runnable with no Autodesk account.
