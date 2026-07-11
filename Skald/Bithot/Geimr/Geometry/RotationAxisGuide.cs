using Godot;
using Array = Godot.Collections.Array;

namespace Skald.Bithot.Geimr.Geometry;

/// <summary>Line marking a body's rotation axis, extending past both poles. Length is set externally, not computed here.</summary>
public partial class RotationAxisGuide : MeshInstance3D
{
    /// <summary>Sets the guide's visual-space half-length (line spans -length to +length along local Y) and colour.</summary>
    public void Configure(float length, Color? color = null)
    {
        var arrayMesh = new ArrayMesh();
        var points = new[] { new Vector3(0f, -length, 0f), new Vector3(0f, length, 0f) };

        var arrays = new Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = points;
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, arrays);

        Mesh = arrayMesh;
        MaterialOverride = new StandardMaterial3D
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoColor = color ?? new Color(1f, 0.8f, 0.2f)
        };
    }
}