using Brunnr.Domain;

namespace Brunnr;

/// <summary>
///     Engine bootstrap configuration.
///     Stores which components are enabled for each domain and their binding identifiers.
/// </summary>
public sealed class EngineConfiguration
{
    private readonly Dictionary<DomainId, ConfiguredDomain> _domains = new();

    public IReadOnlyCollection<ConfiguredDomain> Domains => _domains.Values;

    public ConfiguredDomain AddDomain(DomainId domainId)
    {
        if (_domains.TryGetValue(domainId, out var existing))
        {
            return existing;
        }

        var created = new ConfiguredDomain(domainId);
        _domains[domainId] = created;
        return created;
    }

    public EngineConfiguration BindComponent(DomainId domainId, string componentName, string binding = "")
    {
        AddDomain(domainId).Bind(componentName, binding);
        return this;
    }

    public EngineConfiguration SetParameters<TParameters>(DomainId domainId, TParameters parameters)
        where TParameters : class
    {
        AddDomain(domainId).SetParameters(parameters);
        return this;
    }

    public bool TryGetDomain(DomainId domainId, out ConfiguredDomain? domain)
    {
        var found = _domains.TryGetValue(domainId, out var value);
        domain = value;
        return found;
    }
}

/// <summary>Configuration for one domain.</summary>
public sealed class ConfiguredDomain(DomainId id)
{
    private readonly Dictionary<string, string> _bindings = new();
    private readonly Dictionary<Type, object> _parameters = new();

    public DomainId Id { get; } = id;
    public IReadOnlyDictionary<string, string> Bindings => _bindings;

    public ConfiguredDomain Bind(string componentName, string binding = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(componentName);
        _bindings[componentName] = binding;
        return this;
    }

    public bool TryGetBinding(string componentName, out string binding) =>
        _bindings.TryGetValue(componentName, out binding!);

    public ConfiguredDomain SetParameters<TParameters>(TParameters parameters)
        where TParameters : class
    {
        _parameters[typeof(TParameters)] = parameters;
        return this;
    }

    public bool TryGetParameters<TParameters>(out TParameters? parameters)
        where TParameters : class
    {
        var found = _parameters.TryGetValue(typeof(TParameters), out var value);
        parameters = value as TParameters;
        return found;
    }
}
