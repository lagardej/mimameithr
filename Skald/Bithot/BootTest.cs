using Godot;
using Skald.Bithot.Render;

namespace Skald.Bithot;

public partial class BootTest : Node3D
{
    private const float SphereRadius = 1f;

    public override void _Ready()
    {
        var sphere = new BodySphere();
        AddChild(sphere);
        sphere.Configure(SphereRadius);

        var light = new StellarLight();
        AddChild(light);
        light.Configure(new Vector3(-1f, -0.6f, -0.4f));

        var camera = new OrbitCamera();
        AddChild(camera);
        camera.Configure(SphereRadius);
    }
}
