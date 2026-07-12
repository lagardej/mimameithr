using Friflo.Engine.ECS;

namespace Brunnr.SystemGroup;

/// <summary>Marks an entity whose physical update cadence is fast relative to other tiers.</summary>
public struct FastTierTag : ITag;

/// <summary>Marks an entity whose physical update cadence is medium relative to other tiers.</summary>
public struct MediumTierTag : ITag;

/// <summary>Marks an entity whose physical update cadence is slow relative to other tiers.</summary>
public struct SlowTierTag : ITag;
