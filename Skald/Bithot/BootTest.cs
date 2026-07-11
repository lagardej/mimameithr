using Bifrost.Kart;
using Godot;
using Kjarni.Brunnr.Grid;
using Kjarni.Nornir;
using Kjarni.Nornir.Geimr.Geometry;
using Kjarni.Nornir.Ginnungagap.Seed;
using Kjarni.Nornir.Hlothyn.Tectonics.MobileLid;
using Skald.Bithot.Render;

namespace Skald.Bithot;

public partial class BootTest : Node3D
{
	private const uint Seed = 42;

	private readonly Nornir _engine = new();
	private int _bodyId;

	public override void _Ready()
	{
		GridProvider.Initialize(GridShape.Spherical, new BifrostKart());
		_engine.Handle(new SetSeed(Seed));
		_bodyId = EngineBootstrap.CreateTestBody(_engine);

		var geometry = _engine.GetComponent<GeometryC>(_bodyId);
		var sphereRadius = VisualScale.ToVisualRadius(geometry.Radius);

		var plateCount = _engine.Query<TectonicsPlateC>().Count();
		var boundaryCount = _engine.Query<TectonicsBoundaryC>().Count();
		GD.Print($"Engine booted. Body {_bodyId}: {plateCount} plate cells, {boundaryCount} boundary cells.");

		var sphere = new BodySphere();
		AddChild(sphere);
		sphere.Configure(sphereRadius);

		var light = new StellarLight();
		AddChild(light);
		light.Configure(new Vector3(-1f, -0.6f, -0.4f));

		var camera = new OrbitCamera();
		AddChild(camera);
		camera.Configure(sphereRadius);

		var equator = new EquatorGuide();
		AddChild(equator);
		equator.Configure(sphereRadius * 1.02f);

		var axis = new RotationAxisGuide();
		AddChild(axis);
		axis.Configure(sphereRadius * 1.5f);
	}
}
