using Kjarni.Brunnr.Engine.Data.Validation;
using System.Collections.Concurrent;
using System.Reflection;
using UnitsNet;

namespace Kjarni.Brunnr.Engine.Data.Normalization;

/// <summary>
///     Reflects over a record's properties and normalizes values declared via attributes.
///     Returns a copy with normalized values. Run before <see cref="Validator.Validate{T}" />.
/// </summary>
public static class Normalizer
{
    private static readonly ConcurrentDictionary<Type, PropertyMeta[]> s_cache = new();
    private static readonly ConcurrentDictionary<Type, MethodInfo> s_cloneCache = new();

    /// <summary>Normalizes a record's properties according to declared attributes.</summary>
    /// <typeparam name="T">Record type with properties to normalize.</typeparam>
    /// <param name="record">Record instance to normalize.</param>
    /// <returns>Copy of the record with normalized values, or the original if no changes were needed.</returns>
    public static T Normalize<T>(T record) where T : notnull
    {
        var metas = s_cache.GetOrAdd(typeof(T), BuildMeta);
        var overrides = new Dictionary<string, object>();

        foreach (var meta in metas)
        {
            var value = meta.Property.GetValue(record)!;
            var normalized = meta.Apply(value, meta.Property.Name);
            if (!normalized.Equals(value))
            {
                overrides[meta.Property.Name] = normalized;
            }
        }

        if (overrides.Count == 0)
        {
            return record;
        }

        var clone = s_cloneCache.GetOrAdd(typeof(T), t =>
            t.GetMethod("<Clone>$") ?? throw new InvalidOperationException($"{t.Name} is not a record type."));
        var copy = (T) clone.Invoke(record, null)!;
        foreach (var (name, value) in overrides)
        {
            typeof(T).GetProperty(name)!.SetValue(copy, value);
        }

        return copy;
    }

    private static PropertyMeta[] BuildMeta(Type type)
    {
        var result = new List<PropertyMeta>();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetCustomAttribute<WrapAroundAttribute>() is not { } attr)
            {
                continue;
            }

            var min = attr.Min;
            var max = attr.Max;
            var unit = attr.Unit;

            result.Add(new PropertyMeta(prop,
                (v, n) => v is not IQuantity q
                    ? throw new InvalidOperationException($"[WrapAround] on '{n}' requires IQuantity.")
                    : WrapAround(q, min, max, unit, n)));
        }

        return result.ToArray();
    }

    private static IQuantity WrapAround(IQuantity value, double min, double max, Enum unit, string? parameterName)
    {
        var v = value.As(unit);
        if (double.IsNaN(v) || double.IsInfinity(v))
        {
            throw new ArgumentOutOfRangeException(parameterName, value.ToString(),
                $"{value.GetType().Name} must be finite.");
        }

        var range = max - min;
        var wrapped = (v - min) % range;
        if (wrapped < 0)
        {
            wrapped += range;
        }

        return Quantity.From(wrapped + min, unit);
    }

    private sealed record PropertyMeta(PropertyInfo Property, Func<object, string, object> Apply);
}
