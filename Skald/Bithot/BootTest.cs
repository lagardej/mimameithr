using Bifrost.Kart;
using Godot;
using Kjarni.Brunnr.Engine;
using Kjarni.Brunnr.Grid;
using Kjarni.Nornir;
using Kjarni.Nornir.Ginnungagap.Seed;
using Skald.Bithot.Eldr.Irradiance;
using Skald.Bithot.Geimr.Geometry;
using Skald.Bithot.Geimr.Orbit;

namespace Skald.Bithot;

public partial class BootTest : Node3D
{
    private const uint Seed = 42;

	// Composition root: one shared store, sub-engines built on top of it.
	private readonly BrunnrEngine _brunnr = new();
	private Bithot _bithot = null!;
	private Nornir _nornir = null!;


    public override void _Ready()
    {
        _nornir = new Nornir(_brunnr.Store);
        _bithot = new Bithot(_brunnr.Store);

		GridProvider.Initialize(GridShape.Spherical, new BifrostKart());
		_nornir.Handle(new SetSeed(Seed));
		NornirBootstrap.CreateSolarSystem(_nornir);
		_bithot.Advance();

		new BodyRenderer(_nornir).AttachTo(this);
		new IrradianceRenderer().AttachStellarLight(this, new Vector3(-1f, -0.6f, -0.4f));
		new OrbitRenderer(_nornir).AttachCamera(this);
	}
}
