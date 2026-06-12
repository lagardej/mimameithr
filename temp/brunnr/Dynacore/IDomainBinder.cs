namespace Brunnr.Dynacore;

/// <summary>
///     Instantiates and schedules all components for configured domains,
///     resolving DynaCores from <see cref="IDynaCoreRegistry" />.
/// </summary>
public interface IDomainBinder
{
    void Bind(EngineConfiguration configuration);
}
