using DesignFlow.Domain.Enums;

namespace DesignFlow.Application.Common;

public static class EnumParser
{
    public static T Parse<T>(string? value, string fieldName) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value) || !Enum.TryParse<T>(value.Trim(), ignoreCase: true, out var parsed))
        {
            var allowed = string.Join(", ", Enum.GetNames<T>());
            throw new AppValidationException($"{fieldName} must be one of: {allowed}.");
        }

        return parsed;
    }

    public static PriorityLevel ParsePriority(string? value) => Parse<PriorityLevel>(value, "Priority");
}
