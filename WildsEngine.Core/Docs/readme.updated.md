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
  |- Timeline   Moment history ordered by successful commit
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

For concurrent executions, Timeline order is the serialized order in which terminal Moments are successfully committed. Wilds does not guarantee request-submission order, Pulse-creation order, operation-completion order, or pre-commit finalization order across concurrent requests.

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

Wilds is the exposed consumer surface. QSL integration lives in the companion WildsEngine.Qsl project rather than Wilds Core, and QSL performs delegated computation inside a Wilds-owned request lifecycle.

```text
Consumer
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
Wilds/QSL integration
   |
   +--> consumer bindings
   +--> attached state packages
   `--> QSL source
            |
            v
       QSL computation
            |
            v
         result
            |
            v
   WorkUnit completion
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

Wilds creates the Pulse and WorkUnit before QSL evaluates the request. QSL's result returns to Wilds, which completes the WorkUnit and resolves the Pulse into its terminal Moment.

WildsQslRequest is the certified request envelope for QSL-backed work. It carries QSL source, exact consumer bindings, and attached state packages through Wilds as opaque request data. WildsQslExecutor bridges the Wilds lifecycle to QslEngine; it is integration machinery, not the conceptual consumer front door.

QSL processes the valid source and runtime context it is given. Propagation is independent: a QSL-backed request may be processed locally without requiring its outcome to propagate.

The canonical integration proof is:

```text
player.Health = player.Health - (action.Amount * (action.SpaceDustAppetite / 100))
```

With `player.Health = 50`, native `action.Amount = 10`, and attached `action.SpaceDustAppetite = 87`, the certified route mutates the exact consumer-owned Player to `41.3`, then records the result through Wilds history.

## Certification

The certification surfaces are complementary:

- **QSL core through Wilds:** 156/156 cases pass with zero failures and zero missing dependencies.
- **Wilds/QSL request envelope:** passes envelope execution, exact consumer binding, attached-state composition, and the Space Dust `50 -> 41.3` mutation.
- **Wilds/QSL end-to-end:** proves that Wilds establishes the request lifecycle, delegates QSL-backed work, receives the QSL result, then completes the WorkUnit and resolves the terminal Moment.
- **Emission Gate 3:** certifies the complete internal emission-resolution route from consumer value through topology-derived context and supplied composition to the authoritative resolved emission scalar.

Expected-error QSL cases still produce failed Moments when failure is the correct runtime outcome. Certification success and runtime outcome remain intentionally separate concepts.

The older consumer-dependent corpus remains useful as conformance material, but it no longer defines the architectural requirement that QSL itself be the consumer-facing simulation surface. The certified request-envelope path demonstrates that consumer-owned objects and attached vocabulary can be processed through Wilds without moving consumer concepts into Wilds Core.

One unrelated Core warning currently remains: `CS8604` in `NeighborTraversal.cs`.

## Neighbor topology and membership model

Wilds objects can participate in directional Neighbor relationships.

A Neighbor is not limited to an immediately adjacent object. A Neighbor is another member of a shared relational collection or topology, with distance describing proximity inside that relationship.

**Membership establishes neighborhood. Distance establishes proximity. Adjacency is the special case where distance is 1.**

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

The meaning of intensity remains consumer-owned. Wilds only provides the mechanics for carrying an abstract magnitude through relationship and propagation policies.

## Emission resolution

Wilds now has a certified emitter model for deriving an abstract magnitude from consumer-owned data and the emitter's current relational context.

Emission resolution intentionally separates four concepts:

```text
consumer-owned value
        |
        v
base emission scalar

current Neighbor topology
        |
        v
topology operators
        |
        v
topology scalar

base scalar + topology scalar
        |
        v
composition policy
        |
        v
resolved emission scalar
```

The **base emission scalar** comes from consumer-owned data.

The **topology scalar** comes from current Wilds relational context through the existing Neighbor operator system.

The **composition policy** determines how those independent scalar inputs are combined.

The **resolved emission scalar** is the authoritative output of emission resolution.

For example, the current additive certification fixture proves:

```text
base emission scalar = 1.6
topology scalar      = 2
composition          = additive

resolved emission    = 3.6
```

The value `2` is not itself the emission. It describes topology-derived context. Changing the selected topology can therefore change the resolved emission without changing the consumer-owned base value.

The certified emission route reuses the existing Neighbor abstractions rather than introducing an emitter-specific topology system:

```text
Emitter.Neighbors
       |
       v
INeighborSequenceOperator
       |
       v
relationship snapshot
       |
       v
INeighborScalarOperator<TResult>
       |
       v
topology scalar
```

`NeighborPipeline` can compose topology-selection stages before scalar reduction.

Composition is independently configurable through `IEmissionComposition`. `EmissionCompositionBase` supplies common finite-value invariants while a concrete composition implementation owns its mathematical semantics.

Wilds therefore does not assume that composition must mean addition, multiplication, weighting, normalization, or any other specific operation.

### Certified emission gates

- **Gate 1 - Emitter primitive:** consumer properties and named groups resolve into finite base emission scalars while the emitter remains an ordinary Wilds object with normal Neighbor topology.
- **Gate 2A - Topology scalar:** current topology can be selected through existing Neighbor operators and reduced into a topology-derived scalar.
- **Gate 2B - Composition:** base and topology scalars can be combined through a supplied composition policy without hard-coding the mathematics into the emitter.
- **Gate 3 - Complete resolved emission:** consumer value, current topology, topology scalar reduction, and composition resolve through one complete internal emission operation.

Gate 3 certification proves:

- unrelated memberships remain isolated;
- membership changes immediately affect the next resolved emission;
- missing consumer properties stop resolution cleanly;
- existing Neighbor pipelines can define the topology used by emission;
- emission resolution does not mutate source topology;
- the complete certified example resolves base `1.6` and topology scalar `2` to `3.6` under the additive certification policy.

Emission certification reports are written under:

```text
Certification/
`-- Reports/
    `-- Emission/
        |-- emitter-certification-<timestamp>.log
        `-- emitter-certification-latest.log
```

The next boundary is an immutable resolved-emission carrier. Propagation should consume that resolved result rather than recomputing the base or topology scalar.

## Proposed Timeline routing

Timeline routing follows Neighbor membership because membership may participate in determining whether a resulting Moment continues the active semantic stream.

A Timeline represents one contiguous run of Moments belonging to the same semantic state or processing domain.

This routing policy remains proposed behavior.

### Proposed Neighbor-driven Pulse propagation

Established Neighbor topology, propagation, eligibility, and emission resolution provide the foundation for a causal boundary between independently executing Wilds objects.

An eligible Neighbor does not become additional work owned by the originating Pulse. The intended model allows that Neighbor to receive a causal stimulus and independently originate a new Pulse.

**Causal provenance connects execution without merging execution ownership.**

Causal stimulus delivery and independent Neighbor Pulse creation remain proposed behavior and will be certified only after the resolved-emission handoff and receiver state boundary are stable.

## Consumer-facing API direction

The exposed boundary is clear even though the final convenience API can continue to evolve:

- **Wilds is the front door.**
- consumers provide their objects, vocabulary, state, and rules to Wilds;
- Wilds owns Pulse/WorkUnit creation and lifecycle resolution;
- Wilds owns relationships and emission-resolution mechanics;
- QSL-backed computation is delegated behind the Wilds boundary;
- WildsQslExecutor is integration machinery and should not be treated as the intended consumer construction/invocation surface;
- WildsQslRequest is the certified integration envelope carrying source, consumer bindings, and attached state;
- propagation is a Wilds policy and does not gate whether QSL can process valid delegated work;
- QSL's result returns to Wilds, where the terminal Moment and history are resolved.

Consumer vocabulary remains consumer-owned. Wilds coordinates it; QSL computes against the runtime context it is given.

## Milestones

### Milestone 1: Neighbor topology and relationships

**CERTIFIED**

Implemented and tested:

- membership participates in Neighbor relationship identity;
- distance represents mutable relationship state;
- duplicate relationships are rejected deterministically;
- the same Neighbor can participate through distinct memberships;
- relationships are directional and independently mutable;
- topology mutation flows through state processing;
- topology operators are deterministic and preserve source topology.

### Milestone 2: Neighbor propagation and eligibility

**CERTIFIED**

Implemented and tested:

- propagation interprets relationship distance independently of topology;
- exponential distance attenuation is supported;
- zero distance preserves intensity;
- zero decay preserves intensity;
- invalid negative propagation values are rejected;
- minimum-intensity eligibility uses an inclusive threshold;
- below-threshold values are rejected;
- topology, propagation, and eligibility compose without assuming execution ownership.

### Milestone 3: Emission resolution

**CERTIFIED THROUGH GATE 3**

Implemented and tested:

- consumer-owned values resolve into base emission scalars;
- arbitrary Neighbor topology can produce an independent topology scalar;
- existing Neighbor operators and pipelines are reused for topology selection;
- topology changes immediately affect subsequent emission resolution;
- base and topology scalars remain semantically distinct;
- composition mathematics are supplied through `IEmissionComposition`;
- `EmissionCompositionBase` owns shared finite-value invariants;
- the resolved emission scalar is the authoritative output of emission resolution;
- complete resolution preserves source topology.

### Milestone 4: Resolved emission carrier

**NEXT**

Create and certify the immutable value that carries a resolved emission forward.

Target behaviors:

- exact source/emitter identity is preserved;
- arbitrary consumer emission identity is preserved;
- resolved scalar is finite;
- carrier state is immutable;
- the carrier does not own propagation behavior;
- creating the carrier does not change Gate 3 resolution semantics.

### Milestone 5: Resolved emission propagation

**PLANNED**

Connect the resolved-emission carrier to the already-certified Neighbor propagation and eligibility machinery.

Target behaviors:

- propagation consumes the resolved emission rather than recalculating it;
- relationship distance remains topology state;
- propagation policy remains independent of topology;
- eligibility remains independent of attenuation;
- source emission data remains unchanged by propagation;
- no consumer-specific vocabulary enters Wilds Core.

### Milestone 6: Receiver state boundary

**PLANNED**

Define the transition from an eligible propagated result into a generic consumer-owned state operation.

Target behaviors:

- exact receiver identity survives;
- arbitrary state vocabulary survives;
- resolved values survive without reinterpretation;
- state add/update/remove semantics are explicit;
- Wilds remains independent of consumer-specific object types;
- QSL is not required for the Core state boundary.

### Milestone 7: Independent Neighbor causal execution

**PLANNED**

Certify the causal execution boundary after emission, propagation, eligibility, and receiver state delivery are stable.

Target behaviors include:

- an eligible Neighbor may receive a causal stimulus;
- the receiving Neighbor independently determines its response;
- resulting work belongs to a new Pulse owned by the receiving Neighbor;
- originating and receiving Pulses retain independent WorkUnits and lifecycles;
- failure or cancellation of the receiving Pulse cannot retroactively corrupt the originating Pulse;
- causal provenance connects executions without merging execution ownership;
- resulting Moments enter the history belonging to their owning objects;
- propagation termination is explicit and deterministic.

### Milestone 8: Semantic Timeline routing

**PLANNED**

Use the established relationship and causal model as inputs to temporal organization.

Target behaviors include:

- repeated Moments in the same semantic state continue the active Timeline;
- a semantic-state transition creates a new Timeline;
- returning to an earlier semantic state creates another Timeline instead of reopening historical state;
- membership boundaries can participate in routing policy where appropriate;
- Timeline creation remains Wilds-owned;
- processors and pools remain independent of temporal-routing decisions;
- all Timelines remain preserved by the current Epoch.

### Milestone 9: Wilds-facing API and conformance

**PLANNED**

Freeze the thin Wilds-facing convenience surface around the certified architecture without exposing QSL integration machinery as the product front door.

Preserve:

- the existing QSL core certification baseline;
- Wilds/QSL request-envelope certification;
- Wilds/QSL provenance continuity;
- Wilds/QSL end-to-end lifecycle behavior;
- runtime isolation;
- exact consumer-object identity;
- arbitrary runtime-attached vocabulary.

**Wilds is the exposed surface and owns coordination, request lifecycle, relationships, emission resolution, propagation mechanics, and history. QSL owns delegated computation. The consumer owns meaning.**
