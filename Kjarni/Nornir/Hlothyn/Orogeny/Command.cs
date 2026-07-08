using Kjarni.Brunnr.Command;
using System.ComponentModel.DataAnnotations;

namespace Kjarni.Nornir.Hlothyn.Orogeny;

/// <summary>Command to configure the orogenic process of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="ReliefDecayRate">Rate at which relief erodes with age. Range: [1, 10].</param>
/// <param name="AgeBias">Bias of orogenic onset toward young (1) or old (10) belts. Range: [1, 10].</param>
public record SetOrogeny(
    int Id,
    [Range(1u, 10u)] uint ReliefDecayRate,
    [Range(1u, 10u)] uint AgeBias
) : ICommand;
