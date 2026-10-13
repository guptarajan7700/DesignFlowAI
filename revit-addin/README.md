# Revit plugins

DesignFlow AI loads inside Revit 2025 or 2026 as one add-in with four plugins on the **DesignFlow AI** ribbon:

| Plugin | What it does |
| --- | --- |
| Sync model | Sends the open project’s name and file to `POST /api/revit/models/sync` |
| Run validation | Checks fire ratings, room names, unplaced rooms, and sheet parameters, then posts `POST /api/revit/validation-runs` |
| Fetch requirements | Reads the selected studio project and shows its requirements |
| Queue translation | Queues a `TranslateModel` job for the synced model |

A modeless **Open studio** panel runs the same four plugins through Revit’s external event. **Studio settings** chooses the API, the account, and the project.

The first-run account is the seeded designer `arjun.mehta@designflow.ai` / `DesignFlow123!`. The default project is Sharma Residence, `NW-2BHK-001`. The API must be running at `http://localhost:5080`.

## Install

Close Revit, then:

```powershell
cd "C:\Users\HP\OneDrive\Desktop\AI project\DesignFlowAI\revit-addin"
powershell -ExecutionPolicy Bypass -File .\install.ps1
```

Restart Revit. The manifest is written to `%AppData%\Autodesk\Revit\Addins\2025` and `\2026`.

Sync a model before validation or translation. A failed check creates issues in the studio, assigns the lead designer, and marks the project Needs attention.
