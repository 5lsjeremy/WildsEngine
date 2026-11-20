# Wilds + QSL Integration

Wilds and QSL have separate responsibilities.

- **Wilds** owns the simulation request lifecycle, coordinated work, temporal outcome, and history.
- **QSL** owns stateful language evaluation over legitimately available consumer state.
- **The consumer** owns objects, vocabulary, rules, and meaning.

## Certified lifecycle

`Consumer request → Pulse → WorkUnit → QSL computation → WorkUnit completion → Moment → Timeline → Epoch`

The QSL result returns through the Wilds lifecycle rather than replacing it.

## Request envelope

`WildsQslRequest` carries QSL source, exact consumer bindings, and attached state packages through the integration route. Wilds Core does not need to understand QSL parser or evaluator internals.

## Golden proof

The canonical expression mutates exact consumer state from `Player.Health = 50` to `41.3` using native `action.Amount = 10` and explicitly attached `action.SpaceDustAppetite = 87`.

## Failure

QSL parse/evaluation failure remains failure through the Wilds route. Failed work must not become a successful Moment. Diagnostics and provenance remain authoritative through the integration path.
