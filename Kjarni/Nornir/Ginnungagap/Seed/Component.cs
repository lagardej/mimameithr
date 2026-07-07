using Friflo.Engine.ECS;

namespace Kjarni.Nornir.Ginnungagap.Seed;

/// <summary>World generation seed.</summary>
[ComponentKey("ginnungagap-seed")]
public struct SeedC : IComponent
{
    /// <summary>Unique id of the entity holding the universe's <see cref="SeedC" />.</summary>
    public const string Uid = "universe";

    /// <summary>World generation seed.</summary>
    public uint Seed;
}
