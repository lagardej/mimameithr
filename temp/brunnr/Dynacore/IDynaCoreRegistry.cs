namespace Brunnr.Dynacore;

/// <summary>
///     Maps (component type name, DynaCore name) pairs to factories.
///     Defined in Brunnr; populated by Nornir at startup.
/// </summary>
public interface IDynaCoreRegistry
{
    void Register(string componentId, string dynaCoreId, Func<IDynaCore> factory);
    IDynaCore Resolve(string componentId, string dynaCoreId);
}
