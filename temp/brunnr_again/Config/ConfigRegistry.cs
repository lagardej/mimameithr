using Brunnr.Engine.Domain;

namespace Brunnr.Engine.Config;

public sealed class ConfigRegistry<TConfig> where TConfig: IConfig
{
    private readonly Dictionary<DomainId, TConfig> _entries = new();

    public void Register(DomainId id, TConfig config) => _entries[id] = config;

    public TConfig Resolve(DomainId id) =>
        _entries.TryGetValue(id, out var entry)
            ? entry
            : throw new KeyNotFoundException($"No config registered for domain '{id.Value}'.");

    public IEnumerable<DomainId> Domains() => _entries.Keys;
}
