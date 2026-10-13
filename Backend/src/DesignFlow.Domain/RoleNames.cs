namespace DesignFlow.Domain;

/// <summary>Role names stored in dbo.Roles and emitted as JWT role claims.</summary>
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string ProjectManager = "ProjectManager";
    public const string Designer = "Designer";
    public const string Client = "Client";

    public const string Managers = Admin + "," + ProjectManager;
    public const string Staff = Admin + "," + ProjectManager + "," + Designer;

    public static bool IsManager(IEnumerable<string> roles) =>
        roles.Contains(Admin, StringComparer.Ordinal) || roles.Contains(ProjectManager, StringComparer.Ordinal);

    public static bool IsClientOnly(IEnumerable<string> roles) =>
        roles.Contains(Client, StringComparer.Ordinal) && !roles.Any(role => role is Admin or ProjectManager or Designer);
}
