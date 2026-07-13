using System.Collections.Generic;
using Bithot.Geimr.Geometry;
using Brunnr.Cell;
using Brunnr.Grid;
using Friflo.Engine.ECS;
using Godot;
using Kvasir.Grid;
using Nornir.Eldr.Luminosity;
using Nornir.Geimr.Geometry;
using Nornir.Geimr.Position;

namespace Bithot.Eldr.Irradiance;

/// <summary>
///     Builds one vertex-coloured overlay mesh per body from its R0 cell boundaries, coloured by
///     <see cref="IrradianceColorC" />. Attached as a child of the body's <see cref="BodyNodeC" /> node so it
///     inherits the body's position and rotation.
/// </summary>
/// <remarks>
///     Not registered in <see cref="BithotSystemRegistry" />/<see cref="Bithot.Advance" /> — it depends on
///     <see cref="BodyNodeC" />, which only exists once <c>BodyRenderer.AttachTo</c> has run. Instead
///     <see cref="Bithot" /> calls <see cref="Advance" /> directly, after body nodes are attached.
///     Rebuilds the whole mesh every call rather than updating vertex colours in place — geometry is static
///     but colour changes every tick, and a full rebuild is cheap at R0 scale (~122 cells/body). The
///     <see cref="MeshInstance3D" /> node itself is reused across calls (cached per body), only its
///     <see cref="MeshInstance3D.Mesh" /> is replaced, so this never grows the scene tree.
/// </remarks>
internal sealed class IrradianceMapRenderer
{
    private readonly Dictionary<int, MeshInstance3D> _nodes = new();
    private readonly EntityStore _store;

    internal IrradianceMapRenderer(EntityStore store)
    {
        _store = store;
    }

    /// <summary>Rebuilds the irradiance map mesh for every body that orbits a luminous parent (the star).</summary>
    internal void Advance()
    {
        foreach (var entity in _store.Query<GeometryC, OrbitParentC, BodyNodeC>().Entities)
        {
            if (!entity.GetComponent<OrbitParentC>().Parent.HasComponent<LuminosityC>())
            {
                continue;
            }

            BuildMesh(entity);
        }
    }

    private void BuildMesh(Entity body)
    {
        var geometry = body.GetComponent<GeometryC>();
        var visualRadius = body.GetComponent<VisualRadiusC>().Value * 1.001f;
        var grid = (IGeodesicGrid) GridProvider.Get(geometry.GridShape);

        var surface = new SurfaceTool();
        surface.Begin(Mesh.PrimitiveType.Triangles);

        foreach (var entity in _store.Query<IrradianceColorC, CellIdentityC, CellParentRefC>().Entities)
        {
            if (entity.GetComponent<CellParentRefC>().Parent != body)
            {
                continue;
            }

            var color = entity.GetComponent<IrradianceColorC>().Value;
            var cellId = entity.GetComponent<CellIdentityC>().Id;
            var center = ToGodot(grid.CenterOf(cellId).ToUnitVector()) * visualRadius;
            var boundary = grid.BoundaryOf(cellId);

            surface.SetColor(color);
            for (var i = 0; i < boundary.Length; i++)
            {
                var a = ToGodot(boundary[i].ToUnitVector()) * visualRadius;
                var b = ToGodot(boundary[(i + 1) % boundary.Length].ToUnitVector()) * visualRadius;
                surface.AddVertex(center);
                surface.AddVertex(a);
                surface.AddVertex(b);
            }
        }

        var mesh = new ArrayMesh();
        surface.Commit(mesh);

        if (!_nodes.TryGetValue(body.Id, out var instance))
        {
            instance = new MeshInstance3D
            {
                Name = "IrradianceMap",
                MaterialOverride = new StandardMaterial3D
                {
                    VertexColorUseAsAlbedo = true, ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
                }
            };
            body.GetComponent<BodyNodeC>().Node.AddChild(instance);
            _nodes[body.Id] = instance;
        }

        instance.Mesh = mesh;
    }

    private static Vector3 ToGodot(System.Numerics.Vector3 v)
    {
        return new Vector3(v.X, v.Y, v.Z);
    }
}
