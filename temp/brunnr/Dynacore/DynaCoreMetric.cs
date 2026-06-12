namespace Brunnr.Dynacore;

/// <summary>A named scalar metric emitted by an <see cref="IDynaCore" /> at the end of a tick.</summary>
public record DynaCoreMetric(string Name, double Value);
