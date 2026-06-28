# AGENTS.md — Mimameithr

Solution file: `Mimameithr.slnx`

## Conventions

- British English.
- Oxford spelling (`-ize` not `-ise`).
- Documentation is in English. Discussion may be in French.

## Naming

All project and domain names are Old Norse. See `docs/glossary.adoc` for the full list.

C# identifiers use the original Old Norse form with Unicode characters.
Filenames and directories use ASCII romanization to avoid filesystem issues.

Romanisation rules:
- `ð` (eth) → `th`
- `þ` (thorn) → `th`
- Accented vowels (`ö`, `í`, `á`, `ø`) are dropped or replaced by their unaccented equivalent.

Example: class `Urðr` lives in `Urth.cs`.

## Documentation

### Component properties

XML doc summaries must describe what a property is, not how it will be used. Producers are not consumer-aware.

Computed properties include a `<remarks>` block stating what they are computed from, using `<see cref>` references where possible.

No temporal or lifecycle qualifiers in property docs.

## Architecture

### Kvasir

Simulations are stateless pure functions.

### Nornir

Components hold both parameters and current state as separate properties.

- `[Setting(unit, purpose)]` — externally supplied inputs (world gen, forcings, or runtime).
- `[State(unit, purpose)]` — values computed by a system.

`purpose` is a short description for generated documentation. XML doc summaries may contain more detail for developers.

Systems are stateful: they read parameters and elapsed time, compute current state via Kvasir, and write it back to the component.

### Urth (generation phase)

Urth is the generation phase engine. It lives in `Nornir/Urth/`.
It uses an evaluation graph, not a tick loop. The player is the driver, not time.
It exposes a CQRS interface: one command per system (full settings replacement), queries with no side effects.
It must be fully operable without Godot.

See `docs/architecture.adoc` and `docs/session-handout-urth.adoc` for details.

### Verdandi (active phase)

Verdandi is the active phase engine. It runs a standard ECS tick loop.
The state produced by Urth is the initial ECS state. The epoch is set at phase transition.

## Behaviour

- Instructions are directives, not suggestions.
- The model should act like a machine, be concise and accurate, and don't assume.
- It should only do what is asked, stop and ask if unclear, and provide exactly what was requested.
- The communication style should be short, direct responses, asking first and executing second, with no apologies for brevity.
- The model should not create things that weren't requested, assume user needs, add bonus features or files, build full solutions for vague requests, or use verbose explanations.
- It should ask for clarification when needed, provide exactly what's asked, use minimal clear language, work with what's provided, and stop and ask if something's unclear.
- No praise, no engagement bait, no flattery.
- Assess requests factually.
- Push back when warranted.
- Suggest alternatives without being asked. Guide, don't serve — Sacagawea, not a butler.
- Calibrate enthusiasm: it's an assignment.
