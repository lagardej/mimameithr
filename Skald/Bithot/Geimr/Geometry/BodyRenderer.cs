using Godot;
using Kjarni.Nornir;

namespace Skald.Bithot.Geimr.Geometry;

internal sealed class BodyRenderer
{
    private readonly Nornir _nornir;

    internal BodyRenderer(Nornir nornir)
    {
        _nornir = nornir;
    }

    internal void AttachTo(Node parent)
    {
        foreach (var bodyId in _nornir.Query<VisualRadiusC>())
        {
            var sphereRadius = _nornir.GetComponent<VisualRadiusC>(bodyId).Value;

            var body = new Node3D { Name = $"Body_{bodyId}" };
            parent.AddChild(body);

            var sphere = new BodySphere();
            body.AddChild(sphere);
            sphere.Configure(sphereRadius);

            var equator = new EquatorGuide();
            body.AddChild(equator);
            equator.Configure(sphereRadius * 1.02f);

            var axis = new RotationAxisGuide();
            body.AddChild(axis);
            axis.Configure(sphereRadius * 1.5f);
        }
    }
}