# QSL Getting Started

QSL is a stateful, domain-neutral processing runtime over consumer-owned objects and explicitly attached runtime state.

## Availability rule

There are only two ordinary ways a property becomes available to QSL:

1. It is native to a bound consumer object.
2. It is explicitly introduced through runtime state attachment.

Anything else remains unresolved. QSL does not invent vocabulary because an expression references it.

## Golden state-composition example

The certified integration expression is:

`player.Health = player.Health - (action.Amount * (action.SpaceDustAppetite / 100))`

In the certified route:

- `player.Health` begins at `50`.
- `action.Amount` is native consumer state with value `10`.
- `action.SpaceDustAppetite` is explicitly attached runtime state with value `87`.
- QSL evaluates the ordinary expression against those legitimately available properties.
- the exact consumer-owned `Player.Health` becomes `41.3`.

The point is not the arithmetic. The point is that native and runtime-attached vocabulary compose in ordinary QSL while the consumer-owned object remains authoritative.

## QSL and Wilds

QSL is independently usable. In the Wilds product family it also provides the common programmable processing layer. Wilds owns its simulation lifecycle; QSL owns delegated language evaluation.

See [Wilds + QSL](../integration/wilds-qsl.md).
