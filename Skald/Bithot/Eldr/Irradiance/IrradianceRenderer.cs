using Godot;

namespace Skald.Bithot.Eldr.Irradiance;

internal sealed class IrradianceRenderer
{
    internal void AttachStellarLight(Node parent, Vector3 direction, float energy = 1f)
    {
        var light = new StellarLight();
        parent.AddChild(light);
        light.Configure(direction, energy);
    }
}