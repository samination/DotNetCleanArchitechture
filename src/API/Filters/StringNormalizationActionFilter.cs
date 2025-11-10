using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

public class StringNormalizationActionFilter : IActionFilter
{
    private readonly ILogger<StringNormalizationActionFilter> _logger;

    public StringNormalizationActionFilter(ILogger<StringNormalizationActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            NormalizeStringProperties(argument);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No-op
    }

    private void NormalizeStringProperties(object? instance, HashSet<object>? visited = null)
    {
        if (instance is null)
        {
            return;
        }

        visited ??= new HashSet<object>(ReferenceEqualityComparer.Instance);

        if (!visited.Add(instance))
        {
            return;
        }

        var type = instance.GetType();

        if (type.IsPrimitive || type.IsEnum)
        {
            return;
        }

        if (instance is string)
        {
            return;
        }

        foreach (var property in type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
        {
            if (!property.CanRead || !property.CanWrite)
            {
                continue;
            }

            if (property.PropertyType == typeof(string))
            {
                var value = (string?)property.GetValue(instance);
                var normalized = Normalize(value);

                if (!string.Equals(value, normalized, StringComparison.Ordinal))
                {
                    _logger.LogDebug("Normalized whitespace for property {PropertyName} on type {TypeName}.", property.Name, type.Name);
                    property.SetValue(instance, normalized);
                }
            }
            else if (!property.PropertyType.IsValueType)
            {
                var nested = property.GetValue(instance);
                NormalizeStringProperties(nested, visited);
            }
        }
    }

    private static string? Normalize(string? input)
    {
        if (input is null)
        {
            return null;
        }

        var trimmed = input.Trim();
        return trimmed;
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static ReferenceEqualityComparer Instance { get; } = new();

        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }
}

