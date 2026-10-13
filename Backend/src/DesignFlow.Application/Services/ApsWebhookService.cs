using System.Text.Json;
using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Contracts;
using DesignFlow.Domain.Entities;

namespace DesignFlow.Application.Services;

public sealed class ApsWebhookService : IApsWebhookService
{
    private readonly IRevitRepository _revit;
    private readonly IAuditRepository _audit;
    private readonly IUnitOfWork _unitOfWork;

    public ApsWebhookService(IRevitRepository revit, IAuditRepository audit, IUnitOfWork unitOfWork)
    {
        _revit = revit;
        _audit = audit;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApsWebhookAccepted> ReceiveAsync(string payload, CancellationToken cancellationToken)
    {
        var updated = 0;
        string? urn = null;
        var finished = false;
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(payload) ? "{}" : payload);
            urn = FindString(document.RootElement, "urn") ?? FindString(document.RootElement, "resourceUrn");
            var hook = FindString(document.RootElement, "hook") ?? FindString(document.RootElement, "event");
            var raw = payload.ToLowerInvariant();
            finished = raw.Contains("extraction.finished", StringComparison.Ordinal)
                || raw.Contains("success", StringComparison.Ordinal)
                || string.Equals(hook, "extraction.finished", StringComparison.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            finished = false;
        }

        if (finished && !string.IsNullOrWhiteSpace(urn))
        {
            updated = await _revit.MarkReadyByUrnAsync(urn, cancellationToken);
        }

        var stored = payload.Length > 4000 ? payload[..4000] : payload;
        await _audit.AddAsync(new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            EntityType = "ApsWebhook",
            Action = "Received",
            NewValues = stored
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new ApsWebhookAccepted("accepted", updated);
    }

    private static string? FindString(JsonElement element, string name)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase)
                    && property.Value.ValueKind == JsonValueKind.String)
                {
                    return property.Value.GetString();
                }

                var nested = FindString(property.Value, name);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    return nested;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nested = FindString(item, name);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    return nested;
                }
            }
        }

        return null;
    }
}
