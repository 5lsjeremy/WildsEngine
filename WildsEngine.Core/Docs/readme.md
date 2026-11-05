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

A Pulse owns its required WorkUnits and cannot resolve successfully until the required work resolves honestly. Failed or canceled work cannot silently become a successful Pulse.

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

## Neighbor topology and membership model

Wilds objects can participate in directional Neighbor relationships.

A Neighbor is not limited to an immediately adjacent object. A Neighbor is another member of a shared relational collection or topology, with distance describing proximity inside that relationship.

> **Membership establishes neighborhood. Distance establishes proximity. Adjacency is the special case where distance is 1.**

Relationship identity is defined by:

```text
Neighbor.Id + Membership
```

Distance is relationship state rather than relationship identity and may change without replacing the relationship.

Neighbor relationships are directional:

```text
A -> B
```

does not imply:

```text
B -> A
```

If both relationships exist, they remain independently mutable.

The same Neighbor may also participate through multiple memberships:

```text
A
|
+-- B : local
|
`-- B : remote
```

Those relationships remain independent even though they reference the same Neighbor object.

Wilds provides reusable topology operations over relationship snapshots, including filtering, ordering, grouping, sampling, and scalar computations. Topology operators observe or transform snapshots without owning or mutating the source Neighbor collection.

Distance is intentionally abstract. Depending on the consumer and topology, it may represent hops, cells, traversal depth, semantic separation, or another consumer-defined metric.

Wilds owns the relationship mechanics. The consumer owns the meaning of the relationship and its distance metric.

## Neighbor propagation and eligibility

Wilds can interpret Neighbor distance through a propagation policy without assigning consumer-specific meaning to the propagated value.

The current exponential distance-propagation model is:

```text
Iout = Iin * exp(-k * d)
```

where:

- `Iin` is source intensity;
- `d` is Neighbor relationship distance;
- `k` is configured decay;
- `Iout` is resulting propagated intensity.

Propagation remains separate from topology. Neighbor topology owns relationship distance; propagation policy determines how that distance affects an intensity.

Eligibility is evaluated separately from attenuation. A minimum-intensity eligibility policy determines whether the propagated value is sufficient for a Neighbor to qualify for a subsequent response.

```text
source intensity
       |
       v
Neighbor topology
       |
       v
distance propagation
       |
       v
received intensity
       |
       v
eligibility threshold
       |
       +---- eligible
       |
       `---- not eligible
```

The minimum-intensity boundary is inclusive:

```text
intensity < threshold    -> not eligible
intensity = threshold    -> eligible
intensity > threshold    -> eligible
```

For example:

```text
Source A
intensity = 1.0
decay = 0.5
threshold = 0.25

A -> B : distance 1
         propagated intensity ~= 0.6065
         eligible

A -> C : distance 3
         propagated intensity ~= 0.2231
         not eligible
```

The meaning of intensity remains consumer-owned. Wilds only provides the mechanics for carrying an abstract magnitude through relationship and propagation policies.

The currently established boundary is:

```text
topology
   |
   v
propagation
   |
   v
eligibility
```

The subsequent transition from an eligible Neighbor into causal execution remains proposed behavior.

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

Timeline routing occurs at the history-recording boundary. Processors and processor pools remain responsible for performing work, not organizing temporal history.

This routing policy remains proposed behavior.

### Proposed Neighbor-driven Pulse propagation

Established Neighbor topology, propagation, and eligibility provide the foundation for a causal boundary between independently executing Wilds objects.

The next architecture boundary begins when a Neighbor has received sufficient propagated intensity to qualify for a response.

An eligible Neighbor does not become additional work owned by the originating Pulse. Instead, the intended model allows that Neighbor to receive a causal stimulus and independently originate a new Pulse.

```text
Source object
    |
    v
Source Pulse
    |
    +--> WorkUnits
    |
    +--> topology evaluation
              |
              v
        propagation
              |
              v
         eligibility
              |
              v
        causal stimulus
              |
              v
        Neighbor object
              |
              v
        Independent Pulse
              |
              +--> WorkUnits
              |
              v
            Moment
              |
              v
           Timeline
```

The source Pulse and Neighbor Pulse therefore remain separate execution units.

The intended invariants are:

- a Pulse owns only its own required WorkUnits;
- topology determines which Neighbors are candidates to receive an effect;
- propagation determines the resulting intensity through relationship state;
- eligibility determines whether the propagated effect qualifies for response;
- a Neighbor decides independently whether the received causal stimulus produces work;
- work caused inside the Neighbor belongs to a new Pulse owned by that Neighbor;
- the originating Pulse does not wait for Neighbor work merely because it caused the Neighbor stimulus;
- failure, cancellation, or timeout of a Neighbor Pulse does not silently change the completed outcome of its originating Pulse;
- causal provenance may connect the resulting Pulse or Moment to the event that stimulated it without merging their execution ownership;
- each resulting Moment is recorded into the Timeline appropriate to the object that owns the Pulse.

**Causal provenance connects execution without merging execution ownership.**

Distance propagation and minimum-intensity eligibility are implemented and tested. Causal stimulus delivery and independent Neighbor Pulse creation remain proposed behavior and are the next execution boundary to certify.

## Proposed consumer API

The consumer-facing API remains deliberately thin and should be frozen only after the engine-side relationship, propagation, causal execution, and Timeline-routing semantics are stable.

```csharp
public interface IWildsQslConsumer
{
    void Configure(WildsQslConsumerBuilder builder);
}

public sealed class WildsQslConsumerBuilder
{
    public WildsQslConsumerBuilder AddValue(string name, object? value);
    public WildsQslConsumerBuilder AddDescriptor(string name, object descriptor);
    public WildsQslConsumerBuilder AddFunction(string name, object function);
    public WildsQslConsumerBuilder AddCapability(string name, object capability);
}
```

**This API remains a proposal, not a certified contract.**

## Milestones

### Milestone 1: Neighbor topology and relationships

Implemented and tested:

- membership participates in Neighbor relationship identity;
- distance represents mutable relationship state;
- duplicate relationships are rejected deterministically;
- the same Neighbor can participate through distinct memberships;
- relationships are directional and independently mutable;
- topology mutation flows through state processing;
- topology operators are deterministic and preserve source topology.

### Milestone 2: Neighbor propagation and eligibility

Implemented and tested:

- propagation interprets relationship distance independently of topology;
- exponential distance attenuation is supported;
- zero distance preserves intensity;
- zero decay preserves intensity;
- invalid negative propagation values are rejected;
- minimum-intensity eligibility uses an inclusive threshold;
- below-threshold values are rejected;
- topology, propagation, and eligibility compose without assuming execution ownership.

### Milestone 3: Independent Neighbor Pulse propagation

Define and certify the causal execution boundary.

Target behaviors include:

- an eligible Neighbor may receive a causal stimulus;
- the receiving Neighbor independently determines its response;
- resulting work belongs to a new Pulse owned by the receiving Neighbor;
- originating and receiving Pulses retain independent WorkUnits and lifecycles;
- failure or cancellation of the receiving Pulse cannot retroactively corrupt the originating Pulse;
- causal provenance connects executions without merging execution ownership;
- resulting Moments enter the history belonging to their owning objects;
- propagation termination is explicit and deterministic.

### Milestone 4: Semantic Timeline routing

Use the relationship and causal model as available inputs to temporal organization.

Target behaviors include:

- repeated Moments in the same semantic state continue the active Timeline;
- a semantic-state transition creates a new Timeline;
- returning to an earlier semantic state creates another Timeline instead of reopening historical state;
- membership boundaries can participate in routing policy where appropriate;
- Timeline creation remains Wilds-owned;
- processors and pools remain independent of temporal-routing decisions;
- all Timelines remain preserved by the current Epoch.

### Milestone 5: Consumer API and conformance

Once engine semantics are stable, freeze the consumer-facing integration surface and run the extracted 44-case consumer suite against it without weakening the existing 156/156 core certification baseline.

> **Wilds owns coordination, relationships, propagation mechanics, and history. QSL owns computation. The consumer owns meaning.**
