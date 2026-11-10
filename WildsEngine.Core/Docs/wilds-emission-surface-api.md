# Wilds Gate 8 - Emitter Surface

Wilds-facing emission orchestration contract and acceptance criteria

Status: PROPOSED • Next implementation boundary

Gate 8 exists to expose the already-certified emission route through one Wilds-owned surface. It must make the emitter usable without making the emitter smarter.

## 1. Purpose

Gates 1 through 7 certify the emission machinery from consumer-owned state through topology-aware resolution, propagation, eligibility, and consumer-supplied instruction resolution. Gate 8 does not redefine those mechanics. It coordinates them behind one Wilds-facing operation.

```text
consumer-owned participant
        |
        v
Wilds-facing emission request
        |
        v
certified Gates 1-7
        |
        v
ordinary Wilds instruction output
        |
        v
Wilds lifecycle and history
        |
        v
STOP before instruction execution or consumer-state mutation
```

## 2. Architectural terminology

A consumer-owned object is a participant in Wilds. Source and receiving roles describe positions in a particular interaction; they are not separate Wilds Core entity types.

- A participating consumer-owned object may expose state, emitter behavior, emission rules, topology, and processing behavior.

- The same object may participate as the contextual source of one interaction and on the receiving side of another.

- Gate 8 must not introduce IReceiver, Receiver, or a permanent source/receiver hierarchy.

- Wilds owns generic mechanics. The consumer owns domain objects, vocabulary, state, policies, and semantic meaning.

## 3. Gate 8 non-goals

- No new emission mathematics.

- No new topology mechanics.

- No new propagation semantics.

- No new eligibility semantics.

- No new instruction semantics.

- No receiver abstraction.

- No emission-specific registry or processor architecture.

- No QSL dependency in Wilds Core.

- No implicit instruction execution.

- No implicit consumer-state mutation.

- No causal Neighbor chaining or semantic Timeline routing in this gate.

## 4. Minimal surface

The target surface should be intentionally small: one request contract and one Wilds-facing operation. A new result wrapper should be added only if the existing Moment result cannot carry the Gate 7 output without losing information.

### Conceptual request contract

```text
public interface IEmissionRequest
{
    IEmitter Emitter { get; }

    INeighborRelationship Relationship { get; }

    // Use the existing certified Gate 6 policy contract.
    // Do not create a duplicate eligibility abstraction for Gate 8.

    IEmissionInstructionResolver InstructionResolver { get; }
}
```

The exact Gate 6 policy property is intentionally left to repository alignment. Gate 8 must reuse the certified eligibility contract rather than inventing a second one.

### Wilds-facing operation

```text
ValueTask<IMoment> EmitAsync(
    IEmissionRequest request,
    CancellationToken cancellationToken = default);
```

The spelling and concrete return type should follow the existing WildsEngine public conventions. The architectural requirement is that the consumer invokes one Wilds-owned operation and receives the ordinary Wilds temporal result.

### Result discipline

Prefer the existing Moment and its Result over a new IEmissionResult wrapper. If the Gate 7 output can be carried directly as the authoritative Moment result, Gate 8 should not wrap it merely to label it as emission output.

```text
Gate 8 request
      |
      v
Wilds lifecycle
      |
      v
Moment
      |
      +-- Outcome
      +-- Result = ordinary Gate 7 output
      +-- WorkUnitIds
      `-- Exception when failed
```

## 5. Slice 1 scope

Slice 1 certifies exactly one scalar interaction. Vector support remains certified internally and can be surfaced in a later Gate 8 slice without changing lifecycle ownership.

```text
one emitter
    -> one named scalar emission
    -> one exact Neighbor relationship
    -> one propagated fact
    -> one eligibility decision
    -> zero or one ordinary Wilds instruction
    -> one authoritative Wilds Moment
```

## 6. Acceptance criteria

### Surface

- [ ] A consumer can initiate a complete certified scalar emission interaction through one Wilds-facing operation.

- [ ] The consumer does not manually coordinate Gates 1 through 7.

- [ ] The API reuses existing certified contracts where those contracts already satisfy the requirement.

- [ ] Gate 8 introduces no receiver-specific Core abstraction.

- [ ] Participating objects remain ordinary consumer-owned Wilds participants.

- [ ] Contextual source and receiving roles do not become permanent classifications.

### Gate composition

- [ ] Gate 8 coordinates Gates 1 through 7 without changing their semantics.

- [ ] No downstream stage recalculates or replaces an authoritative upstream value.

- [ ] Resolved magnitude remains Gate 3/4 authority.

- [ ] Propagated magnitude remains Gate 5 authority.

- [ ] Eligibility remains Gate 6 authority.

- [ ] Consumer interpretation remains Gate 7 authority.

- [ ] Gate 8 adds orchestration, not another interpretation layer.

### Identity and provenance

- [ ] Exact emitter identity survives the complete operation.

- [ ] Exact participating consumer-owned object identities survive wherever represented by the certified route.

- [ ] Exact Neighbor relationship identity survives.

- [ ] Arbitrary consumer-owned emission vocabulary survives unchanged.

- [ ] Gate 4 resolved provenance remains reachable.

- [ ] Gate 5 propagation provenance remains reachable.

- [ ] Gate 6 exact eligibility provenance remains reachable.

- [ ] Gate 7 instructions retain the exact eligibility decision that caused their resolution.

### Eligibility and instruction resolution

- [ ] An eligible scalar fact may resolve to one ordinary Wilds instruction.

- [ ] An ineligible scalar fact produces no instruction.

- [ ] Ineligibility is not treated as execution failure.

- [ ] Gate 8 does not execute an instruction merely because Gate 7 resolved it.

- [ ] Gate 8 performs no implicit consumer-state mutation.

### Wilds lifecycle

- [ ] The interaction enters the existing Wilds request lifecycle.

- [ ] Wilds owns the Pulse and required WorkUnit work.

- [ ] Successful orchestration produces one authoritative completed Moment.

- [ ] Failure produces an honest failed Moment rather than false success.

- [ ] Cancellation produces honest cancellation rather than false success.

- [ ] The terminal Moment enters Timeline exactly once.

- [ ] Timeline remains represented through Epoch according to the existing temporal model.

- [ ] No emission-specific temporal infrastructure is introduced.

### Historical stability

- [ ] Changing live topology after a completed interaction does not rewrite recorded facts from that interaction.

- [ ] A later Gate 8 request observes the new topology.

- [ ] The earlier request continues to expose its original relationship, propagation, eligibility, and instruction provenance.

- [ ] Different requests represent distinct historical evaluations rather than mutable views of current topology.

### Consumer neutrality

- [ ] Gate 8 accepts unfamiliar consumer-defined vocabulary.

- [ ] Gate 8 requires no game-specific Core types.

- [ ] A participating consumer-owned object may expose emitter behavior regardless of its role in another interaction.

- [ ] Gate 8 does not assume an emitting participant is permanently a source.

- [ ] Gate 8 does not assume another participant is permanently a receiver.

### QSL boundary

- [ ] QSL is not required for a Core Gate 8 interaction.

- [ ] Gate 8 introduces no dependency from Wilds Core to QSL.

- [ ] Existing Wilds/QSL behavior remains unchanged.

- [ ] Scripted downstream computation remains outside Gate 8.

### Regression gate

- [ ] Existing Gate 1 through 7 focused tests remain green.

- [ ] Gate 7 remains 12/12 unless deliberately extended with additional Gate 7 certification.

- [ ] Full Core Debug regression passes.

- [ ] Full Core Release regression passes.

- [ ] QSL core certification remains 156/156 with zero missing dependencies.

- [ ] Canonical Wilds/QSL consumer-state behavior remains Player.Health: 50 -> 41.3.

## 7. Human certification story

Given a consumer-owned object with an emitter and another consumer-owned object related to it through Wilds topology, when the consumer invokes the Wilds-facing emission operation, Wilds coordinates the certified topology, propagation, eligibility, and instruction-resolution route, returns the resulting ordinary Wilds instruction through a completed Wilds lifecycle, and records the interaction in temporal history without executing the instruction or mutating consumer-owned state.

```text
Machine (consumer-owned)          Mara (consumer-owned)
        |                                  |
        | emitter                          | state / processors /
        |                                  | possible own emitters
        +---------- Neighbor --------------+
                         |
                         v
                      Gate 8
                         |
                coordinates Gates 1-7
                         |
                         v
               ordinary IStateInstruction
                         |
                       STOP
```

## 8. Implementation guardrail

Gate 8 should make the emitter usable, not make the emitter smarter.

If implementing Gate 8 requires substantial new evaluator logic, new emission mathematics, new receiver taxonomy, or new processing infrastructure, the implementation has likely crossed the boundary of the milestone. The preferred implementation is a thin composition layer over already-certified behavior.

## 9. Pre-implementation repository alignment

- [ ] Crawl the current concrete Gates 1 through 7 API before freezing names.

- [ ] Map each request property to an existing certified abstraction.

- [ ] Confirm whether Moment.Result can carry the Gate 7 scalar output directly.

- [ ] Use existing WildsEngine execution conventions for cancellation, failure, Pulse, WorkUnit, Moment, Timeline, and Epoch.

- [ ] Create no abstraction solely because the surface document contains a conceptual slot for it.

## 10. Definition of done

Gate 8 Slice 1 is complete when a consumer can call one Wilds-facing emission operation for one scalar interaction and Wilds owns the complete certified route through temporal history, while Gates 1 through 7 remain authoritative and unchanged.

The surface is successful when it feels boring: one clear entry point over machinery that already works.
