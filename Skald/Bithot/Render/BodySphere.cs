using Godot;

namespace Skald.Bithot.Render;

/// <summary>Mesh representing a spherical body (planet, moon, star). Radius is set externally, not computed here.</summary>
public partial class BodySphere : MeshInstance3D
{
    /// <summary>Sets the sphere's visual-space radius.</summary>
    public void Configure(float radius)
    {
        Mesh = new SphereMesh { Radius = radius, Height = radius * 2f };
    }
}