# Wilds Discovery-Driven Agent Model

> **The agent does not need to know the world. It needs to know how to learn from one.**

## Status

This document captures the current design direction for discovery-driven agents in Wilds. It is an architectural proposal and certification target, not a claim that every element described here is already implemented.

## 1. Premise

A Wilds agent does not require prior semantic knowledge of the consumer world it enters.

It does not need to know what Miller's Point is. It does not need built-in concepts for fishing, sharks, flooding, underwater logs, boats, galleries, miners, weapons, stores, or any other consumer-owned thing.

Instead, the agent brings a reusable capability:

> **Receive discoveries, accumulate movement, resolve facts, develop bias, and allow resolved experience to influence future resolution.**

The world supplies meaning through experience. The agent supplies the machinery for dealing with that experience.

This preserves a core Wilds/QSL rule: native bound-object properties are immediately available; runtime properties become available only after explicit state attachment. Anything neither native nor attached remains unresolved, and processing against it must fail.

An agent therefore cannot become omniscient merely because its implementation happens to be backed by a broadly trained model.

## 2. Discovery is state in motion

A Discovery is not merely a fact waiting to become `true` or `false`.

A Discovery may exist before resolution and may carry:

- provenance;
- evidence;
- strength;
- direction;
- accumulated movement;
- resolution requirements;
- current resolution state.

Its vector provides a representation of that movement.

Conceptually:

```text
V(next) = F(V(current), V(incoming))
```

A Discovery requiring three supporting conditions does not have to behave like an unfinished Boolean:

```text
false -> false -> true
```

It may instead carry meaningful progression:

```text
0/3 -> 1/3 -> 2/3 -> 3/3
```

The `2/3` state matters. It expresses accumulated movement and what the Discovery is trending toward, even though its resolution contract has not yet been satisfied.

Resolution is the point at which that accumulated state earns a resolved result. It is not the point at which the prior movement suddenly comes into existence.

## 3. Unseeded discoveries are still vectors

A Discovery does not have to originate in an agent profile to participate in this model.

An unfamiliar Discovery may enter as a base vector, accumulate additional evidence, and remain unresolved while it waits for the final push required by its resolution contract.

For example:

```text
Miller's Point flooded yesterday.
Downstream fishing may now be good.
Downstream contains fewer underwater logs.
```

If only two of three required conditions have been established, the Discovery may remain at `2/3`.

It is unresolved, but it is not nonexistent or inert. Its trajectory is already represented.

## 4. Bias and resilience use vector interaction

The same general vector model can represent an agent's existing bias or resilience.

Conceptually:

```text
Delta = F(Discovery, Bias)
```

The incoming Discovery describes pressure from the world. The agent profile describes the current state against which that pressure operates.

The resulting movement represents how the incoming information affects this particular agent.

The critical invariant is:

> **Bias must not rewrite evidence or world truth. Bias changes how strongly evidence moves the agent.**

Two agents may therefore receive the same Discovery and move differently without requiring the world to become different for either of them.

## 5. Personality becomes an initial condition

Part of the profile should be consumer-accessible from the Wilds-facing surface so a consumer can seed an agent's starting state.

For example, Jerry might begin with established Discoveries equivalent to:

```text
Jerry likes to fish.
Jerry does not mind risk.
Jerry is cautious about unfamiliar sources.
```

These are not intended to become permanent behavioral branches such as:

```csharp
if (jerry.IsCautious)
{
    // special cautious-agent logic
}
```

Instead, these facts contribute to Jerry's starting vector state.

A seed establishes where Jerry begins. It does not dictate what Jerry must eventually decide.

> **The consumer describes who Jerry initially is. The world participates in who Jerry becomes.**

This allows a profile to remain structurally agnostic about the consumer world while still providing meaningful initial disposition.

## 6. New dimensions can be learned from the world

The profile does not have to contain every possible dimension before the simulation begins.

An incoming Discovery can introduce a dimension the agent has never encountered. The relevant dimension can then become part of the agent's evolving cognitive state.

This mirrors the existing QSL vocabulary principle:

- unavailable state does not magically exist;
- legitimate exposure or attachment makes new state available;
- future processing can then operate against that state.

The portable agent therefore does not need a predefined `FishingBias`, `SharkBias`, `ArtBias`, `AlienBias`, or similar catalog of consumer concepts.

It needs rules for how new experience is received and resolved.

## 7. Miller's Point: preference without a preference subsystem

Consider Jerry learning two things:

```text
Miller's Point has excellent fishing.
Sharks have been reported at Miller's Point.
```

Both may be true simultaneously.

Wilds does not need to decide whether Miller's Point is "good." Instead, both Discoveries operate against Jerry's current state.

For a risk-tolerant Jerry, the resultant may trend toward:

```text
GO -> Miller's Point
```

For a highly risk-sensitive Jerry, the same world state may trend toward:

```text
DON'T GO -> Miller's Point
```

Neither outcome changes the evidence.

The world describes Miller's Point. Jerry's accumulated state determines what those conditions mean for Jerry.

This potentially removes the need for world-specific systems such as:

```text
FishingPolicy
SharkPolicy
FishingSpotPreferencePolicy
```

from Wilds Core.

## 8. Ordering and merging become consequences of composition

The same mechanism can be tested against multiple alternatives.

Suppose Jerry encounters:

```text
Miller's Point
  excellent fishing
  higher perceived danger

North Cove
  good fishing
  lower perceived danger

South Bank
  poor fishing
  very low perceived danger
```

Each candidate can interact with the same current Jerry state:

```text
Result(candidate) = F(candidateDiscovery, JerryBias)
```

Their resultants provide the basis for preference or ordering.

Under this model, preference, ranking, conflicting evidence, merging, resistance, and persuasion may not require separate domain-specific engines. They become candidate manifestations of the same underlying composition primitive.

This is a hypothesis to certify, not an assumption to bake into Core prematurely.

## 9. The important property is trajectory

The vector representation matters because Wilds can preserve more than a final answer.

A Discovery can move through states such as:

```text
V0 -> V1 -> V2 -> V3 -> ... -> Vn
```

That allows a distinction between:

```text
Jerry currently says NO.
```

and:

```text
Jerry currently says NO, but his state is moving strongly toward YES.
```

The second statement contains information that a Boolean result loses.

Wilds' temporal model gives this progression a natural place to live. Moments can preserve completed results, while Timelines can preserve progression across those Moments.

The historical explanation for a change can therefore be grounded in actual simulation history:

```text
prior state
+ incoming Discovery
+ evidence/provenance
= resultant movement
```

The system need not invent a plausible explanation afterward when the actual causal progression is available.

## 10. Personality and experience can share an epistemic doorway

A useful design constraint is that seeded personality and later world experience should not require unrelated processing models.

For example:

```text
Seeded:
  Jerry likes fishing.             established
  Jerry tolerates risk.            established

Experienced later:
  Fishing was excellent here.      established
  Sharks may be nearby.            partial
  Tom exaggerates danger.          established
```

These differ in origin and provenance, but they can participate in the same Discovery/evidence machinery.

That means a profile can be understood as the current resultant cognitive state produced by its initial conditions plus subsequent experience.

The important distinction remains:

> **Experience changes the agent. It does not rewrite what happened.**

## 11. Learning without world-specific cognition

The overall model becomes:

```text
Initial Profile
      |
Seed Discoveries
      |
Current Bias / Resilience
      |
World Discovery
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
Changed Cognitive State
      |
Next Discovery
```

The agent does not require a catalog of all possible world concepts.

Its portable knowledge is more fundamental:

> **how to encounter discoveries, accumulate movement, satisfy resolution requirements, and allow experience to affect subsequent encounters.**

Two initially identical agents placed into different worlds may therefore cease to be identical as their histories diverge.

## 12. QSL remains authoritative about available vocabulary

This model does not weaken QSL's state boundary.

It reinforces it.

The agent may operate against:

1. native properties on the consumer-bound object; and
2. runtime properties that have been explicitly attached.

Anything else remains unresolved.

The underlying language model's general training is not simulation truth and is not automatically available consumer state.

This is important for believable agents and for simulations in which agents must not cheat.

## 13. The LLM is not the source of world truth

An LLM-backed agent may eventually contribute capabilities such as:

```text
interpret
reason
plan
hypothesize
choose queries
propose actions
explain
```

But the architectural boundary should keep the simulation authoritative.

A useful working separation is:

```text
LLM / Agent Strategy
  reasoning
  planning
  hypothesis generation
  proposed actions

Wilds
  identity
  discoveries
  evidence
  provenance
  resolution
  history
  capabilities
  world interaction

QSL
  executable semantic composition over legitimately available state
```

Established LLM agent strategies can then be tested above the substrate rather than embedded as assumptions inside it.

## 14. Drop-in agents

This model suggests a future form of portable agent.

A drop-in agent does not need to arrive with knowledge of a specific game or simulation. It can arrive with a cognitive strategy and initial profile characteristics.

For example:

```text
Cautious Explorer
  risk sensitivity
  novelty response
  evidence threshold characteristics
  persistence
  source-trust disposition
```

Those characteristics describe how the agent begins processing experience, not what the world contains.

The same agent could theoretically enter unrelated consumer worlds and grow differently in each one, provided each world exposes its state and capabilities through the legitimate Wilds/QSL boundaries.

The certification question is not whether the agent already knows the world.

The certification question is whether it can learn enough about an unfamiliar world to act coherently within it.

## 15. Canonical certification: Jerry at Miller's Point

Jerry and Miller's Point should become a canonical adversarial certification scenario for this model.

### Initial conditions

Jerry begins without fishing-world knowledge.

Only minimal personality Discoveries are seeded through the consumer surface.

### World Discoveries

Introduce, through normal Wilds mechanisms:

1. Jerry likes fishing.
2. Jerry tolerates some risk.
3. Miller's Point has excellent fishing.
4. Someone reports sharks at Miller's Point.
5. Miller's Point experienced flooding.
6. Downstream fishing may now be good.
7. Underwater hazards may differ downstream.
8. Some reports remain partially verified.
9. Later evidence resolves previously incomplete Discoveries.

### Required proofs

The certification should prove that:

- unresolved Discoveries retain meaningful vector state and trajectory;
- a partial Discovery does not falsely advertise a resolved result;
- incoming Discoveries can accumulate movement toward resolution;
- profile seeds affect movement without replacing evidence;
- consumer-seeded state and world-acquired state preserve distinct provenance;
- contradictory evidence can change an agent's trajectory;
- alternatives can be compared without fishing-specific ranking logic in Core;
- historical movement remains attributable to actual incoming evidence;
- Jerry cannot operate against vocabulary that has not legitimately become available;
- Wilds Core contains no Miller's Point, fish, shark, flood, or fishing-specific cognition;
- QSL does not invent unavailable state;
- consumer-owned object identity remains intact throughout processing;
- an action is attempted only through a legitimately exposed capability.

### Final demonstration

Expose an appropriate consumer capability only through the normal boundary.

Let Jerry accumulate enough legitimate experience to decide whether to use it.

The final demonstration is intentionally mundane:

> **Eventually, Jerry may put a line in the water.**

The significance is not the fishing action itself. The significance is that the path to the action was produced without teaching Wilds Core what fishing is.

## 16. Architectural guardrails

Until certification proves otherwise, preserve these rules:

1. **World truth remains outside the agent.**
2. **Discovery is how world information reaches the agent.**
3. **Partial resolution is meaningful state, not fake completion.**
4. **Vectors represent direction and accumulated movement, not merely scores.**
5. **Bias influences response; it does not rewrite evidence.**
6. **Profile seeds establish initial conditions rather than permanent behavioral branches.**
7. **Unknown vocabulary remains unknown until legitimately available.**
8. **Consumer concepts must not leak into Wilds Core.**
9. **Provenance must survive every transformation.**
10. **History should explain behavioral movement rather than requiring post-hoc invention.**
11. **Established agent models should be certification subjects, not mandatory Wilds architecture.**
12. **Do not introduce separate preference, personality, conflict, or ranking subsystems until vector composition has been proven insufficient.**

## 17. Next certification direction

Do not redesign the current E7 discovery/evidence work around an LLM agent model.

First certify the primitive:

> **A Discovery can possess vector state, accumulate incoming movement, remain honestly unresolved, interact with an agent's current bias/resilience, and eventually produce a resolved result without violating provenance or vocabulary boundaries.**

Once that primitive is stable, use established agent control strategies as adversarial integration tests above it.

Candidate follow-on gates include:

```text
Agent Gate A - reactive reason / act / observe strategy
Agent Gate B - plan and execute strategy
Agent Gate C - reflection / feedback strategy
Agent Gate D - multi-agent information asymmetry
```

None of those strategies should require privileged access to simulation truth.

## 18. Post-Entity-Certification Agent Strategies

Once Entities and the Discovery-driven cognitive substrate are certified, Wilds should support multiple proven agent-control strategies **above** the entity boundary rather than baking any one strategy into Wilds Core.

The goal is not to make Wilds a ReAct engine, a planner, or a reflection framework. The goal is to prove that independently developed agent strategies can operate an Entity through the same honest Discovery, evidence, provenance, capability, and QSL boundaries.

### 18.1 ReAct: reason, act, observe

ReAct interleaves reasoning with actions and observations from an external environment.

For Wilds, this becomes a natural reference strategy:

```text
Entity receives legitimate Discovery
        |
Agent reasons from available state
        |
Agent proposes / selects an exposed action
        |
Wilds performs world interaction
        |
Result becomes new evidence / Discovery
        |
Agent observes the legitimate result
        |
repeat
```

A ReAct-driven Entity must receive no privileged simulation truth. Its observations must come through the same legitimate Wilds surfaces available to any other Entity.

**Certification target:** Something like Jerry should be able to reason, act, observe the consequences, and continue without ReAct-specific concepts leaking into Wilds Core.

### 18.2 Plan-and-Execute

Plan-and-Execute separates formation of a plan from execution of its steps. Execution results can then inform continuation or replanning.

Wilds already has a promising substrate for testing this pattern:

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
new Discovery / evidence
  |
continue, revise, or abandon plan
```

The planner may propose intent, but it does not get to declare that intent successful. Wilds remains authoritative about what actually happened.

A failed or cancelled piece of work must not become success merely because the planning agent expected success.

**Certification target:** An Entity should be able to pursue a multi-step objective and revise that plan from legitimate outcomes without gaining access to unavailable world state.

### 18.3 Reflection / Reflexion-style strategy

Reflection-oriented agents use feedback from prior attempts to improve later behavior. Reflexion is an established example in which linguistic feedback and episodic memory are used to influence subsequent decision-making without requiring the underlying model weights to be updated.

This is especially interesting for Wilds because the Discovery model already gives us a grounded distinction between:

```text
what happened
what the Entity discovered
what the Entity believed before the event
how the incoming vector moved it
what the Entity believes now
```

Reflection therefore should not be allowed to rewrite history. It should consume legitimate outcomes and use them to influence subsequent processing.

**Certification target:** Jerry should be able to fail, receive evidence about that failure, change his resultant cognitive state, and behave differently on a later attempt while the original evidence and provenance remain intact.

### 18.4 Orchestrator / Worker and multi-agent strategies

Later certification should test coordinated agents in which one agent delegates work to other agents or Entities.

This becomes particularly valuable when participants have different:

- capabilities;
- discoveries;
- relationships;
- evidence;
- beliefs;
- profile biases;
- legitimately available QSL vocabulary.

The important Wilds requirement is information asymmetry.

The orchestrator must not become omniscient merely because it is coordinating other participants. Knowledge acquired by one Entity should not automatically become knowledge available to every Entity.

**Certification target:** Multiple Entities should be able to collaborate while preserving identity, provenance, capability boundaries, and individual knowledge state.

### 18.5 Why these remain above Wilds Core

These strategies answer different questions about **how an agent directs its cognition and activity**.

Wilds answers a lower-level set of questions:

```text
What does this Entity legitimately know?
Where did that knowledge come from?
What remains unresolved?
How is incoming evidence moving it?
What capabilities can it legitimately invoke?
What actually happened when it acted?
```

That separation is intentional.

A ReAct agent, planning agent, reflective agent, or coordinating agent should all be able to inhabit the same certified Entity substrate.

If supporting a new strategy requires adding strategy-specific semantics to Wilds Core, that is a warning that the boundary may be wrong.

### 18.6 Proposed agent certification sequence

After Entity certification, the first cross-architecture gates should be:

```text
A1 - ReAct / reason-act-observe
A2 - Plan-and-Execute
A3 - Reflection / feedback-driven adaptation
A4 - Orchestrator / worker
A5 - Multi-agent information asymmetry
```

Each gate should use the same fundamental rules:

1. No strategy receives privileged simulation truth.
2. Unknown vocabulary remains unresolved until legitimately available.
3. Discoveries and evidence retain provenance.
4. Agent reasoning cannot manufacture successful world outcomes.
5. Actions occur only through exposed capabilities.
6. Entity identity survives the entire request and outcome path.
7. The strategy may influence cognition, but Wilds remains authoritative about simulation state.
8. Strategy-specific concepts should remain outside Wilds Core unless certification proves a genuinely general primitive is missing.

### 18.7 The intended end state

This creates a useful distinction between **the mind** and **the world substrate**.

Wilds does not need to ship one canonical intelligence.

It can eventually allow different agent strategies to inhabit certified Entities:

```text
ReAct Entity
Planner Entity
Reflective Entity
Coordinating Entity
Consumer-defined Entity
LLM-backed Entity
```

All of them encounter the world through the same Discovery system.

All of them remain subject to the same state, provenance, vocabulary, capability, and history rules.

That is the basis for a true drop-in agent model: the agent strategy can change without teaching Wilds Core what the consumer's world means.

## 18. The design in one sentence

> **A Wilds agent does not arrive knowing what the world means; it arrives knowing how to be changed by what it legitimately discovers, until accumulated experience gives it enough resolved state to act.**

Or, more simply:

> **The agent does not need to know the world. It needs to know how to learn from one.**
