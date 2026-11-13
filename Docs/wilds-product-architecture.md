# Wilds Product Architecture

> **Independent products. Shared foundations. Capability-based composition.**

## Status

This document captures the intended product and architectural boundaries for QSL, Wilds.Discoveries, Wilds Core, Entities, and future add-ons. It is a product-direction and certification charter. Proposed future behavior remains subject to implementation and certification.

## 1. Product family

The Wilds technology family is not intended to become one inseparable engine package.

Each major product should solve a coherent problem independently, while becoming more capable when composed with the others.

```text
QSL
  semantic and execution foundation

Wilds.Discoveries
  evidence, vectors, resolution, provenance,
  bias/resilience, and evolving state

Wilds Core
  simulation, processing, relationships,
  time, capabilities, and world interaction

Entities
  agent-facing composition layer

Add-ons
  capability-based extensions that can target
  any valid combination of the product family
```

The governing principle is:

> **Integration should create additional value without making artificial coupling a requirement for individual product value.**

## 2. QSL

QSL is the common semantic and execution foundation.

It remains independently purchasable and independently useful.

Its role is not to own Discoveries, Wilds simulation, or agent cognition. Its role is to provide executable semantic composition over state that is legitimately available to it.

The current vocabulary rule remains authoritative:

- native properties on a bound consumer object are immediately available;
- runtime properties become available only after explicit state attachment;
- properties that are neither native nor attached remain unresolved;
- processing against unavailable properties must fail rather than invent state.

### Commercial position

QSL can be purchased alone.

Products higher in the family may include QSL because their practical expressiveness benefits substantially from the common language/runtime foundation.

## 3. Wilds.Discoveries

Wilds.Discoveries is intended to become a separately certifiable and potentially standalone product.

Its domain is epistemic state and change, including concepts such as:

- Discovery;
- evidence;
- provenance;
- strength;
- truth and believability;
- partial resolution;
- vector state;
- accumulated movement;
- resolution;
- bias and resilience;
- resultant state;
- trajectory.

Wilds.Discoveries should not require Wilds Core merely to perform its own core work.

### Independence rule

> **Wilds may consume Discoveries, but Discoveries must not depend on Wilds-specific simulation meaning to justify its existence.**

Standalone certification should prove Discovery behavior without requiring Wilds-specific concepts such as NPCs, world locations, fishing, or game-domain semantics.

### Commercial position

A Discoveries product offering can include:

```text
Wilds.Discoveries
+ QSL
```

This gives a customer the discovery/evidence/vector system together with the semantic execution foundation, without requiring purchase of Wilds Core.

## 4. Wilds Core

Wilds Core owns the general simulation substrate.

Its concerns include the processing and world machinery already being developed and certified, including areas such as:

- consumer-owned state and identity;
- processing;
- Pulse and WorkUnit execution;
- Moment, Timeline, and Epoch temporal history;
- relationships and neighbors;
- emissions and propagation;
- capabilities and world interaction;
- generic simulation behavior.

Wilds Core should not become the home of agent-specific cognition or Discovery-specific epistemology merely because those systems integrate with it.

### Commercial position

A Wilds Core offering can include:

```text
Wilds Core
+ QSL
```

A customer therefore receives the simulation substrate with its semantic execution foundation without being required to purchase Discoveries.

## 5. Entities are the Wilds agent layer

Entities already exist as their own project. They are therefore the natural home for the Wilds agent-facing model rather than requiring creation of another future "agent integration" product.

The important distinction is:

> **An Entity is capable of being an agent. It is not synonymous with one specific AI implementation.**

An Entity can carry the state and boundaries needed for agent behavior while the actual cognitive strategy remains replaceable.

Conceptually:

```text
                 Entity
                    |
             Agent Strategy
                    |
        +-----------+-----------+
        |           |           |
 Discoveries    Wilds Core     QSL
        |           |           |
     learns       exists      operates
        |         in world    on exposed
        |           |          state
        +-----------+-----------+
```

Entities is where the surrounding capabilities become a particular participant in the world.

Wilds Core does not need to know that Jerry thinks.

Wilds.Discoveries does not need to know that Jerry inhabits a simulation.

QSL does not need to know that Jerry is an agent.

Entities provides the composition surface where those capabilities can become useful to Jerry.

## 6. Agent strategies belong around Entities

Once Entities and the relevant supporting systems are certified, established and future agent strategies should be evaluated as strategies that operate an Entity.

They should not become mandatory semantics inside Wilds Core.

Candidate strategies include:

```text
ReAct-style reason / act / observe
Plan-and-Execute
Reflection / feedback-driven adaptation
Orchestrator / Worker
consumer-defined strategies
LLM-backed strategies
```

### ReAct-style strategy

```text
Entity receives legitimate state / Discovery
        |
strategy reasons
        |
strategy proposes or selects exposed action
        |
world interaction occurs through Wilds
        |
outcome becomes legitimate evidence / Discovery
        |
Entity observes outcome
        |
repeat
```

### Plan-and-Execute

```text
Goal
  |
Plan
  |
Pulse
  |
WorkUnits
  |
Moments / outcomes
  |
Discoveries / evidence
  |
continue, revise, or abandon
```

The planner may propose intent. It does not get to declare simulation success. Wilds remains authoritative about what actually happened.

### Reflection

A reflective strategy can consume legitimate outcomes and allow those outcomes to influence future decisions.

Reflection must not rewrite historical evidence merely because a later interpretation changes.

### Orchestrator / Worker

Multiple Entities can eventually coordinate while preserving individual:

- identity;
- capabilities;
- discoveries;
- evidence;
- relationships;
- bias;
- legitimately available vocabulary.

Coordination must not imply omniscience.

## 7. Discoveries gives Entities a way to grow

The Discovery-driven model provides a path for an Entity to enter a consumer world without already knowing what that world means.

An Entity may begin with profile seeds representing initial conditions.

The world then supplies Discoveries.

Those Discoveries can accumulate vector movement, remain partially resolved, eventually resolve, and influence subsequent state.

The intended progression is:

```text
Initial Entity Profile
        |
Seed Discoveries
        |
Current Bias / Resilience
        |
Incoming World Discovery
        |
Vector Interaction
        |
Accumulated Movement
        |
Resolution
        |
Result / Action Pressure
        |
Experience
        |
Changed Entity State
        |
Next Discovery
```

The agent therefore does not need to arrive knowing Miller's Point, fishing, sharks, flooding, or underwater logs.

It needs a legitimate mechanism for learning from what the world exposes.

## 8. Product composition

The intended commercial paths are modular.

### QSL

```text
QSL
```

For customers who want the language/runtime foundation independently.

### Discoveries stack

```text
QSL
+ Wilds.Discoveries
+ optional Add-ons
```

For customers interested in evidence, resolution, vector movement, evolving state, or related systems without requiring Wilds Core.

### Wilds stack

```text
QSL
+ Wilds Core
+ optional Add-ons
```

For customers interested in generic simulation processing without requiring Discoveries.

### Full adaptive stack

```text
QSL
+ Wilds.Discoveries
+ Wilds Core
+ Entities
+ optional Add-ons
```

This is where the full adaptive-agent/world model can emerge.

## 9. Add-ons are capability-based

Future add-ons must not unnecessarily become "Wilds-only" or "Discoveries-only" packages.

The governing rule is:

> **Add-ons depend on capabilities, not product identity, wherever the required behavior can be expressed through stable shared contracts.**

Conceptually, an add-on might declare:

```text
Requires:
  QSL

Uses:
  Vector State

Optional:
  Discoveries
  Entities
  Temporal History
```

rather than declaring Wilds Core as a dependency when no Wilds-specific behavior is actually required.

This allows add-ons to serve multiple purchase configurations.

## 10. Minimum-dependency certification

Every product and add-on should certify against its minimum promised dependency set.

Examples:

- QSL certification runs independently.
- Wilds.Discoveries standalone certification runs without Wilds Core.
- Wilds Core certification runs without Discoveries.
- A QSL + Discoveries add-on certifies without Wilds Core.
- A QSL + Wilds add-on certifies without Discoveries.
- An add-on requiring the full stack states that requirement honestly and certifies against it.

This converts modularity from marketing language into an executable architectural guarantee.

## 11. Independent roadmaps

Each product should be able to receive fixes, features, certification work, and future value without requiring simultaneous movement across the entire family.

### QSL roadmap

1. Maintain the certified language/runtime boundary.
2. Complete remaining stabilization and audits.
3. Harden standalone API and developer experience.
4. Improve diagnostics, tooling, and integration surfaces.
5. Maintain independent packaging and release certification.

### Wilds.Discoveries roadmap

1. Establish the independent project boundary.
2. Preserve and recertify existing Discovery/evidence semantics.
3. Certify standalone operation.
4. Certify base-vector and accumulated-movement behavior.
5. Certify bias/resilience interaction without modifying evidence.
6. Explore whether preference, ordering, merging, conflict, and trend emerge from the same general vector composition primitive.
7. Integrate with Entities and Wilds only after the standalone primitives are stable.

### Wilds Core roadmap

1. Continue certifying the simulation substrate.
2. Preserve consumer ownership and identity.
3. Stabilize public extension boundaries.
4. Certify Wilds + QSL independently of Discoveries.
5. Add Discoveries integration through explicit contracts rather than implicit coupling.

### Entities roadmap

1. Complete current Entity certification.
2. Preserve Entity independence as its own project boundary.
3. Establish the agent strategy interface/surface only after the Entity contract is stable.
4. Certify Entity + QSL behavior.
5. Certify Entity + Discoveries behavior.
6. Certify Entity + Wilds behavior.
7. Certify the full Entity + Wilds + Discoveries + QSL composition.
8. Begin agent-strategy gates only after those boundaries are proven.

### Add-on roadmap

1. Do not create an add-on merely because a useful idea exists.
2. Identify the minimum capability contract required.
3. Certify against that minimum dependency set.
4. Support optional richer integration where additional products are present.
5. Let actual product use determine which add-on families deserve long-term investment.

## 12. Proposed Entity agent-strategy gates

Once the Entity substrate is certified, candidate cross-architecture gates are:

```text
A1 - ReAct-style reason / act / observe
A2 - Plan-and-Execute
A3 - Reflection / feedback adaptation
A4 - Orchestrator / Worker
A5 - Multi-agent information asymmetry
```

Every strategy should obey the same invariants:

1. No agent receives privileged simulation truth.
2. Unknown vocabulary remains unresolved until legitimately available.
3. Evidence remains distinct from bias.
4. Discoveries preserve provenance.
5. Agent reasoning cannot manufacture successful world outcomes.
6. Actions occur only through legitimately exposed capabilities.
7. Entity identity survives the complete processing path.
8. Strategy-specific concepts remain outside Wilds Core unless certification demonstrates a genuinely general missing primitive.

## 13. Canonical integration proof

Jerry at Miller's Point remains a useful eventual adversarial integration proof.

Jerry begins without fishing-world knowledge.

The consumer may seed minimal personality Discoveries.

The world subsequently provides legitimate Discoveries about fishing conditions, risk, reports, flooding, hazards, and alternatives.

The certification should demonstrate that Jerry can accumulate enough legitimate state to form a preference and eventually attempt an exposed action without any fishing-specific intelligence being built into Wilds Core.

The deliberately mundane final demonstration remains:

> **Eventually, Jerry may put a line in the water.**

Its significance is that the path to that action emerged from generic certified systems.

## 14. Strategic product rule

The product family should preserve four simultaneous truths:

> **QSL should not need to wait on Wilds.**
>
> **Wilds.Discoveries should not need Wilds Core to justify its value.**
>
> **Wilds Core should not require Discoveries to justify its value.**
>
> **Entities should be the place where interchangeable agent strategies become participants, rather than turning Core into an AI framework.**

The products may move at different speeds, solve different customer problems, and receive independent future investment.

Composition should make them stronger without erasing their boundaries.

## 15. Product architecture in one sentence

> **QSL provides the common language, Discoveries provides evolving epistemic state, Wilds provides the simulation world, Entities provides the agent-facing participant, and capability-based add-ons extend whichever parts a customer actually chooses to use.**
