using Godot;
using Kjarni.Brunnr.Grid;
using Kjarni.Nornir;
using Kjarni.Nornir.Geimr.Geometry;
using Kjarni.Nornir.Geimr.Physics;

namespace Skald.Bithot;

/// <summary>Smoke test for the Nornir command/query round-trip. No rendering.</summary>
public partial class BootTest : Node
{
	/// <inheritdoc />
	public override void _Ready()
	{
		var nornir = new Nornir();
		var id = nornir.CreateEntity();

		nornir.Handle(new SetPhysics(id, Age: 50, Mass: 50));
		nornir.Handle(new SetGeometry(id, AxialTilt: 23, GridShape: GridShape.Spherical, Radius: 50));

		var physics = nornir.GetComponent<PhysicsC>(id);
		var geometry = nornir.GetComponent<GeometryC>(id);

		GD.Print($"Entity {id}: Age={physics.Age}, Mass={physics.Mass}, Gravity={physics.Gravity}");
		GD.Print($"Entity {id}: AxialTilt={geometry.AxialTilt}, Radius={geometry.Radius}, GridShape={geometry.GridShape}");
	}
}
