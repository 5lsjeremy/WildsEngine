# Wilds Engine

Wilds is a reusable game simulation engine.

It knows that it is running a game simulation, but it does not know what game is consuming it. Game-specific concepts, behaviors, entities, and rules belong outside the Wilds core.

The engine is currently built as a `netstandard2.1` library so its simulation model can remain independent of any particular host such as Unity.

## Core Model

Wilds currently begins with these foundational interfaces:

- `IBaseObject`
- `IComponent`
- `IState`
- `IContext`
- `ISignal`
- `IStateInstruction`

These establish the common language used by the rest of the engine.

## IBaseObject

`IBaseObject` is the common foundation for Wilds runtime objects.

Every base object has:

- A permanent `Guid` identity.
- A readable, appendable slug.
- A collection of neighboring Wilds objects.
- Access to its component.

The GUID is the canonical identity.

The slug is a human-readable runtime identity derived from the GUID and layer. Additional semantic segments may be appended without changing the object's GUID.

Neighbor relationships are part of the base object because Wilds operates within a hexagonal simulation topology. At this level, a neighbor simply means another adjacent Wilds object. The base object does not need to understand the meaning or direction of that relationship.

## IComponent

`IComponent` extends `IBaseObject` and represents an active component within the simulation.

A component owns:

- `State`
- `Signal`
- `Metadata`

Component metadata uses a `string` to `object` dictionary so consumer systems can attach arbitrary descriptive information without requiring Wilds Core to understand game-specific types.

`BaseObject` provides convenient shortcuts to a component's State, Signal, and Metadata. These properties are owned by the component contract, not `IBaseObject`.

## IState

`IState` is the processing receptor for a Wilds object.

State owns:

- Its current `Context`.
- The ability to process an `IStateInstruction`.

```text
Instruction
    |
    v
  State
    |
    v
 Processing
    |
    v
 Context
```

Context is not passed into `Process()` because the state already owns its processing context.

Processing may update that context as the simulation evolves.

## IContext

`IContext` represents the information available to state processing.

It exposes:

- `Variables`
- contextual `Metadata`

Both collections are exposed as read-only dictionaries.

Mutation occurs through explicit operations:

- `SetVariable`
- `SetMetadata`

This prevents arbitrary consumers from directly modifying the underlying collections while still allowing processing to evolve the context.

Component metadata and Context metadata are deliberately separate:

- **Component Metadata** describes the component.
- **Context Metadata** describes its processing context.
- **Context Variables** represent values available to or produced by processing.

## IStateInstruction

`IStateInstruction` is the native Wilds instruction boundary.

It intentionally contains no QSL-specific types.

External systems may translate their own instructions or intentions into Wilds instructions without requiring Wilds Core to depend upon those systems.

The planned QSL integration follows this boundary:

```text
QSL
 |
 v
WildsEngine.Qsl
 |
 v
IStateInstruction
 |
 v
IState.Process(...)
 |
 v
IContext
```

This keeps Wilds and QSL independently usable while providing a clean handshake between them.

## ISignal

`ISignal` represents the outward signaling side of a Wilds object.

Its detailed contract has intentionally not yet been defined.

The emerging division of responsibility is:

```text
State listens and processes.
Context maintains the current processing environment.
Signal communicates outward.
```

Signal behavior will be expanded when the signaling and event-processing layer is implemented.

## BaseObject

`BaseObject` provides the common implementation for:

- GUID generation and preservation.
- Slug generation and extension.
- Neighbor storage and management.
- Component access.

It also provides convenience shortcuts to:

- `State`
- `Signal`
- `Metadata`

These shortcuts resolve through the object's `Component`; they do not move ownership of those properties into `IBaseObject`.

## BaseComponent

`BaseComponent` derives from `BaseObject` and implements `IComponent`.

A component identifies itself as its component:

`Component => this`

It owns its metadata collection and requires concrete component implementations to provide their State and Signal behavior.

## Design Boundaries

Wilds Core should remain independent of:

- Unity
- QSL
- Cryo Nation
- Any particular game entity or gameplay rule

Wilds may understand concepts intrinsic to game simulation such as components, state, context, signals, neighbors, topology, processing, time, and change.

Game-specific meaning belongs to consumers of the engine.

> **Wilds knows how a game simulation evolves. It does not know what any particular game is about.**

## Current Status

The foundational contracts and base component/object model compile successfully under `netstandard2.1`.

The next implementation work builds outward from this foundation rather than redesigning it.
