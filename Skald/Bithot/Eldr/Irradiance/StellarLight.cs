using Godot;

namespace Skald.Bithot.Eldr.Irradiance;

/// <summary>Directional light representing a star. Direction and energy are set externally, not computed here.</summary>
public partial class StellarLight : DirectionalLight3D
{
    /// <summary>Sets light direction (light travels along this vector) and intensity.</summary>
    public void Configure(Vector3 direction, float energy = 1f)
    {
        LookAtFromPosition(Vector3.Zero, direction, Vector3.Up);
        LightEnergy = energy;
    }
}