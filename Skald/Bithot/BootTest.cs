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

	private const float GuideLevel = 1.01f;
	private const float EdgeLevel = 1.01f;
	private const float PlateLevel = 1.08f;
	private const float BoundaryLevel = 1.12f;
	private const float ArrowLevel = 1.14f;
	private const float SeedOutlineLevel = 1.18f;

	// Real body radius spans 1 km (asteroid) to 1e9 km (supergiant star) — 9 orders of magnitude,
	// far beyond what float32 scene coordinates can hold without precision collapse. Map log-scale
	// into a fixed, bounded scene-space range instead: preserves relative size ordering between
	// bodies (star > planet > moon) without letting real units leak into scene-space distances.
	private const double MinRealRadiusKm = 1.0;
	private const double MaxRealRadiusKm = 1_000_000_000.0;
	private const float MinVisualRadius = 0.05f;
	private const float MaxVisualRadius = 40f;

	private Camera3D _camera = null!;
	private float _distance;
	private Label _fpsLabel = null!;
	private bool _orbiting;
	private float _pitch = -0.3f;
	private float _visualRadius;
	private float _yaw;

	private MeshInstance3D _plateMesh = null!;
	private MeshInstance3D _arrowMesh = null!;
	private MeshInstance3D _seedOutlineMesh = null!;
	private MeshInstance3D _boundaryMesh = null!;
	private MeshInstance3D _edgeMesh = null!;
	private MeshInstance3D _guideMesh = null!;

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

		// Real radius doesn't drive scene-space size directly — see ToVisualRadius.
		var visualRadius = ToVisualRadius(geometry.Radius);
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
		SetupToggleUi();
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

	private void SetupToggleUi()
	{
		var layer = new CanvasLayer();
		AddChild(layer);

		var container = new VBoxContainer { Position = new Vector2(8f, 40f) };
		layer.AddChild(container);

		AddToggle(container, "Plates + arrows", visible =>
		{
			_plateMesh.Visible = visible;
			_arrowMesh.Visible = visible;
			_seedOutlineMesh.Visible = visible;
		});
		AddToggle(container, "Boundaries", visible => _boundaryMesh.Visible = visible);
		AddToggle(container, "Grid", visible => _edgeMesh.Visible = visible);
		AddToggle(container, "Guides", visible => _guideMesh.Visible = visible);
	}

	private static void AddToggle(Node parent, string label, Action<bool> onToggle)
	{
		var checkBox = new CheckBox { Text = label, ButtonPressed = true };
		checkBox.Toggled += visible => onToggle(visible);
		parent.AddChild(checkBox);
	}

	private static MeshInstance3D CreateBody(float visualRadius)
	{
		return new MeshInstance3D
		{
			Mesh = new SphereMesh { Radius = visualRadius, Height = visualRadius * 2f },
			MaterialOverride = new StandardMaterial3D { AlbedoColor = Colors.DarkSlateGray }
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

	/// <summary>
	///     Maps real body radius to a bounded scene-space radius via log scale. Preserves relative size
	///     ordering between bodies without letting real units (1 km – 1e9 km) leak into scene coordinates,
	///     where float32 precision would collapse at the extremes of that range.
	/// </summary>
	private static float ToVisualRadius(Length radius)
	{
		var km = Math.Clamp(radius.Kilometers, MinRealRadiusKm, MaxRealRadiusKm);
		var t = (Math.Log10(km) - Math.Log10(MinRealRadiusKm)) / (Math.Log10(MaxRealRadiusKm) - Math.Log10(MinRealRadiusKm));
		return (float) (MinVisualRadius + t * (MaxVisualRadius - MinVisualRadius));
	}

	private void DrawGrid(IGeodesicGrid grid, float visualRadius, Result tectonics)
	{
		var tectonicCells = tectonics.Cells;

		GD.Print($"Grid: {tectonicCells.Count} cells at resolution 2");
		GD.Print(
			$"Plates: {tectonicCells.Values.Select(c => c.PlateSeedCellId).Distinct().Count()} unique plate seeds");

		_guideMesh = CreateInstance(BuildGuideMesh(visualRadius), Colors.Yellow);
		AddChild(_guideMesh);

		_edgeMesh = CreateInstance(BuildEdgeMesh(grid, tectonicCells, visualRadius), Colors.WhiteSmoke);
		AddChild(_edgeMesh);

		_plateMesh = CreateInstance(BuildPlateMesh(grid, tectonicCells, visualRadius), Colors.White, true);
		AddChild(_plateMesh);

		_boundaryMesh = CreateInstance(BuildBoundaryMesh(grid, tectonicCells, visualRadius), Colors.White, true);
		AddChild(_boundaryMesh);

		_arrowMesh = CreateInstance(BuildVelocityArrowMesh(grid, tectonicCells, visualRadius), Colors.Black);
		AddChild(_arrowMesh);

		_seedOutlineMesh = CreateInstance(BuildSeedOutlineMesh(grid, tectonicCells, visualRadius), Colors.Red);
		AddChild(_seedOutlineMesh);
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
			Convergent => Colors.OrangeRed,
			Divergent => Colors.DeepSkyBlue,
			BoundaryType.Transform => Colors.Gold,
			HotSpot => Colors.HotPink,
			_ => Colors.White
		};
	}

	/// <summary>
	///     Builds one small arc-shaped arrow per R0 plate cell, showing that cell's local velocity.
	///     The arc curves around the plate's rotation axis (Euler pole) — rotating a point about an axis
	///     through the sphere's centre keeps it on the sphere for free, so the arc needs no reprojection.
	///     Length/curvature scale with local speed. Winding matches <see cref="AddCellFan" />'s outward convention.
	/// </summary>
	private static ArrayMesh BuildVelocityArrowMesh(IGeodesicGrid grid, IReadOnlyDictionary<CellId, TectonicsCell> tectonics,
		float visualRadius)
	{
		var tool = new SurfaceTool();
		tool.Begin(Mesh.PrimitiveType.Triangles);

		// Plate angular velocity is constant across a plate but only stored per-R2-cell in the result;
		// fold it down to one entry per R0 (plate) cell before drawing.
		var r0AngularVelocities = new Dictionary<CellId, Vector3>();
		foreach (var (r2CellId, cell) in tectonics)
		{
			var r0Cell = grid.ParentOf(r2CellId, Resolution.R0);
			r0AngularVelocities[r0Cell] = new Vector3(
				cell.PlateAngularVelocity.X, cell.PlateAngularVelocity.Y, cell.PlateAngularVelocity.Z);
		}

		foreach (var r0Cell in grid.RootCells())
		{
			if (!r0AngularVelocities.TryGetValue(r0Cell, out var angularVelocity)) continue;

			var posUnit = grid.CenterOf(r0Cell).ToUnitVector();
			var normal = new Vector3(posUnit.X, posUnit.Y, posUnit.Z);

			AddArcArrow(tool, normal, angularVelocity, visualRadius);
		}

		tool.GenerateNormals();
		return tool.Commit();
	}

	/// <summary>Appends one small curved arrow at <paramref name="normal" />'s position into <paramref name="tool" />.</summary>
	private static void AddArcArrow(SurfaceTool tool, Vector3 normal, Vector3 angularVelocity, float visualRadius)
	{
		if (angularVelocity.LengthSquared() < 1e-9f) return;

		var axis = angularVelocity.Normalized();
		var localSpeed = angularVelocity.Cross(normal).Length();
		if (localSpeed < 1e-6f) return;

		const int samples = 4;
		var arcAngle = Mathf.Clamp(localSpeed * 0.15f, 0.012f, 0.05f);
		var liftRadius = visualRadius * ArrowLevel;

		var points = new Vector3[samples];
		for (var i = 0; i < samples; i++)
		{
			var t = (float) i / (samples - 1);
			points[i] = normal.Rotated(axis, arcAngle * t) * liftRadius;
		}

		var shaftWidth = visualRadius * 0.0035f;

		for (var i = 0; i < samples - 1; i++)
		{
			var a = points[i];
			var b = points[i + 1];
			var segNormal = a.Normalized();
			var direction = (b - a).Normalized();
			var side = segNormal.Cross(direction).Normalized();

			if (i == samples - 2)
			{
				// Tip segment: taper the shaft into a small triangular head.
				var p0 = a - side * (shaftWidth * 0.5f);
				var p1 = a + side * (shaftWidth * 0.5f);
				tool.AddVertex(p0); tool.AddVertex(p1); tool.AddVertex(b);
			}
			else
			{
				var p0 = a - side * (shaftWidth * 0.5f);
				var p1 = a + side * (shaftWidth * 0.5f);
				var p2 = b + side * (shaftWidth * 0.5f);
				var p3 = b - side * (shaftWidth * 0.5f);
				tool.AddVertex(p0); tool.AddVertex(p1); tool.AddVertex(p2);
				tool.AddVertex(p0); tool.AddVertex(p2); tool.AddVertex(p3);
			}
		}
	}

	/// <summary>Builds a bold outline around each plate's seed cell boundary, marking the plate's anchor point.</summary>
	private static ArrayMesh BuildSeedOutlineMesh(IGeodesicGrid grid, IReadOnlyDictionary<CellId, TectonicsCell> tectonics,
		float visualRadius)
	{
		var tool = new SurfaceTool();
		tool.Begin(Mesh.PrimitiveType.Triangles);

		var outlineWidth = visualRadius * 0.012f;
		var liftRadius = visualRadius * SeedOutlineLevel;

		var seenSeeds = new HashSet<CellId>();
		foreach (var cell in tectonics.Values)
		{
			if (!seenSeeds.Add(cell.PlateSeedCellId)) continue;

			var boundary = grid.BoundaryOf(cell.PlateSeedCellId);
			if (boundary.Length < 2) continue;

			for (var i = 0; i < boundary.Length; i++)
			{
				var aUnit = boundary[i].ToUnitVector();
				var bUnit = boundary[(i + 1) % boundary.Length].ToUnitVector();
				var a = new Vector3(aUnit.X, aUnit.Y, aUnit.Z);
				var b = new Vector3(bUnit.X, bUnit.Y, bUnit.Z);

				var mid = ((a + b) * 0.5f).Normalized();
				var direction = (b - a).Normalized();
				var side = mid.Cross(direction).Normalized();

				var p0 = a * liftRadius - side * (outlineWidth * 0.5f);
				var p1 = a * liftRadius + side * (outlineWidth * 0.5f);
				var p2 = b * liftRadius + side * (outlineWidth * 0.5f);
				var p3 = b * liftRadius - side * (outlineWidth * 0.5f);

				tool.AddVertex(p0); tool.AddVertex(p1); tool.AddVertex(p2);
				tool.AddVertex(p0); tool.AddVertex(p2); tool.AddVertex(p3);
			}
		}

		tool.GenerateNormals();
		return tool.Commit();
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

	private static ArrayMesh BuildPlateMesh(IGeodesicGrid grid, IReadOnlyDictionary<CellId, TectonicsCell> tectonics,
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
			AddCellFan(tool, center, boundary, visualRadius * PlateLevel, PlateColour(cell.PlateSeedCellId, allSeeds));
		}

		tool.GenerateNormals();
		return tool.Commit();
	}

	private static ArrayMesh BuildBoundaryMesh(IGeodesicGrid grid, IReadOnlyDictionary<CellId, TectonicsCell> tectonics,
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
			AddCellFan(tool, center, boundary, visualRadius * BoundaryLevel, BoundaryColour(cell.BoundaryType));
		}

		tool.GenerateNormals();
		return tool.Commit();
	}

	private static ImmediateMesh BuildEdgeMesh(IGeodesicGrid grid, IReadOnlyDictionary<CellId, TectonicsCell> tectonics,
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
				var start = new Vector3(a.X, a.Y, a.Z) * visualRadius * EdgeLevel;
				var end = new Vector3(b.X, b.Y, b.Z) * visualRadius * EdgeLevel;
				mesh.SurfaceAddVertex(start);
				mesh.SurfaceAddVertex(end);
			}
		}

		mesh.SurfaceEnd();
		return mesh;
	}

	private static ImmediateMesh BuildGuideMesh(float visualRadius)
	{
		var mesh = new ImmediateMesh();
		mesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

		const int guideSegments = 128;
		var guideRadius = visualRadius * GuideLevel;
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
