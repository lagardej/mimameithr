namespace Brunnr.Domain;

/// <summary>Identifies a domain. Opaque to the scheduler; meaningful to components and the binding layer.</summary>
public readonly record struct DomainId(string Value);
