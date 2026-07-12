using Friflo.Engine.ECS;

namespace Nornir.Hlothyn.Tectonics;

/// <summary>Tectonic regime selected for a planetary body.</summary>
[ComponentKey("hlothyn-tectonics-regime")]
public struct TectonicsRegimeC : IComponent
{
    /// <summary>Current tectonic regime.</summary>
    public Regime Regime;
}
