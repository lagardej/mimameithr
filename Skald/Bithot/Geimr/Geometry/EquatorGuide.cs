using Godot;

namespace Skald.Bithot.Geimr.Geometry;

/// <summary>Line loop marking a body's equatorial plane. Radius is set externally, not computed here.</summary>
public partial class EquatorGuide : MeshInstance3D
{
    private const int Segments = 96;

    /// <summary>Sets the guide's visual-space radius and colour.</summary>
    public void Configure(float radius, Color? color = null)
    {
        var arrayMesh = new ArrayMesh();
        var points = new Vector3[Segments + 1];
        for (var i = 0; i <= Segments; i++)
        {
            var angle = Mathf.Tau * i / Segments;
            points[i] = new Vector3(radius * Mathf.Cos(angle), 0f, radius * Mathf.Sin(angle));
        }

        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = points;
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.LineStrip, arrays);

        Mesh = arrayMesh;
        MaterialOverride = new StandardMaterial3D
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoColor = color ?? new Color(0.3f, 0.6f, 1f),
        };
    }
}
