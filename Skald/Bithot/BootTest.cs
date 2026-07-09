using Bifrost.Kart;
using Godot;
using Kjarni.Brunnr.Grid;
using Kjarni.Nornir;
using Kjarni.Nornir.Geimr.Geometry;
using Kjarni.Nornir.Geimr.Physics;
using Kjarni.Nornir.Ginnungagap.Seed;
using Kjarni.Nornir.Hlothyn.Tectonics.MobileLid;
using Skald.Bithot.Render;

namespace Skald.Bithot;

public partial class BootTest : Node3D
{
	private const float OrbitSensitivity = 0.01f;
	private const float ZoomStep = 0.1f;
	private const float MinDistanceFactor = 1.2f;
	private const float MaxDistanceFactor = 20f;
	private const float MaxPitch = Mathf.Pi / 2f - 0.05f;

	private MeshInstance3D _bodyMesh = null!;
	private MeshInstance3D _arrowMesh = null!;
	private MeshInstance3D _boundaryMesh = null!;
	private MeshInstance3D _edgeMesh = null!;
	private MeshInstance3D _guideMesh = null!;
	private MeshInstance3D _plateMesh = null!;
	private MeshInstance3D _seedOutlineMesh = null!;

	private Camera3D _camera = null!;
	private float _distance;
	private Label _fpsLabel = null!;
	private bool _orbiting;
	private float _pitch = -0.3f;
	private float _visualRadius;
	private float _yaw;

	private readonly LayerManager _layerManager = new();

	public override void _Ready()
	{
		SetupStage();
		SetUi();
		Lights();
		Camera();

		_layerManager.RebuildScene(_visualRadius);
	}

	private void SetupStage()
	{
		GridProvider.Initialize(GridShape.Spherical, new BifrostKart());

		var nornir = new Nornir();
		var id = nornir.CreateEntity();

		nornir.Handle(new SetSeed(42));
		nornir.Handle(new SetGeometry(id, AxialTilt: 23, GridShape.Spherical, Radius: 380));
		nornir.Handle(new SetPhysics(id, Age: 365, Mass: 365));
		nornir.Handle(new SetTectonicsMobileLid(id,
			BoundaryFocus: 67,
			CollisionDominance: 44,
			HotSpotDensity: 22,
			PlateCount: 15,
			PlateFragmentation: 78,
			PlateStability: 11
		));

		var geometry = nornir.GetComponent<GeometryC>(id);

		_visualRadius = VisualScale.ToVisualRadius(geometry.Radius);
		_distance = _visualRadius * 4f;
	}

	private void Lights() => AddChild(new DirectionalLight3D { RotationDegrees = new Vector3(-45f, -30f, 0f) });

	private void Camera()
	{
		_camera = new Camera3D();
		AddChild(_camera);
		_camera.Current = true;
		UpdateCameraTransform();
	}

	private void SetUi()
	{
		SetupFpsLabel();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventMouseButton { ButtonIndex: MouseButton.Right } mb:
				_orbiting = mb.Pressed;
				break;
			case InputEventMouseButton { ButtonIndex: MouseButton.WheelUp, Pressed: true }:
				Zoom(-ZoomStep);
				break;
			case InputEventMouseButton { ButtonIndex: MouseButton.WheelDown, Pressed: true }:
				Zoom(ZoomStep);
				break;
			case InputEventMouseMotion motion when _orbiting:
				_yaw -= motion.Relative.X * OrbitSensitivity;
				_pitch = Mathf.Clamp(_pitch - motion.Relative.Y * OrbitSensitivity, -MaxPitch, MaxPitch);
				UpdateCameraTransform();
				break;
		}
	}

	public override void _Process(double delta) => _fpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";

	private void Zoom(float factorDelta)
	{
		var minDistance = _visualRadius * MinDistanceFactor;
		var maxDistance = _visualRadius * MaxDistanceFactor;
		_distance = Mathf.Clamp(_distance * (1f + factorDelta), minDistance, maxDistance);
		UpdateCameraTransform();
	}

	private void UpdateCameraTransform()
	{
		var offset = new Vector3(
			_distance * Mathf.Cos(_pitch) * Mathf.Sin(_yaw),
			_distance * Mathf.Sin(_pitch),
			_distance * Mathf.Cos(_pitch) * Mathf.Cos(_yaw));
		_camera.LookAtFromPosition(offset, Vector3.Zero, Vector3.Up);
	}

	private void SetupFpsLabel()
	{
		var layer = new CanvasLayer();
		AddChild(layer);
		_fpsLabel = new Label { Position = new Vector2(8f, 8f) };
		layer.AddChild(_fpsLabel);
	}

	private static ImmediateMesh BuildGuideMesh(float layerRadius)
	{
		var mesh = new ImmediateMesh();
		mesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
		const int guideSegments = 128;
		var poleLength = layerRadius * 1.5f;
		for (var i = 0; i < guideSegments; i++)
		{
			var a0 = Mathf.Tau * i / guideSegments;
			var a1 = Mathf.Tau * (i + 1) / guideSegments;
			mesh.SurfaceAddVertex(new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)) * layerRadius);
			mesh.SurfaceAddVertex(new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * layerRadius);
		}

		mesh.SurfaceAddVertex(new Vector3(0f, -poleLength, 0f));
		mesh.SurfaceAddVertex(new Vector3(0f, poleLength, 0f));
		mesh.SurfaceEnd();
		return mesh;
	}
}
