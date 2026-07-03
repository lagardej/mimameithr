using Friflo.Engine.ECS;

namespace Kjarni.Nornir.Wyrd.Ginnungagap;

/// <summary>World generation seed.</summary>
[ComponentKey("ginnungagap-seed")]
public struct SeedC : IComponent
{
    /// <summary>World generation seed.</summary>
    public uint Seed;
}
