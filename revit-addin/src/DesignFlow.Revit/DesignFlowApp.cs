using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using DesignFlow.Revit.Commands;
using DesignFlow.Revit.Studio;

namespace DesignFlow.Revit;

public sealed class DesignFlowApp : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        StudioRuntime.Event = ExternalEvent.Create(StudioRuntime.Handler);
        const string tab = "DesignFlow AI";
        try
        {
            application.CreateRibbonTab(tab);
        }
        catch (Exception)
        {
            // The tab already exists from a previous load.
        }

        var panel = application.CreateRibbonPanel(tab, "Plugins");
        Add(panel, "Sync", "Sync\nmodel", typeof(SyncCommand), "Send the open model to the studio.");
        Add(panel, "Validate", "Run\nvalidation", typeof(ValidationCommand), "Post door, room, and sheet checks. Failures become issues.");
        Add(panel, "Requirements", "Fetch\nrequirements", typeof(RequirementsCommand), "Show the requirements stored for the selected project.");
        Add(panel, "Translate", "Queue\ntranslation", typeof(TranslateCommand), "Queue a Model Derivative translation for the synced model.");
        Add(panel, "Studio", "Open\nstudio", typeof(OpenStudioCommand), "Open the plugin panel.");
        Add(panel, "Settings", "Studio\nsettings", typeof(SettingsCommand), "Choose the API, account, and project.");
        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;

    private static void Add(RibbonPanel panel, string name, string text, Type command, string toolTip)
    {
        var data = new PushButtonData(name, text, typeof(DesignFlowApp).Assembly.Location, command.FullName)
        {
            ToolTip = toolTip
        };
        if (panel.AddItem(data) is PushButton button)
        {
            button.LargeImage = Icon(32);
            button.Image = Icon(16);
        }
    }

    private static BitmapSource Icon(int size)
    {
        var pixels = new byte[size * size * 4];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var index = (y * size + x) * 4;
                var edge = x < 2 || y < 2 || x >= size - 2 || y >= size - 2;
                pixels[index] = edge ? (byte)20 : (byte)166;
                pixels[index + 1] = edge ? (byte)184 : (byte)70;
                pixels[index + 2] = edge ? (byte)166 : (byte)79;
                pixels[index + 3] = 255;
            }
        }

        var source = BitmapSource.Create(size, size, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null, pixels, size * 4);
        source.Freeze();
        return source;
    }
}
