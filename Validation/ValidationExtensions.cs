using FluentValidation.Results;

namespace RestaurantApi.Validation;

public static class ValidationExtensions
{
    public static Dictionary<string, string[]> ToProblemDetails(this ValidationResult result)
    {
        return result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => ToCamelCaseFirst(g.Key),
                g => g.Select(e => e.ErrorMessage).Distinct().ToArray()
            );
    }

    private static string ToCamelCaseFirst(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        if (input.Length == 1) return input.ToLowerInvariant();
        return char.ToLowerInvariant(input[0]) + input[1..];
    }
}
