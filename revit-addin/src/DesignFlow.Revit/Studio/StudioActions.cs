using System.IO;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DesignFlow.Revit.Checks;
using DesignFlow.Revit.Ui;

namespace DesignFlow.Revit.Studio;

public static class StudioActions
{
    public static void Sync(UIApplication application)
    {
        var document = RequireProject(application);
        if (document is null)
        {
            return;
        }

        var settings = StudioSettings.Load();
        if (!Guid.TryParse(settings.ProjectId, out var projectId))
        {
            TaskDialog.Show("DesignFlow", "Choose a DesignFlow project in Studio settings before syncing.");
            return;
        }

        var fileName = string.IsNullOrWhiteSpace(document.PathName)
            ? document.Title + ".rvt"
            : Path.GetFileName(document.PathName);

        var client = new StudioClient(settings);
        var model = client.SyncAsync(new
        {
            projectId,
            name = document.Title,
            fileName,
            versionLabel = document.Application.VersionNumber
        }).GetAwaiter().GetResult();

        settings.LastRevitModelId = model.RevitModelId.ToString();
        settings.Save();
        TaskDialog.Show("DesignFlow Sync", $"Synced {model.Name} ({model.FileName}). Status: {model.Status}.");
    }

    public static void Validate(UIApplication application)
    {
        var document = RequireProject(application);
        if (document is null)
        {
            return;
        }

        var settings = StudioSettings.Load();
        if (!Guid.TryParse(settings.LastRevitModelId, out var modelId))
        {
            TaskDialog.Show("DesignFlow Validation", "Sync the model first so the studio has a model id.");
            return;
        }

        var results = ModelChecks.Run(document);
        var client = new StudioClient(settings);
        var run = client.ValidateAsync(new
        {
            revitModelId = modelId,
            results
        }).GetAwaiter().GetResult();

        TaskDialog.Show(
            "DesignFlow Validation",
            $"{run.Status}: {run.ChecksPassed} passed, {run.ChecksFailed} failed. Issues created: {run.IssuesCreated}.");
    }

    public static void ShowRequirements(UIApplication application)
    {
        _ = application;
        var settings = StudioSettings.Load();
        if (!Guid.TryParse(settings.ProjectId, out _))
        {
            TaskDialog.Show("DesignFlow", "Choose a DesignFlow project in Studio settings.");
            return;
        }

        var project = new StudioClient(settings).ProjectAsync(settings.ProjectId).GetAwaiter().GetResult();
        var window = new RequirementsWindow(project);
        window.Show();
    }

    public static void QueueTranslation(UIApplication application)
    {
        _ = application;
        var settings = StudioSettings.Load();
        if (!Guid.TryParse(settings.ProjectId, out var projectId) || !Guid.TryParse(settings.LastRevitModelId, out var modelId))
        {
            TaskDialog.Show("DesignFlow Automation", "Sync the model first, and keep a project selected.");
            return;
        }

        var job = new StudioClient(settings).EnqueueAsync(new
        {
            projectId,
            revitModelId = modelId,
            jobType = "TranslateModel",
            requestPayload = "Queued from the Revit plugin"
        }).GetAwaiter().GetResult();

        TaskDialog.Show("DesignFlow Automation", $"Translation job {job.AutomationJobId} is {job.Status}.");
    }

    public static void EditSettings()
    {
        var settings = StudioSettings.Load();
        var window = new SettingsWindow(settings);
        window.ShowDialog();
    }

    private static Document? RequireProject(UIApplication application)
    {
        var document = application.ActiveUIDocument?.Document;
        if (document is null)
        {
            TaskDialog.Show("DesignFlow", "Open a Revit project first.");
            return null;
        }

        if (document.IsFamilyDocument)
        {
            TaskDialog.Show("DesignFlow", "Open a project model. Family documents are not synced.");
            return null;
        }

        return document;
    }
}

public sealed class StudioRequestHandler : IExternalEventHandler
{
    public string Action { get; set; } = string.Empty;

    public void Execute(UIApplication application)
    {
        try
        {
            switch (Action)
            {
                case "sync":
                    StudioActions.Sync(application);
                    break;
                case "validate":
                    StudioActions.Validate(application);
                    break;
                case "requirements":
                    StudioActions.ShowRequirements(application);
                    break;
                case "translate":
                    StudioActions.QueueTranslation(application);
                    break;
                case "settings":
                    StudioActions.EditSettings();
                    break;
            }
        }
        catch (Exception exception)
        {
            TaskDialog.Show("DesignFlow", exception.Message);
        }
    }

    public string GetName() => "DesignFlow AI plugins";
}

public static class StudioRuntime
{
    public static StudioRequestHandler Handler { get; } = new();

    public static ExternalEvent? Event { get; set; }

    public static void Raise(string action)
    {
        Handler.Action = action;
        Event?.Raise();
    }
}
