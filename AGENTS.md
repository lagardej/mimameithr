# AGENTS.md — Mimameithr

Solution file: `Mimameithr.slnx`

## Conventions

- British English.
- Oxford spelling (`-ize` not `-ise`).
- Documentation is in English. Discussion may be in French.

## Documentation

### Component properties

XML doc summaries must describe what a property is, not how it will be used. Producers are not consumer-aware. Required
for all public classes, properties and methods. Use `<summary>` and `<remarks>` tags. Use `<see cref>` references where
possible.

No temporal or lifecycle qualifiers in property docs.

## Architecture

### Bounded contexts

No hard Kvasir/Nornir split. Group by what changes together, not by statelessness.

Bounded context = domain folder under `Nornir/` (`Eldr`, `Geimr`, `Hlothyn`, `Ginnungagap`, ...). Each folder
mixes generation-phase code (`Command.cs`, `Command.Handler.cs`) and active-phase code (`System.cs`) alongside the
`Component.cs` they operate on. Phase is not a folder split — domain is.

- Cross-cutting computation (physics, geometry, mantle flux, etc.) lives as static functions, called directly by
  whichever system or handler needs it.
- Store derived state on a component only when: (a) expensive to recompute, or (b) multiple unrelated systems read it
  and per-read recompute duplicates real work. Otherwise, compute at point of use — don't cache a value the consumer
  could derive from components it already holds.

### Urðr and Verðandi

`Urðr` (`Nornir/Urth.cs`) and `Verðandi` (`Nornir/Verthandi.cs`) are not bounded contexts. They are internal engine
classes owned by `Nornir` (`Nornir/Nornir.cs`) that manage the two phases. They hold no domain logic — only
orchestration.

- **Urðr** — generation-phase dispatcher. Routes a validated command to the `ICommandHandler<TCommand>` registered for
  it, one handler per domain command, resolved via DI at construction. No tick loop, no evaluation graph of its own —
  the player calls a command, a handler runs, done. Must stay operable without Godot.
- **Verðandi** — active-phase driver. Builds a `SystemRoot` (`VerðandiSystems.Build`) from the domain `System` classes
  and ticks it forward on `Advance`. The ECS state at phase transition is whatever Urðr's handlers left behind; epoch is
  set then.

## Behaviour

Respond terse like smart caveman. All technical substance stay. Only fluff die.

Rules:

- Drop: articles (a/an/the), filler (just/really/basically), pleasantries, hedging
- Fragments OK. Short synonyms. Technical terms exact. Code unchanged.
- Pattern: {thing} {action} {reason}. {next step}.
- Not: "Sure! I'd be happy to help you with that."
- Yes: "Bug in auth middleware. Fix:"

Switch level: /caveman lite|full|ultra|wenyan
Stop: "stop caveman" or "normal mode"

Auto-Clarity: drop caveman for security warnings, irreversible actions, user confused. Resume after.

Boundaries: code/commits/PRs written normal.
Caveman full activated by default.
