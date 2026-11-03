# Wilds Engine - Architecture Introduction

Wilds is a reusable simulation-processing engine designed around a strict boundary: it knows how simulation work evolves, but it does not know what a particular game is about.

Consumer-specific entities, functions, data structures, and gameplay rules remain outside Wilds Core. The engine provides identity, state, processing, work coordination, and temporal history while allowing arbitrary consumer-owned runtime values to cross those boundaries.

## Foundational runtime

The foundational object model includes:

- `IBaseObject` for stable runtime identity and topology;
- `IComponent` for component-owned state, signal, and metadata;
- `IState` for state processing;
- `IContext` for variables and contextual metadata;
- `ISignal` for outward communication;
- instructions and tasks for processing intent and schedulable work;
- registries for work lifetime independent of processors;
- processors and bounded pools for reusable execution infrastructure.

Component and context metadata can hold arbitrary consumer-owned values. The GUID remains canonical object identity; readable slugs can provide runtime context without replacing that identity.

## Unified BaseUnit model

Wilds now coordinates work and history through one unit family:

```text
                 BaseUnit
                    |
       +------------+------------+-------------+------------+
       |            |            |             |            |
     Pulse       WorkUnit      Moment       Timeline       Epoch
   request         work        result        history       history
```

These units are related stages and scales of the same processing model.

### Pulse

A `Pulse` represents the coordinated request. It owns the WorkUnits that must resolve before the request can resolve.

### WorkUnit

A `WorkUnit` represents individually tracked required work. WorkUnits can complete, fail, or be cancelled independently while remaining owned by the Pulse that requires them.

A Pulse cannot resolve successfully while required WorkUnits remain pending, failed, or cancelled.

### Moment

A `Moment` represents a completed Pulse outcome. Success and failure are both historical truths. A failed computation is retained as a failed Moment rather than rewritten into success.

### Timeline and Epoch

A `Timeline` collects Moments. An `Epoch` collects Timelines.

The complete lifecycle is:

```text
+------------------- ACTIVE WORK -------------------+
|                                                   |
|  Consumer                                         |
|     |                                             |
|     v                                             |
|   Pulse                                           |
|     |                                             |
|     +---------> WorkUnit ------------------+      |
|     +---------> WorkUnit ------------------+      |
|     `---------> WorkUnit ------------------+      |
|                                             |      |
|                              all required work     |
|                                     resolves      |
+-------------------------------------|-------------+
                                      v
+------------------ HISTORY ------------------------+
|                                                   |
|                  Moment                           |
|                     |                             |
|                     v                             |
|                  Timeline                         |
|                     |                             |
|                     v                             |
|                   Epoch                           |
|                                                   |
+---------------------------------------------------+
```

Consumer runtime-object references can survive this route. The engine does not need to convert an arbitrary consumer object into a Wilds-specific gameplay type simply to coordinate or retain it.

## Processing infrastructure beneath the units

Instructions and tasks remain lower-level processing infrastructure.

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
processors + registries
    |
    v
state / context
```

Registries own pending-work lifetime independently from processor leases. Processor pools can therefore rent, execute, reset, and return processors without implicitly destroying the work being coordinated.

## QSL companion integration

Wilds Core remains independent of QSL. Integration belongs in `WildsEngine.Qsl`.

The current certified execution route is:

```text
Consumer / QSL source
          |
          v
    WildsQslExecutor
          |
          v
      WildsEngine
          |
          v
        Pulse
          |
          v
       WorkUnit
          |
          v
    QSL computation
          |
          v
        Moment
          |
          v
       Timeline
          |
          v
         Epoch
```

This establishes a clear responsibility split:

- **Consumer:** owns game-specific meaning and runtime vocabulary.
- **QSL:** parses and evaluates computation.
- **Wilds:** coordinates the work and preserves its historical outcome.
- **WildsEngine.Qsl:** owns the handshake between Wilds and QSL.

Wilds Core therefore does not need QSL AST, parser, or evaluator internals. QSL does not need Wilds processor pools, temporal units, or scheduling internals.

## Certification boundary

Core certification is now separate from consumer conformance.

The current Wilds/QSL core suite is certified at **156/156**, with zero certification failures and zero missing dependencies. It retains 156 Moments through one Timeline held by one Epoch.

Expected-error tests demonstrate an important semantic distinction: a test can pass because the runtime correctly produced a failed Moment. Runtime outcome describes what happened; certification status describes whether that behavior matched the contract.

The original combined corpus also exposed 44 cases that depend upon consumer-provided runtime vocabulary. Those cases have been extracted into the consumer-conformance suite rather than being treated as core failures.

## Consumer-facing API proposal

The next architecture slice is an explicit consumer composition boundary.

QSL's existing consumer examples demonstrate the underlying host pattern: a consumer creates a `QslRuntimeRegistry`, registers standard evaluators, registers a host-owned descriptor and function, creates `QslRuntimeContext`, applies host state, creates `QslEngine`, and evaluates QSL. Host concepts such as `Player`, `health`, `mana`, and `heal` remain consumer concepts rather than QSL primitives.

Wilds should preserve that ownership boundary while hiding QSL-specific composition details behind `WildsEngine.Qsl`.

### Proposed surface

```csharp
public interface IWildsQslConsumer
{
    void Configure(WildsQslConsumerBuilder builder);
}

public sealed class WildsQslConsumerBuilder
{
    public WildsQslConsumerBuilder AddValue(
        string name,
        object? value);

    public WildsQslConsumerBuilder AddDescriptor(
        string name,
        object descriptor);

    public WildsQslConsumerBuilder AddFunction(
        string name,
        object function);

    public WildsQslConsumerBuilder AddCapability(
        string name,
        object capability);
}
```

This is an architectural proposal rather than a frozen API. Before it becomes public contract, each method should be mapped onto the current QSL registry/context APIs and certified through the consumer suite.

The intended consumer experience is:

```csharp
public sealed class GameRuntime : IWildsQslConsumer
{
    public void Configure(
        WildsQslConsumerBuilder builder)
    {
        builder
            .AddDescriptor("Player", playerDescriptor)
            .AddFunction("heal", healFunction)
            .AddCapability("ApplyForce", physicsCapability);
    }
}
```

Nothing in Wilds Core needs to know what `Player`, `heal`, or `ApplyForce` means.

## Consumer conformance

The extracted consumer suite currently gives this work a concrete proving ground: 44 cases whose dependencies were deliberately separated from the certified core corpus.

The conformance harness should ultimately let an external consumer provide:

- objects and descriptors;
- functions;
- capabilities;
- arbitrary runtime data structures;
- expected test results.

Then the same execution machinery can validate them:

```text
Consumer registration
        |
        v
Consumer test statement
        |
        v
       QSL
        |
        v
      Wilds
        |
        v
      Pulse
        |
        v
    WorkUnit
        |
        v
     Moment
        |
        v
    Timeline
        |
        v
      Epoch
        |
        v
Conformance report
```

The next milestone is to implement this consumer-facing boundary using the actual QSL registration contracts, run all 44 extracted cases through it, and keep the existing 156/156 core certification unchanged.

## Architectural rule

**Wilds owns coordination and history. QSL owns computation. Consumers own meaning.**
