# Wilds Engine

Wilds is a reusable game-simulation and processing engine. It coordinates work, state, relationships, and history without knowing the game-specific meaning of the objects it processes.

Wilds Core stays independent of Unity, QSL, Cryo Nation, and consumer-specific entities or rules. Consumer-owned values can live in component metadata and processing context without becoming Wilds Core types.

## Runtime model

Wilds uses a unified work and temporal model:

```text
BaseUnit
  |- Pulse      coordinated request
  |- WorkUnit   required work belonging to a Pulse
  |- Moment     resolved Pulse outcome
  |- Timeline   ordered Moment history
  `- Epoch      collection of Timelines
```

A Pulse owns its required WorkUnits and cannot resolve successfully until the required work resolves honestly. Failed or cancelled work cannot silently become a successful Pulse.

```text
Consumer request
      |
      v
    Pulse
      |
      +----> WorkUnit
      +----> WorkUnit
      `----> WorkUnit
               |
               v
        Pulse resolution
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

The route can retain arbitrary consumer runtime-object references. Wilds coordinates the work and history; it does not need to understand the concrete game type carried by the result.

## Processing infrastructure

Wilds provides the lower-level machinery used by the execution route:

- components, state, context, signals, and metadata;
- state instructions and tasks;
- instruction and task registries;
- component-bound processors;
- bounded processor pools;
- engine composition and lifecycle management.

Processor lifetime remains independent of registered work lifetime. Work can therefore survive the processor lease that happened to execute it.

## QSL integration

QSL integration lives in the companion `WildsEngine.Qsl` project rather than Wilds Core.

```text
QSL source
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

QSL computes. Wilds coordinates the work and preserves the resulting history. Neither core needs to absorb the other's internal implementation model.

## Certification

The current Wilds/QSL core certification contains 156 self-contained cases and is clean:

```text
Total:                 156
Passed:                156
Failed:                  0
Missing Dependencies:    0
Timeline Moments:      156
Epoch Timelines:         1
```

Expected-error cases still produce failed Moments when failure is the correct runtime outcome. Certification success and runtime outcome are intentionally separate concepts.

The former combined corpus has been split into:

- **Core certification:** 156 engine-owned cases.
- **Consumer conformance:** 44 consumer-dependent cases.

## Proposed Neighbor and membership model

Before freezing the consumer-facing API, the next internal architecture milestone is to define and certify Neighbor membership and distance.

A Neighbor is not limited to an immediately adjacent object. A Neighbor is another member of a shared relational collection or topology, with distance describing proximity inside that relationship.

> **Membership establishes neighborhood. Distance establishes proximity. Adjacency is the special case where distance is 1.**

For example, two pieces may belong to the same puzzle without touching. They remain neighbors because they share membership in the puzzle, while their distance expresses how separated they are within that structure.

```text
Shared collection / topology
        |
        +-- Member A
        |
        +-- Member B      distance 1 from A
        |
        +-- Member C      distance 2 from A
        |
        `-- Member D      distance 3 from A
```

Distance is intentionally abstract. Depending on the consumer and topology, it may represent hops, cells, traversal depth, semantic separation, or another certified metric.

Wilds owns the relationship mechanics. The consumer owns the meaning of the relationship and its distance metric.

This model is proposed behavior and is not yet part of the certified core baseline.

## Proposed Timeline routing

Timeline routing follows Neighbor membership because membership may participate in determining whether a resulting Moment continues the active semantic stream.

A Timeline represents one contiguous run of Moments belonging to the same semantic state or processing domain.

```text
Epoch
|
+-- Timeline 1: status.Emotions
|     +-- Moment: nervous
|     +-- Moment: afraid
|     `-- Moment: angry
|
+-- Timeline 2: status.Actions
|     +-- Moment: attack
|     +-- Moment: retreat
|     `-- Moment: reload
|
`-- Timeline 3: status.Emotions
      +-- Moment: relieved
      `-- Moment: calm
```

The proposed routing rule is:

- if an incoming request continues the active semantic state and applicable membership policy, its resulting Moment remains on the active Timeline;
- if the semantic state or applicable membership boundary changes, Wilds creates a new Timeline in the current Epoch;
- returning to a previous semantic state creates a new contiguous Timeline rather than reopening an older Timeline.

This preserves transition history:

```text
status.Emotions
      |
      v
status.Actions
      |
      v
status.Emotions
```

Timeline routing occurs at the history-recording boundary. Processors and processor pools remain responsible for performing work, not organizing temporal history.

The intended responsibility split is:

- the consumer supplies semantic and relational meaning;
- Wilds owns Neighbor and membership mechanics and certified distance behavior;
- Pulses and WorkUnits coordinate execution;
- Moments preserve terminal outcomes;
- Wilds owns Timeline selection and creation;
- Epoch preserves the resulting sequence of Timelines.

This routing policy is proposed behavior and is not yet part of the certified core baseline.

## Proposed consumer API

The consumer-facing API moves to the milestone after Neighbor membership and Timeline routing are defined and certified.

The consumer API is an endpoint into consumer-owned vocabulary, not a place to discover or redefine Wilds engine semantics. Stabilizing the internal relationship and history rules first allows the public boundary to remain small and stable.

The existing QSL consumer model already demonstrates the underlying pattern: create a runtime registry, register evaluators, register host-owned descriptors and functions, create a runtime context, apply host state, and evaluate through `QslEngine`.

The current Wilds-facing proposal remains deliberately thin:

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

**This API remains a proposal, not a certified contract.** Its final shape should be frozen only after the engine-side Neighbor, membership, distance, and Timeline-routing semantics are stable.

A consumer should ultimately be able to express setup like:

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

`Player`, `heal`, and `ApplyForce` remain consumer vocabulary. Wilds only needs a registered runtime capable of resolving them through certified engine mechanics.

## Milestones

### Milestone 1: Neighbor membership and distance

Define and certify the engine-side relationship model before exposing it through a consumer boundary.

Target behaviors include:

- membership establishes Neighbor qualification;
- distance describes proximity within the shared relationship;
- distance 1 represents adjacency by convention;
- non-adjacent members of the same collection can remain Neighbors;
- unrelated objects do not become Neighbors merely because they exist in the runtime;
- duplicate relationship handling is deterministic;
- arbitrary runtime objects can participate without becoming game-specific Wilds Core types.

### Milestone 2: Semantic Timeline routing

Use the certified relationship model as an available input to temporal organization.

Target behaviors include:

- repeated Moments in the same semantic state continue the active Timeline;
- a semantic-state transition creates a new Timeline;
- returning to an earlier semantic state creates another Timeline instead of reopening historical state;
- membership boundaries can participate in routing policy where appropriate;
- Timeline creation remains Wilds-owned;
- processors and pools remain independent of temporal-routing decisions;
- all Timelines remain preserved by the current Epoch.

### Milestone 3: Consumer API and conformance

Once engine semantics are stable, freeze the consumer-facing integration surface and run the extracted 44-case consumer suite against it.

The goal is to prove that consumer-provided objects, functions, capabilities, and data structures can traverse:

```text
Consumer semantics
      |
      v
QSL -> Wilds -> Pulse -> WorkUnit -> Moment
                                  |
                                  v
                         Timeline routing
                                  |
                                  v
                              Timeline
                                  |
                                  v
                                Epoch
```

without weakening the existing 156/156 core certification baseline.

> Wilds owns coordination, relationships, and history. QSL owns computation. The consumer owns meaning.
