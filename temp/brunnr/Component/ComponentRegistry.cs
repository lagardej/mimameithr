namespace Brunnr.Component;

/// <summary>
///     Registry for component instances. Used by the bootstrapper to schedule components across domains.
/// </summary>
public interface IComponentRegistry
{
    /// <summary>Register a component by name. Components are singletons.</summary>
    void Register(string componentName, BaseComponent component);

    /// <summary>Get a registered component by name.</summary>
    BaseComponent? Get(string componentName);

    /// <summary>Get all registered components.</summary>
    IReadOnlyDictionary<string, BaseComponent> GetAll();
}

/// <summary>In-memory implementation of <see cref="IComponentRegistry" />.</summary>
public sealed class ComponentRegistry : IComponentRegistry
{
    private readonly Dictionary<string, BaseComponent> _components = new();

    public void Register(string componentName, BaseComponent component) => _components[componentName] = component;

    public BaseComponent? Get(string componentName)
    {
        _components.TryGetValue(componentName, out var component);
        return component;
    }

    public IReadOnlyDictionary<string, BaseComponent> GetAll() => _components;
}
