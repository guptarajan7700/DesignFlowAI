using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DesignFlow.Revit.Studio;

namespace DesignFlow.Revit.Commands;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public sealed class SyncCommand : CommandBase
{
    protected override void Run(UIApplication application) => StudioActions.Sync(application);
}

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public sealed class ValidationCommand : CommandBase
{
    protected override void Run(UIApplication application) => StudioActions.Validate(application);
}

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public sealed class RequirementsCommand : CommandBase
{
    protected override void Run(UIApplication application) => StudioActions.ShowRequirements(application);
}

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public sealed class TranslateCommand : CommandBase
{
    protected override void Run(UIApplication application) => StudioActions.QueueTranslation(application);
}

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public sealed class SettingsCommand : CommandBase
{
    protected override void Run(UIApplication application) => StudioActions.EditSettings();
}

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public sealed class OpenStudioCommand : CommandBase
{
    protected override void Run(UIApplication application)
    {
        _ = application;
        var panel = new Ui.StudioPanel();
        panel.Show();
    }
}

public abstract class CommandBase : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            Run(commandData.Application);
            return Result.Succeeded;
        }
        catch (Exception exception)
        {
            message = exception.Message;
            TaskDialog.Show("DesignFlow", exception.Message);
            return Result.Failed;
        }
    }

    protected abstract void Run(UIApplication application);
}
