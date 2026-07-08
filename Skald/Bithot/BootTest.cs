using Bifrost.Kart;
using Godot;
using Kjarni.Brunnr.Grid;
using Kjarni.Kvasir.Foundation;
using Kjarni.Kvasir.Foundation.Grid;
using Kjarni.Nornir;
using Kjarni.Nornir.Geimr.Geometry;
using Kjarni.Nornir.Geimr.Physics;
using Kjarni.Nornir.Hlothyn.Tectonics.MobileLid;
using UnitsNet;
using static Kjarni.Nornir.Hlothyn.Tectonics.MobileLid.BoundaryType;

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
	private float _distance;
	private Label _fpsLabel = null!;
	private bool _orbiting;
	private float _pitch = -0.3f;
	private float _visualRadius;
	private float _yaw;

	/// <inheritdoc />
	public override void _Ready()
	{
		GridProvider.Initialize(GridShape.Spherical, new BifrostKart());

		var nornir = new Nornir();
		var id = nornir.CreateEntity();

		nornir.Handle(new SetGeometry(id, 23, GridShape.Spherical, 38));
		nornir.Handle(new SetPhysics(id, 37, 28));

		var geometry = nornir.GetComponent<GeometryC>(id);
		var physics = nornir.GetComponent<PhysicsC>(id);

		GD.Print($"Entity {id}: AxialTilt={geometry.AxialTilt}, Radius={geometry.Radius}, GridShape={geometry.GridShape}");
		GD.Print($"Entity {id}: Age={physics.Age}, Mass={physics.Mass}, Gravity={physics.Gravity}");

		// Radius is thousands to billions of km. Scale down (km -> Mm) to keep the mesh a sane scene size.
		var visualRadius = (float)(geometry.Radius.Kilometers / 1000.0);
		_visualRadius = visualRadius;
		_distance = visualRadius * 4f;

		AddChild(CreateBody(visualRadius));

		var grid = (IGeodesicGrid)GridProvider.Get(GridShape.Spherical);
		var tectonics = Simulation.Run(new Parameters
		{
			BodyAge = physics.Age,
			BodyMass = physics.Mass,
			BodyRadius = geometry.Radius,
			BodySurfaceGravity = physics.Gravity,
			BoundaryFocus = Ratio.FromDecimalFractions(0.67),
			CollisionDominance = Ratio.FromDecimalFractions(0.44),
			Grid = grid,
			HotSpotDensity = Ratio.FromDecimalFractions(0.22),
			PlateCount = 15,
			PlateFragmentation = Ratio.FromDecimalFractions(0.78),
			PlateStability = Ratio.FromDecimalFractions(0.11)
		}, new StableRandom(42));

		DrawGrid(grid, visualRadius, tectonics);

		AddChild(CreateLight());

		_camera = CreateCamera();
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
				_pitch = Mathf.Clamp(_pitch - motion.Relative.Y * OrbitSensitivity, -MaxPitch, MaxPitch);
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

	private static MeshInstance3D CreateBody(float visualRadius)
	{
		return new MeshInstance3D
		{
			Mesh = new SphereMesh { Radius = visualRadius, Height = visualRadius * 2f },
			MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(0.2f, 0.25f, 0.3f) }
		};
	}

	private static DirectionalLight3D CreateLight()
	{
		return new DirectionalLight3D { RotationDegrees = new Vector3(-45f, -30f, 0f) };
	}

	private static Camera3D CreateCamera()
	{
		return new Camera3D();
	}

	private void DrawGrid(IGeodesicGrid grid, float visualRadius, Result tectonics)
	{
		var tectonicCells = tectonics.Cells;

		GD.Print($"Grid: {tectonicCells.Count} cells at resolution 2");
		GD.Print($"Plates: {tectonicCells.Values.Select(c => c.PlateSeedCellId).Distinct().Count()} unique plate seeds");

		//AddChild(CreateInstance(BuildPlateMesh(grid, tectonicCells, visualRadius), Colors.White, true));
		 AddChild(CreateInstance(BuildBoundaryMesh(grid, tectonicCells, visualRadius), Colors.White, true));
		// AddChild(CreateInstance(BuildEdgeMesh(grid, tectonicCells, visualRadius), new Color(0.95f, 0.95f, 0.95f)));
		AddChild(CreateInstance(BuildGuideMesh(visualRadius), new Color(1f, 0.9f, 0.2f)));
	}

	private static MeshInstance3D CreateInstance(Mesh mesh, Color colour, bool useVertexColour = false)
	{
		return new MeshInstance3D
		{
			Mesh = mesh,
			MaterialOverride = new StandardMaterial3D
			{
				AlbedoColor = colour,
				VertexColorUseAsAlbedo = useVertexColour,
				ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
			}
		};
	}

	/// <summary>Returns colour for a tectonic plate, indexed by position among all distinct plate seeds for max hue spread.</summary>
	private static Color PlateColour(CellId seedCellId, List<CellId> allSeeds)
	{
		var index = allSeeds.IndexOf(seedCellId);
		var hue = (double)index / allSeeds.Count;
		return Color.FromHsv((float)hue, 0.55f, 0.55f);
	}

	private static Color BoundaryColour(BoundaryType boundaryType)
	{
		return boundaryType switch
		{
			Convergent => new Color(1f, 0.35f, 0.2f),
			Divergent => new Color(0.2f, 0.7f, 1f),
			BoundaryType.Transform => new Color(0.9f, 0.85f, 0.25f),
			HotSpot => new Color(1f, 0.45f, 0.85f),
			_ => new Color(1f, 1f, 1f)
		};
	}

	private static void AddCellFan(SurfaceTool tool, Vector3 center, LatLng[] boundary, float radius, Color colour)
	{
		if (boundary.Length < 3) return;

		tool.SetColor(colour);
		for (var i = 0; i < boundary.Length; i++)
		{
			var a = boundary[i].ToUnitVector();
			var b = boundary[(i + 1) % boundary.Length].ToUnitVector();
			tool.AddVertex(center * radius);
			tool.AddVertex(new Vector3(b.X, b.Y, b.Z) * radius);
			tool.AddVertex(new Vector3(a.X, a.Y, a.Z) * radius);
		}
	}

	private static Mesh BuildPlateMesh(IGeodesicGrid grid, IReadOnlyDictionary<CellId, TectonicsCell> tectonics,
		float visualRadius)
	{
		var tool = new SurfaceTool();
		tool.Begin(Mesh.PrimitiveType.Triangles);

		var allSeeds = tectonics.Values.Select(c => c.PlateSeedCellId).Distinct().ToList();

		foreach (var (cellId, cell) in tectonics)
		{
			var boundary = grid.BoundaryOf(cellId);
			if (boundary.Length < 3) continue;

			var centerUnit = grid.CenterOf(cellId).ToUnitVector();
			var center = new Vector3(centerUnit.X, centerUnit.Y, centerUnit.Z);
			AddCellFan(tool, center, boundary, visualRadius * 1.08f, PlateColour(cell.PlateSeedCellId, allSeeds));
		}

		tool.GenerateNormals();
		return tool.Commit();
	}

	private static Mesh BuildBoundaryMesh(IGeodesicGrid grid, IReadOnlyDictionary<CellId, TectonicsCell> tectonics,
		float visualRadius)
	{
		var tool = new SurfaceTool();
		tool.Begin(Mesh.PrimitiveType.Triangles);

		foreach (var (cellId, cell) in tectonics)
		{
			if (cell.BoundaryType == None) continue;

			var boundary = grid.BoundaryOf(cellId);
			if (boundary.Length < 3) continue;

			var centerUnit = grid.CenterOf(cellId).ToUnitVector();
			var center = new Vector3(centerUnit.X, centerUnit.Y, centerUnit.Z);
			AddCellFan(tool, center, boundary, visualRadius * 1.12f, BoundaryColour(cell.BoundaryType));
		}

		tool.GenerateNormals();
		return tool.Commit();
	}

	private static Mesh BuildEdgeMesh(IGeodesicGrid grid, IReadOnlyDictionary<CellId, TectonicsCell> tectonics,
		float visualRadius)
	{
		var mesh = new ImmediateMesh();
		mesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

		foreach (var (cellId, _) in tectonics)
		{
			var boundary = grid.BoundaryOf(cellId);
			if (boundary.Length < 2) continue;

			for (var i = 0; i < boundary.Length; i++)
			{
				var a = boundary[i].ToUnitVector();
				var b = boundary[(i + 1) % boundary.Length].ToUnitVector();
				var start = new Vector3(a.X, a.Y, a.Z) * visualRadius * 1.01f;
				var end = new Vector3(b.X, b.Y, b.Z) * visualRadius * 1.01f;
				mesh.SurfaceAddVertex(start);
				mesh.SurfaceAddVertex(end);
			}
		}

		mesh.SurfaceEnd();
		return mesh;
	}

	private static Mesh BuildGuideMesh(float visualRadius)
	{
		var mesh = new ImmediateMesh();
		mesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

		const int guideSegments = 128;
		var guideRadius = visualRadius * 1.01f;
		var poleLength = guideRadius * 1.5f;

		for (var i = 0; i < guideSegments; i++)
		{
			var a0 = Mathf.Tau * i / guideSegments;
			var a1 = Mathf.Tau * (i + 1) / guideSegments;
			mesh.SurfaceAddVertex(new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)) * guideRadius);
			mesh.SurfaceAddVertex(new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * guideRadius);
		}

		mesh.SurfaceAddVertex(new Vector3(0f, -poleLength, 0f));
		mesh.SurfaceAddVertex(new Vector3(0f, poleLength, 0f));
		mesh.SurfaceEnd();
		return mesh;
	}
}
