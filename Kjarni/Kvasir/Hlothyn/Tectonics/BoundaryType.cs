namespace Kjarni.Kvasir.Hlothyn.Tectonics;

/// <summary>Tectonic plate boundary type at a cell.</summary>
public enum BoundaryType
{
    /// <summary>No plate boundary. Intraplate cell.</summary>
    None,

    /// <summary>Plates collide. Drives mountain-building and subduction.</summary>
    Convergent,

    /// <summary>Plates separate. Drives rifting and seafloor spreading.</summary>
    Divergent,

    /// <summary>Plates slide laterally past each other. Drives seismicity without significant vertical displacement.</summary>
    Transform,

    /// <summary>Intraplate volcanic cell. Classification deferred to Dynamics concern.</summary>
    HotSpot
}
