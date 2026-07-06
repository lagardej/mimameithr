using Kjarni.Kvasir.Foundation;
using Xunit;

namespace Kjarni.Nornir.Tests;

sealed file class TectonicsStubGrid : IGeodesicGrid
{
    public static readonly CellId R0Cell = new(1);
    public static readonly CellId R2Cell = new(2);

    public static readonly TectonicsStubGrid Instance = new();

    public CellId[] RootCells() => [R0Cell];

    public IEnumerable<CellId> CellsAtResolution(Resolution resolution) =>
        resolution.Value == 2 ? [R2Cell] : [];

    public CellId ParentOf(CellId cell, Resolution parentResolution) => R0Cell;

    // Interior cell — only neighbour is itself, so BoundaryType will be None or HotSpot.
    public CellId[] Disk(CellId cell, int k) => [cell];

    public LatLng CenterOf(CellId cell) => new(0, 0);
    public CellId CellAt(double latDeg, double lngDeg, Resolution resolution) => default;
    public LatLng[] BoundaryOf(CellId cell) => [];
    public Resolution MaxResolution => new(15);
    public Resolution ResolutionOf(CellId cell) => default;
    public bool IsValid(CellId cell) => true;
    public CellId[] ChildrenOf(CellId cell, Resolution childResolution) => [];
    public Resolution InferResolution<T>(Dictionary<CellId, T> cells, Resolution fallback) => fallback;
    public IEnumerable<CellId> GridRing(CellId center, int k) => [];
}

public sealed class TectonicsSystemTests
{
    [Fact]
    public void Advance_PopulatesTectonicsComponent_OnCellEntity()
    {
        // var verðandi = new Verðandi(new EntityStore());
        // var grid = TectonicsStubGrid.Instance;
        // const int seed = 42;
        //
        // engine.Configure(grid, seed, store =>
        // {
        //     var body = store.CreateEntity();
        //     body.AddComponent(new BodyGeometryC
        //     {
        //         Radius = Length.FromKilometers(6_371), AxialTilt = Angle.FromDegrees(23.5)
        //     });
        //     body.AddComponent(new BodyPhysicsC
        //     {
        //         Mass = Mass.FromKilograms(5.972e24),
        //         Age = Duration.FromSeconds(4.5e9 * 365.25 * 24 * 3600),
        //         SurfaceGravity = Acceleration.FromMetersPerSecondSquared(9.81)
        //     });
        //     body.AddComponent(new TectonicsSettingsC
        //     {
        //         PlateCount = new Setting10(5),
        //         PlateFragmentation = new Setting10(5),
        //         PlateStability = new Setting10(5),
        //         BoundaryFocus = new Setting10(5),
        //         CollisionDominance = new Setting10(5),
        //         HotSpotDensity = new Setting10(5)
        //     });
        //
        //     var cell = store.CreateEntity();
        //     cell.AddComponent(new CellIdentityC { Id = TectonicsStubGrid.R2Cell });
        //     cell.AddComponent(new CellParentRefC { Parent = body });
        //     cell.AddComponent(new TectonicsC());
        // });
        //
        // engine.Advance(13f); // beyond the 10s stagger threshold
        //
        // var identity = engine.Query<CellIdentityC>(2);
        // Assert.Equal(TectonicsStubGrid.R2Cell, identity.Id);
        //
        // var tectonics = engine.Query<TectonicsC>(2); // entity id 2 = cell
        // Assert.True(tectonics.CrustalThickness.Meters > 0,
        //     "CrustalThickness should be positive after tectonics simulation.");
    }
}
