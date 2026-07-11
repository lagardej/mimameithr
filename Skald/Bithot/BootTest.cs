using Bifrost.Kart;
using Godot;
using Kjarni.Brunnr.Engine;
using Kjarni.Brunnr.Grid;
using Kjarni.Nornir;
using Kjarni.Nornir.Ginnungagap.Seed;

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
        _bithot = new Bithot(_brunnr.Store, _nornir);

		GridProvider.Initialize(GridShape.Spherical, new BifrostKart());
		_nornir.Handle(new SetSeed(Seed));
		NornirBootstrap.CreateSolarSystem(_nornir);
		_bithot.Advance();

		_bithot.AttachTo(this);
	}

    public override void _Process(double delta)
    {
        _nornir.Advance((float)delta);
        _bithot.Advance();
    }
}
