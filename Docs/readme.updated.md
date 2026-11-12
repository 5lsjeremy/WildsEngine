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
- **Wilds/QSL request envelope:** passes envelope execution, exact consumer binding, attached-state composition, and the Space Dust 50 -> 41.3 mutation.
- **Wilds/QSL end-to-end:** proves that Wilds establishes the request lifecycle, delegates QSL-backed work, receives the QSL result, then completes the WorkUnit and resolves the terminal Moment.
- **Emission Gates 1-6:** focused emitter regression passes 67/67 before the final Gate 6 topology-snapshot additions; the final Gate 6 suite passes 13/13. Gates 4-6 certify immutable resolved carriers, arbitrary N-dimensional normalized propagation, received-value eligibility, complete provenance continuity, and historical stability after topology changes.
- **Full Core regression:** 165/165 passes when run serialized. The Wilds/QSL end-to-end lifecycle also passes 20/20 in isolated repetition. One earlier parallel full-suite run produced a single intermittent Wilds/QSL end-to-end failure; this is tracked as test-suite parallel-isolation behavior rather than an emission semantic failure.

Expected-error QSL cases still produce failed Moments when failure is the correct runtime outcome. Certification success and runtime outcome remain intentionally separate concepts.

The older consumer-dependent corpus remains useful as conformance material, but it no longer defines the architectural requirement that QSL itself be the consumer-facing simulation surface. The certified request-envelope path demonstrates that consumer-owned objects and attached vocabulary can be processed through Wilds without moving consumer concepts into Wilds Core.

Known unrelated warnings remain separate from the emission gates: CS8604 in `NeighborTraversal.cs` and existing xUnit analyzer warnings for `ConfigureAwait(false)` in test methods.

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

Direct scalar propagation preserves the authoritative resolved magnitude and does not claim normalization. Vector propagation first groups an ordered set of same-source resolved emissions into an immutable `IResolvedEmissionVector`, normalizes that set into an immutable `INormalizedEmissionVector`, and then produces an immutable `IPropagatedEmissionVector`.

```text
resolved emissions
        |
        v
IResolvedEmissionVector
        |
        v
normalization policy
        |
        v
INormalizedEmissionVector
        |
        v
Neighbor propagation
        |
        v
IPropagatedEmissionVector
```

The vector route supports arbitrary dimension `N`. It preserves exact emitter identity, component identity and ordering, consumer-owned channel names, resolved magnitudes, normalized magnitudes, received magnitudes, exact relationship identity, and the propagation distance used during evaluation. Duplicate channel names and mixed-source vectors are rejected. Caller-owned collections are snapshotted and exposed through read-only wrappers.

Eligibility is evaluated separately from attenuation. Gate 6 evaluates the received propagated `Value`, never the stronger resolved magnitude or the pre-attenuation normalized magnitude. The minimum-intensity boundary remains inclusive:

```text
received value < threshold    -> not eligible
received value = threshold    -> eligible
received value > threshold    -> eligible
```

Scalar eligibility produces an immutable decision over one exact propagated emission. Vector eligibility evaluates every received component independently and preserves both eligible and ineligible channels. `EligibleCount`, `AnyEligible`, and `AllEligible` are descriptive summaries, not execution policy.

```text
IPropagatedEmissionVector
        |
        v
eligibility policy
        |
        v
IEmissionEligibilityVector
        |
        +-- eligible component decisions
        `-- ineligible component decisions retained
```

Eligibility results remain historical facts. If live topology changes after propagation and eligibility, the exact propagated source, evaluated value, decision, original relationship reference, and frozen propagation distance remain unchanged. Eligibility does not mean that a receiver accepted the value, that state changed, or that an effect must occur.

The meaning of every channel and value remains consumer-owned. Wilds provides the mechanics for resolution, grouping, normalization, propagation, and qualification without interpreting consumer vocabulary.

## Emission resolution

Wilds has a certified emitter model for deriving abstract magnitudes from consumer-owned data and the emitter's current relational context. Emission processing now has six certified gates:

```text
consumer-owned value
        |
        v
base emission scalar
        |
        +---- current Neighbor topology
        |              |
        |              v
        |       topology operators
        |              |
        |              v
        |       topology scalar
        |              |
        +--------------+
        |
        v
composition policy
        |
        v
resolved emission
        |
        v
immutable carrier / vector
        |
        v
normalization when vectorized
        |
        v
relationship propagation
        |
        v
received propagated fact
        |
        v
eligibility decision
```

The **base emission scalar** comes from consumer-owned data. The **topology scalar** comes from current Wilds relational context through the existing Neighbor operator system. The **composition policy** determines how those independent scalar inputs are combined. The **resolved emission scalar** is the authoritative output of emission resolution.

The current additive certification fixture proves:

```text
base emission scalar = 1.6
topology scalar      = 2
composition          = additive
resolved emission    = 3.6
```

The value `2` is not itself the emission. It describes topology-derived context. Changing the selected topology can therefore change the resolved emission without changing the consumer-owned base value.

The certified route reuses the existing Neighbor abstractions rather than introducing an emitter-specific topology system. `NeighborPipeline` can compose topology-selection stages before scalar reduction, while `IEmissionComposition` keeps composition mathematics consumer-supplied.

### Certified emission gates

- **Gate 1 - Emitter primitive:** consumer properties and named groups resolve into finite base emission scalars while the emitter remains an ordinary Wilds object with normal Neighbor topology.
- **Gate 2A - Topology scalar:** current topology can be selected through existing Neighbor operators and reduced into a topology-derived scalar.
- **Gate 2B - Composition:** base and topology scalars combine through a supplied composition policy without hard-coding mathematics into the emitter.
- **Gate 3 - Complete resolved emission:** consumer value, current topology, scalar reduction, and composition resolve through one source-safe internal operation.
- **Gate 4 - Resolved emission carrier:** exact emitter identity, arbitrary consumer vocabulary, finite resolved magnitude, and snapshot stability survive in an immutable carrier.
- **Gate 5 - Propagation and vectors:** direct scalar propagation and arbitrary N-dimensional named-vector propagation preserve exact source/component/relationship provenance. Vector propagation requires normalization, keeps extreme resolved magnitudes, freezes the evaluated distance, and returns immutable carriers.
- **Gate 6 - Eligibility:** scalar and vector eligibility evaluate the exact received propagated value. Eligible and ineligible facts remain present, ordering and dimensions remain authoritative, and decisions survive later topology changes without state mutation or consumer interpretation.

Gate 6 certification specifically proves:

- received value is the authoritative eligibility input;
- exact-threshold inclusion is preserved;
- arbitrary `N` survives through eligibility;
- zero, partial, and all-component eligibility are represented honestly;
- vector dimensions and component ordering cannot be rewritten;
- eligibility vectors snapshot caller-owned decision collections;
- complete Gate 5 provenance survives into Gate 6;
- scalar and vector decisions remain stable after live topology changes.

Emission certification reports are written under:

```text
Certification/
`-- Reports/
    `-- Emission/
        |-- emitter-certification-<timestamp>.log
        `-- emitter-certification-latest.log
```

The next boundary is Gate 7: translate an eligible propagated fact into an immutable generic state operation against the exact consumer-owned target. Gate 7 must preserve target identity, arbitrary state vocabulary, value provenance, and explicit add/update/remove semantics without performing consumer interpretation inside Wilds Core.

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

Membership participates in relationship identity; distance is mutable relationship state; duplicate relationships are rejected; relationships are directional; the same Neighbor may participate through multiple memberships; and topology operators are deterministic and source-safe.

### Milestone 2: Neighbor propagation and eligibility mechanics

**CERTIFIED**

Exponential attenuation, zero-distance and zero-decay behavior, inclusive minimum-intensity eligibility, invalid-input rejection, and topology-to-propagation-to-eligibility composition are certified.

### Milestone 3: Emission resolution

**CERTIFIED THROUGH GATE 3**

Consumer-owned values, current Neighbor topology, topology scalar reduction, and supplied composition resolve into one authoritative source-safe emission scalar.

### Milestone 4: Resolved emission carrier

**CERTIFIED**

The immutable carrier preserves exact source identity, arbitrary consumer emission identity, finite resolved magnitude, and snapshot stability without owning propagation behavior.

### Milestone 5: Resolved emission propagation and vector carriers

**CERTIFIED**

Direct scalar propagation and arbitrary N-dimensional vector propagation preserve resolved, normalized, and received values. Vector grouping, normalization, dimension/order invariants, exact relationship identity, frozen propagation distance, and collection immutability are certified.

### Milestone 6: Receiver eligibility

**CERTIFIED**

Eligibility evaluates received propagated values, retains eligible and ineligible facts, supports arbitrary N-dimensional vectors, preserves exact provenance, exposes descriptive count/any/all summaries, and remains historically stable after live topology changes. No receiver mutation or consumer interpretation occurs.

### Milestone 7: Receiver state boundary

**NEXT**

Translate an eligible propagated fact into a generic immutable state operation against the exact consumer-owned target.

Target behaviors:

- exact receiver/target identity survives;
- arbitrary state vocabulary survives;
- source eligibility and propagated provenance remain reachable;
- state add, update, set, or remove semantics are explicit rather than inferred;
- operation creation does not immediately mutate consumer state;
- Wilds remains independent of consumer-specific object types;
- QSL is not required for the Core state-operation boundary.

### Milestone 8: Wilds-facing end-to-end emission vertical slice

**PLANNED**

Certify one authoritative Wilds route from consumer-owned values and topology through resolution, immutable carriers, vector normalization, propagation, eligibility, and resolved state output. Topology may change between evaluations, unfamiliar consumer vocabulary remains valid, and Wilds remains the public coordination surface.

### Milestone 9: QSL downstream integration and conformance

**PLANNED**

Where scripting is requested, hand already-valid Wilds state operations through the existing internal QSL integration boundary without making QSL the simulation authority. Preserve the QSL core baseline, request-envelope certification, provenance continuity, runtime isolation, exact consumer-object identity, and arbitrary runtime-attached vocabulary.

Future causal Neighbor execution, semantic Timeline routing, and final convenience API freezing remain follow-on milestones after the receiver state boundary and full emission vertical slice are stable.

**Wilds is the exposed surface and owns coordination, request lifecycle, relationships, emission resolution, propagation, eligibility, and history. QSL owns delegated computation. The consumer owns meaning.**
