using Bifrost.Kart;
using Godot;
using Kjarni.Brunnr.Grid;
using Kjarni.Kvasir.Foundation.Grid;
using Kjarni.Nornir;
using Kjarni.Nornir.Geimr.Geometry;
using Kjarni.Nornir.Geimr.Physics;

namespace Skald.Bithot;

/// <summary>Smoke test for the Nornir command/query round-trip: renders the geodesic grid on a body-sized sphere.</summary>
public partial class BootTest : Node3D
{
	private const float OrbitSensitivity = 0.01f;
	private const float ZoomStep = 0.1f;
	private const float MinDistanceFactor = 1.2f;
	private const float MaxDistanceFactor = 20f;
	private const float MaxPitch = Mathf.Pi / 2f - 0.05f;

	private Camera3D _camera = null!;
	private Label _fpsLabel = null!;
	private float _visualRadius;
	private float _yaw;
	private float _pitch = -0.3f;
	private float _distance;
	private bool _orbiting;

	/// <inheritdoc />
	public override void _Ready()
	{
		GridProvider.Initialize(GridShape.Spherical, new BifrostKart());

		var nornir = new Nornir();
		var id = nornir.CreateEntity();

		nornir.Handle(new SetPhysics(id, Age: 50, Mass: 50));
		nornir.Handle(new SetGeometry(id, AxialTilt: 23, GridShape: GridShape.Spherical, Radius: 50));

		var physics = nornir.GetComponent<PhysicsC>(id);
		var geometry = nornir.GetComponent<GeometryC>(id);

		GD.Print($"Entity {id}: Age={physics.Age}, Mass={physics.Mass}, Gravity={physics.Gravity}");
		GD.Print($"Entity {id}: AxialTilt={geometry.AxialTilt}, Radius={geometry.Radius}, GridShape={geometry.GridShape}");

		// Radius is thousands to billions of km. Scale down (km -> Mm) to keep the mesh a sane scene size.
		var visualRadius = (float) (geometry.Radius.Kilometers / 1000.0);
		_visualRadius = visualRadius;
		_distance = visualRadius * 4f;

		var body = new MeshInstance3D
		{
			Mesh = new SphereMesh { Radius = visualRadius, Height = visualRadius * 2f },
			MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(0.2f, 0.25f, 0.3f) }
		};
		AddChild(body);

		DrawGrid((IGeodesicGrid) GridProvider.Get(GridShape.Spherical), visualRadius);

		var light = new DirectionalLight3D { RotationDegrees = new Vector3(-45f, -30f, 0f) };
		AddChild(light);

		_camera = new Camera3D();
		AddChild(_camera);
		_camera.Current = true;
		UpdateCameraTransform();

		SetupFpsLabel();
	}

	/// <inheritdoc />
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
				_pitch = Mathf.Clamp(_pitch - (motion.Relative.Y * OrbitSensitivity), -MaxPitch, MaxPitch);
				UpdateCameraTransform();
				break;
		}
	}

	/// <inheritdoc />
	public override void _Process(double delta)
	{
		_fpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
	}

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

		_fpsLabel = new Label
		{
			Position = new Vector2(8f, 8f)
		};
		layer.AddChild(_fpsLabel);
	}

	/// <summary>Draws R0 cell edges and yellow guides on the surface of a sphere of the given radius.</summary>
	private void DrawGrid(IGeodesicGrid grid, float visualRadius)
	{
		var edgeMesh = new ImmediateMesh();
		var guideMesh = new ImmediateMesh();

		var cells = grid.CellsAtResolution(Resolution.R0).ToArray();

		GD.Print($"Grid: {cells.Length} cells at resolution 0");

		edgeMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
		foreach (var cell in cells)
		{
			var boundary = grid.BoundaryOf(cell);
			if (boundary.Length < 2)
			{
				continue;
			}

			for (var i = 0; i < boundary.Length; i++)
			{
				var a = boundary[i].ToUnitVector();
				var b = boundary[(i + 1) % boundary.Length].ToUnitVector();
				var start = new Vector3(a.X, a.Y, a.Z) * visualRadius * 1.01f;
				var end = new Vector3(b.X, b.Y, b.Z) * visualRadius * 1.01f;
				edgeMesh.SurfaceAddVertex(start);
				edgeMesh.SurfaceAddVertex(end);
			}
		}
		edgeMesh.SurfaceEnd();

		guideMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
		const int guideSegments = 128;
		var guideRadius = visualRadius * 1.01f;
		var poleLength = guideRadius * 1.5f;

		for (var i = 0; i < guideSegments; i++)
		{
			var a0 = Mathf.Tau * i / guideSegments;
			var a1 = Mathf.Tau * (i + 1) / guideSegments;
			guideMesh.SurfaceAddVertex(new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)) * guideRadius);
			guideMesh.SurfaceAddVertex(new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * guideRadius);
		}

		guideMesh.SurfaceAddVertex(new Vector3(0f, -poleLength, 0f));
		guideMesh.SurfaceAddVertex(new Vector3(0f, poleLength, 0f));
		guideMesh.SurfaceEnd();

		var edgeInstance = new MeshInstance3D
		{
			Mesh = edgeMesh,
			MaterialOverride = new StandardMaterial3D
			{
				AlbedoColor = new Color(0.95f, 0.95f, 0.95f),
				ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
			}
		};
		AddChild(edgeInstance);

		var guideInstance = new MeshInstance3D
		{
			Mesh = guideMesh,
			MaterialOverride = new StandardMaterial3D
			{
				AlbedoColor = new Color(1f, 0.9f, 0.2f),
				ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
			}
		};
		AddChild(guideInstance);
	}
}
