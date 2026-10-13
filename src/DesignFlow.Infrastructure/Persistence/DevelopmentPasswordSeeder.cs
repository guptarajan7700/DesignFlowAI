using DesignFlow.Application.Abstractions;
using DesignFlow.Infrastructure.Ai;
using DesignFlow.Infrastructure.Autodesk;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesignFlow.Infrastructure.Persistence;

public static class DevelopmentPasswordSeeder
{
    public const string PlaceholderHash = "DEV_ONLY_NOT_A_REAL_HASH";
    public const string DevelopmentPassword = "DesignFlow123!";

    public static async Task ReplacePlaceholderHashesAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DesignFlowDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DesignFlowDbContext>>();

        try
        {
            var users = await db.Users.Where(user => user.PasswordHash == PlaceholderHash).ToListAsync(cancellationToken);
            if (users.Count == 0)
            {
                return;
            }

            foreach (var user in users)
            {
                user.PasswordHash = hasher.Hash(DevelopmentPassword);
            }

            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Replaced {Count} development password placeholder(s). Local login password is the documented development password.", users.Count);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not update development password hashes. Check the SQL Server connection string.");
        }
    }
}
