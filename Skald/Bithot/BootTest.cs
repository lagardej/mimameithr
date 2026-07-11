using Godot;
using Kjarni.Nornir;
using Kjarni.Nornir.Hlothyn.Tectonics.MobileLid;
using Skald.Bithot.Render;

namespace Skald.Bithot;

public partial class BootTest : Node3D
{
	private const float SphereRadius = 1f;

	private Nornir? _engine;
	private int _bodyId;

	public override void _Ready()
	{
		(_engine, _bodyId) = EngineBootstrap.CreateTestBody();
		GD.Print($"Engine booted. Body {_bodyId} tectonics generated.");

		var sphere = new BodySphere();
		AddChild(sphere);
		sphere.Configure(SphereRadius);

		var light = new StellarLight();
		AddChild(light);
		light.Configure(new Vector3(-1f, -0.6f, -0.4f));

		var camera = new OrbitCamera();
		AddChild(camera);
		camera.Configure(SphereRadius);

		var equator = new EquatorGuide();
		AddChild(equator);
		equator.Configure(SphereRadius * 1.02f);

		var axis = new RotationAxisGuide();
		AddChild(axis);
		axis.Configure(SphereRadius * 1.5f);
	}
}
