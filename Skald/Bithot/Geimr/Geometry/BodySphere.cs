using Godot;

namespace Bithot.Geimr.Geometry;

/// <summary>Mesh representing a spherical body (planet, moon, star). Radius is set externally, not computed here.</summary>
public partial class BodySphere : MeshInstance3D
{
    /// <summary>Sets the sphere's visual-space radius and surface colour.</summary>
    public void Configure(float radius, Color color)
    {
        Mesh = new SphereMesh { Radius = radius, Height = radius * 2f };
        MaterialOverride = new StandardMaterial3D { AlbedoColor = color };
    }
}