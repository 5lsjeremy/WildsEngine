# Wilds Engine

Wilds is a reusable game simulation and processing engine.

It knows that it is running a game simulation, but it does not know what game is consuming it. Game-specific concepts, behaviors, entities, and rules belong outside Wilds Core.

Wilds Core targets `netstandard2.1` so that its runtime model remains independent of a specific host such as Unity.

## Architecture

Wilds is built around a small set of runtime concepts:

- `IBaseObject`
- `IComponent`
- `IState`
- `IContext`
- `ISignal`
- `IStateInstruction`
- `ITask`
- registries
- processors
- processor pools
- engine composition

These concepts define how simulation objects are identified, how state is stored and changed, and how work moves through the runtime.

## IBaseObject

`IBaseObject` is the common foundation for Wilds runtime objects.

Every base object has:

- a permanent `Guid` identity;
- a readable, appendable slug;
- neighboring Wilds objects;
- access to its component.

The GUID is the canonical identity.

The slug is a human-readable runtime identity derived from the GUID and layer. Additional semantic segments may be appended without changing the object's GUID.

Neighbor relationships are part of the base object because Wilds operates within a simulation topology. At this level, a neighbor simply represents another adjacent Wilds object. The base object does not need to interpret the semantic meaning of that relationship.

## IComponent

`IComponent` extends `IBaseObject` and represents an active component within the simulation.

A component owns:

- `State`
- `Signal`
- `Metadata`

Component metadata supports arbitrary consumer-owned runtime values.

`BaseObject` also provides convenient access to the owning component's State, Signal, and Metadata. Ownership of those values remains with the component.

## IState

`IState` represents the state-processing surface of a Wilds component.

State owns:

- its current `Context`;
- the ability to process a Wilds-native `IStateInstruction`.

A state does not need to receive its context as a separate processing argument because it already owns that context.

Stateful processing may update the context as the simulation evolves.

## IContext

`IContext` represents the information available to state processing.

It exposes:

- `Variables`
- contextual `Metadata`

Both support arbitrary consumer-owned runtime values.

Mutation occurs through explicit operations such as:

- `SetVariable`
- `SetMetadata`

Component metadata and context metadata are deliberately separate:

- **Component Metadata** describes the component.
- **Context Metadata** describes its processing context.
- **Context Variables** represent values available to or produced by processing.

## ISignal

`ISignal` represents the outward signaling surface associated with a Wilds component.

The broad responsibility split is:

```text
State   -> processes state
Context -> holds the current processing environment
Signal  -> communicates outward
```

The signal model can evolve independently as event and simulation communication behavior expands.

## IStateInstruction

`IStateInstruction` represents Wilds-native processing intent.

Instructions are distinct from executable tasks.

An instruction may describe coordinated work that must be translated or decomposed into one or more executable tasks before its processing is complete.

This separation allows instructions and tasks to have independent lifetimes.

## ITask

`ITask` represents schedulable work inside Wilds.

Tasks can be registered, retrieved, processed, and retired independently from the processors that execute them.

This establishes an important processing distinction:

```text
Instruction
    |
    v
coordinated processing intent
    |
    v
Task / Tasks
    |
    v
executable work
    |
    v
State / Context
```

Task lifetime is independent from processor lifetime.

A processor may be returned to its pool while registered work continues to exist elsewhere in the runtime.

## Registries

Wilds uses registries to maintain runtime work independently from the processors operating on that work.

The current processing architecture includes:

- instruction registries;
- task registries.

Registry storage uses GUID identity and supports:

- registration;
- containment checks;
- retrieval;
- enumeration;
- removal;
- clearing.

This allows instructions and tasks to remain available across processor leases without making processors responsible for owning the lifetime of pending work.

## Processors

Wilds currently separates processing into two major processor types.

### InstructionsProcessor

The instruction processor operates against the instruction registry and can delegate executable work through the task processor pool.

### TaskProcessor

The task processor operates against the task registry and provides the runtime boundary for task work.

Processors are component-bound Wilds runtime objects.

Component ownership is established by `ProcessorBase`, rather than being reimplemented by every concrete processor.

## Processor Pools

Wilds uses bounded object pools for processor reuse.

Processor lifetime is deliberately separate from instruction and task lifetime.

The engine can therefore:

```text
rent processor
    |
    v
perform work
    |
    v
return processor
```

without requiring registered instructions or tasks to share that same lifetime.

The current pool implementation includes bounded concurrency, reuse, cancellation while waiting, reset behavior, and disposal handling.

## Processing Model

The current architecture separates:

```text
request
   |
   v
instruction processing
   |
   v
instruction set
   |
   v
task decomposition
   |
   v
task processing
   |
   v
state / context changes
   |
   v
completed instruction results
   |
   v
result processing / combination
   |
   v
completed request
```

The purpose of this separation is to allow larger requests to be broken into independently schedulable pieces of work while retaining the coordination required to produce a final result.

## Wilds Execution Surface

The intended public execution boundary belongs to Wilds.

Conceptually:

```text
Consumer
    |
    v
Wilds.Execute(...)
    |
    v
command processing
    |
    v
Wilds runtime work
    |
    v
completed Wilds result
    |
    v
Consumer
```

Consumers should not need to directly manage internal processor leases, registries, task pools, or instruction-processing infrastructure.

## QSL Integration

Wilds Core remains independent of QSL.

QSL integration belongs in the companion `WildsEngine.Qsl` layer.

The intended execution direction is:

```text
Consumer
    |
    | command string
    v
Wilds.Execute(...)
    |
    v
WildsEngine.Qsl
    |
    | source string
    v
QSL
    |
    | parse / evaluate / diagnostics
    v
QslProcessingResult
    |
    v
WildsEngine.Qsl
    |
    | translated Wilds work
    v
Wilds Core processing
    |
    v
instructions / tasks / state
    |
    v
completed Wilds result
    |
    v
WildsEngine.Qsl
    |
    v
Consumer
```

QSL therefore acts beneath the Wilds execution surface rather than forming the public Wilds API.

Wilds Core does not need to understand QSL AST types, parser diagnostics, provenance types, or other QSL-specific runtime structures.

Likewise, QSL does not need to understand Wilds processor pools, registries, or task scheduling.

The companion integration layer owns the handshake between the two systems.

## QSL Processing Boundary

QSL accepts source text for parsing:

```csharp
QslParseResult Parse(
    string source)
```

The processed QSL result carries the semantic outcome back to the integration layer.

Successful QSL processing can then be translated into Wilds work.

Failed QSL parsing or evaluation should remain QSL-level processing information rather than being reinterpreted by Wilds Core.

## BaseObject

`BaseObject` provides the common implementation for:

- GUID generation and preservation;
- slug generation and extension;
- neighbor storage and management;
- component access.

It also provides convenience access to:

- `State`
- `Signal`
- `Metadata`

These resolve through the object's component.

## BaseComponent

`BaseComponent` derives from `BaseObject` and implements `IComponent`.

A component resolves its component relationship to itself:

```csharp
Component => this
```

It owns component metadata and requires concrete component implementations to provide their State and Signal behavior.

## Component Ownership

Processing infrastructure follows a consistent component-ownership model.

Base processing abstractions receive their `IComponent` through construction and own that relationship for their derived runtime types.

This prevents concrete processors, tasks, and registries from repeatedly reimplementing the same component relationship.

## Certification

Wilds includes certification coverage for the infrastructure currently implemented.

Certification areas include:

- object pooling;
- engine composition;
- task registry lifecycle;
- high-volume task registry behavior;
- processor acquisition and reuse;
- processor cleanup after failure;
- cancellation behavior;
- current end-to-end instruction/task processing infrastructure.

Integration certification is being extended toward true stateful processing through the complete Wilds execution path.

The larger QSL integration target is to eventually run the existing QSL certification corpus through the Wilds execution surface and prove both QSL processing and downstream Wilds processing together.

## Design Boundaries

Wilds Core remains independent of:

- Unity
- QSL
- Cryo Nation
- any particular game entity
- any particular gameplay rule

Wilds may understand concepts intrinsic to game simulation including:

- components;
- state;
- context;
- signals;
- topology;
- instructions;
- tasks;
- processing;
- time;
- change.

Game-specific meaning belongs to consumers of the engine.

> **Wilds knows how a game simulation evolves. It does not know what any particular game is about.**

## Current Direction

The foundational runtime model is now accompanied by processing infrastructure including task and instruction registries, processor pools, instruction processors, task processors, engine composition, and certification tests.

The next vertical slice is to prove genuine stateful execution through the full processing path.

After that path is certified natively, `WildsEngine.Qsl` can connect the Wilds execution surface to QSL and expand the same path toward full cross-engine certification.
