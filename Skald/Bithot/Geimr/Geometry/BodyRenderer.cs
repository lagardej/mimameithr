using Friflo.Engine.ECS;
using Godot;
using Nornir.Eldr.Luminosity;
using Nornir.Geimr.Geometry;
using Nornir.Geimr.Position;

namespace Bithot.Geimr.Geometry;

internal sealed class BodyRenderer
{
    private readonly Nornir.Nornir _nornir;
    private readonly EntityStore _store;

    internal BodyRenderer(Nornir.Nornir nornir, EntityStore store)
    {
        _nornir = nornir;
        _store = store;
    }

    internal void AttachTo(Node parent)
    {
        foreach (var bodyId in _nornir.Query<VisualRadiusC>().ToList())
        {
            var entity = _store.GetEntityById(bodyId);
            var sphereRadius = _nornir.GetComponent<VisualRadiusC>(bodyId).Value;
            var realRadius = entity.GetComponent<GeometryC>().Radius;
            var color = DetermineColor(entity);

            // GD.Print($"Body_{bodyId}: real radius={realRadius.Kilometers:N0} km, visual radius={sphereRadius:F3}");

            var body = new Node3D { Name = $"Body_{bodyId}" };
            parent.AddChild(body);

            var sphere = new BodySphere();
            body.AddChild(sphere);
            sphere.Configure(sphereRadius, color);

            var equator = new EquatorGuide();
            body.AddChild(equator);
            equator.Configure(sphereRadius * 1.02f);

            var axis = new RotationAxisGuide();
            body.AddChild(axis);
            axis.Configure(sphereRadius * 1.5f);

            _store.GetEntityById(bodyId).AddComponent(new BodyNodeC { Node = body });
        }
    }

    /// <summary>
    ///     Classifies a body by orbital hierarchy to pick a display colour: radiates light → star; orbits a
    ///     star → planet; otherwise → moon. Render-only heuristic — no domain component carries body kind.
    /// </summary>
    private static Color DetermineColor(Entity entity)
    {
        if (entity.HasComponent<LuminosityC>())
            return Colors.Yellow;

        if (entity.HasComponent<OrbitParentC>() &&
            entity.GetComponent<OrbitParentC>().Parent.HasComponent<LuminosityC>())
            return Colors.Blue;

        return Colors.Gray;
    }
}