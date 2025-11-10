using System.Reflection;
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
        if (ShouldSkipNormalization(instance, ref visited, out var type))
        {
            return;
        }

        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanRead || !property.CanWrite)
            {
                continue;
            }

            if (property.PropertyType == typeof(string))
            {
                NormalizeStringProperty(instance!, property, type);
                continue;
            }

            if (!property.PropertyType.IsValueType)
            {
                var nested = property.GetValue(instance);
                NormalizeStringProperties(nested, visited);
            }
        }
    }

    private static bool ShouldSkipNormalization(object? instance, ref HashSet<object>? visited, out Type type)
    {
        type = null!;

        if (instance is null)
        {
            return true;
        }

        visited ??= new HashSet<object>(ReferenceEqualityComparer.Instance);
        if (!visited.Add(instance))
        {
            return true;
        }

        type = instance.GetType();
        return type.IsPrimitive || type.IsEnum || instance is string;
    }

    private void NormalizeStringProperty(object instance, PropertyInfo property, Type declaringType)
    {
        var value = (string?)property.GetValue(instance);
        var normalized = Normalize(value);

        if (string.Equals(value, normalized, StringComparison.Ordinal))
        {
            return;
        }

        _logger.LogDebug("Normalized whitespace for property {PropertyName} on type {TypeName}.", property.Name, declaringType.Name);
        property.SetValue(instance, normalized);
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

