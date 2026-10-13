using System.IO;
using System.Text.Json;

namespace DesignFlow.Revit.Studio;

public sealed class StudioSettings
{
    public string ApiUrl { get; set; } = "http://localhost:5080";

    public string Email { get; set; } = "arjun.mehta@designflow.ai";

    public string Password { get; set; } = "DesignFlow123!";

    public string ProjectId { get; set; } = "D4000001-0000-4000-8000-000000000001";

    public string? LastRevitModelId { get; set; }

    public static string FilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DesignFlowAI", "revit-settings.json");

    public static StudioSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                var created = new StudioSettings();
                created.Save();
                return created;
            }

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<StudioSettings>(json, JsonOptions()) ?? new StudioSettings();
        }
        catch
        {
            return new StudioSettings();
        }
    }

    public void Save()
    {
        var folder = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrWhiteSpace(folder))
        {
            Directory.CreateDirectory(folder);
        }

        File.WriteAllText(FilePath, JsonSerializer.Serialize(this, JsonOptions()));
    }

    private static JsonSerializerOptions JsonOptions() => new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
}
