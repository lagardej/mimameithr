using Godot;
using Kjarni.Nornir;
using Skald.Bithot.Geimr.Geometry;

namespace Skald.Bithot.Geimr.Orbit;

internal sealed class OrbitRenderer
{
    private readonly Nornir _nornir;

    internal OrbitRenderer(Nornir nornir)
    {
        _nornir = nornir;
    }

    internal void AttachCamera(Node parent)
    {
        var targetRadius = 0f;
        foreach (var bodyId in _nornir.Query<VisualRadiusC>())
        {
            var radius = _nornir.GetComponent<VisualRadiusC>(bodyId).Value;
            if (radius > targetRadius) targetRadius = radius;
        }

        var camera = new OrbitCamera();
        parent.AddChild(camera);
        camera.Configure(targetRadius);
    }
}