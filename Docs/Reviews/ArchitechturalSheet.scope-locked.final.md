# Wilds Architecture — Core Invariants

## Purpose

Wilds is designed to provide **domain-agnostic processing of consumer-owned worlds**. The platform may process domain-specific objects, vocabulary, state, Evidence, Facts, Discoveries, knowledge geometry, scoped Truths, Consequences, and consumer policies without itself acquiring the meaning of those things.

**Consumer meaning may pass through Wilds. It must never become hidden Wilds meaning.**

The consumer is expected to understand its domain. QSL, Wilds Core, and Wilds.Discoveries process that domain while preserving semantic and epistemic custody.

The current product scope is locked. The flagship roadmap ends at **Consequence projection**. Agency, behavior selection, planning, and action systems are downstream integrations rather than requirements for completion of the flagship stack.

## 1. The Consumer Owns Meaning

Consumer applications may define arbitrarily domain-specific concepts. Domain-specific consumer policies are therefore **not architectural leaks** merely because they contain domain meaning.

**Domain-aware consumers are expected. Domain-aware platform semantics are not.**

## 2. QSL Is Stateful Processing

QSL is a standalone stateful, domain-neutral processing runtime over consumer-owned objects and explicitly supplied runtime state.

The language expresses operations. The runtime establishes the state against which those operations may legally execute. QSL may read, evaluate, compare, compose, calculate against, and mutate authorized consumer state without requiring QSL to understand the semantic meaning of that state.

A property becomes available because it is:
- native to a bound consumer object; or
- explicitly introduced through runtime state attachment.

A property that is neither native nor explicitly attached remains unresolved.

**Availability is explicit. Availability is not semantic truth.**

QSL is also the processing enhancement included with Wilds product offerings. That packaging relationship does not erase QSL's standalone product boundary: QSL remains usable as its own stateful processing runtime, while Wilds products can use it as their common programmable processing layer.

## 3. Wilds Core Is Simulation Infrastructure

Wilds Core is the standalone domain-neutral simulation substrate. It provides established machinery for processing units, temporal activity, relationships and topology, emissions, propagation, coordinated work, and movement of consumer-owned information through a simulation.

Core does not define the consumer world. It provides machinery through which that world can operate.

### Core is canon

Wilds Core is treated as established canon during current Discoveries development.
- Discoveries does not reshape Core for convenience.
- No Core contract or implementation is changed as part of Discoveries work without an explicit architectural discussion and decision.
- Discoveries may integrate with Core, but current Core integration must not silently become part of the intrinsic Discoveries ontology.

## 4. Processing Does Not Transfer Ownership

Consumer objects remain consumer objects. Passing information through QSL, Wilds Core, or Discoveries does not transfer semantic ownership to the platform.

QSL may evaluate it. Core may transport or process it. Discoveries may organize it into attributed knowledge structures. A consumer-supplied policy may interpret it.

None of those operations inherently teach the platform what the underlying concept means.

## 5. World State Is Not Entity Knowledge

Information may exist in the consumer world without being legitimately available to an entity as knowledge.

Discoveries protects this boundary. Consumer Reality, Evidence, Proof, Facts, and Discoveries participate in an epistemic process before information can contribute to higher-order knowledge.

Observation or authorization may describe **how Evidence is admitted**. They are not separate canonical knowledge objects in the intrinsic progression.

**The world knowing something does not mean an entity knows it.**

## 6. Wilds.Discoveries Is a Standalone Knowledge and Emergent-Narrative Product

Wilds.Discoveries is intended to stand on its own even though Wilds Core is currently a dependency.

The current intrinsic knowledge progression is:

**Consumer Reality → Evidence → Proof → Facts → Discoveries → Knowledge Geometry / Momentum → Clusters → Scoped Truths → Consequences**

This progression separates admitted information, validation, local discovery, geometric composition, stabilized narrative, and projected outcome.

Discoveries provides the machinery. The consumer provides the meaning.

## 7. Evidence, Proof, Facts, and Discoveries Remain Distinct

The architecture preserves epistemic boundaries:
- an observation is not automatically Evidence;
- Evidence is not automatically a Fact;
- believable Evidence is not necessarily true;
- Proof does not rewrite its Evidence;
- a Fact is not automatically available to every entity;
- a Discovery is not automatically resolved because it has partial support;
- an inference does not automatically become an observation;
- a hypothesis does not automatically become Consumer Reality.

Failure or unresolved state is preferred over invented knowledge.

## 8. Partial Knowledge May Have Direction Without Being Resolved

A Discovery need not collapse immediately into a boolean answer.

Partially resolved knowledge may participate in consumer-defined geometry. Successive immutable evaluations may expose position, velocity, acceleration, momentum, convergence, divergence, or reversal without falsely promoting the source Discovery to resolved state.

A two-of-three contributing state may therefore have meaningful directional momentum while remaining unresolved.

**Resolution and trajectory are distinct concerns.**

## 9. Knowledge Geometry Is Consumer-Defined

Facts and Discoveries may contribute to consumer-defined vector dimensions. Discoveries validates the geometric structure while leaving dimension meaning external.

The model supports:
- components and points;
- contributions and weights;
- motion separated from position;
- pluggable metrics;
- scoped ordered vector series;
- nonlinear curves and future mathematical strategies supplied through explicit boundaries.

Mathematical sophistication is allowed. Hidden semantic ownership is not.

## 10. Clustering Finds Structure, Not Consensus

Discovery-vector populations may form coherent geometric structures.

Clusters may represent convergence, divergence, competing shapes, broad variation, or outliers. Multiple coherent clusters may coexist.

**Discoveries must not manufacture consensus merely because aggregation is convenient.**

Discovery clustering is knowledge-space clustering. It is distinct from Core topology clustering, which may be used by an integration to select a population but does not define the semantic meaning of that population.

## 11. Truth Is Congealed Composition, Not Universal Reality

A scoped Truth is higher-order knowledge produced when consumer-authorized congealment rules determine that the composition of relevant Discoveries has become sufficiently stable.

Truth therefore retains its composition and provenance, including the vector series, clusters, and authoritative Discoveries that produced it.

A scoped Truth is **not automatically Consumer Reality**.

This distinction is intentional. A coherent narrative may congeal from incomplete, misleading, or false information while remaining attributable to the information that produced it. New experience may strengthen, weaken, compete with, or supersede later knowledge without rewriting the historical Truth.

This allows Discoveries to represent both true and false narratives without collapsing either into hidden platform knowledge.

## 12. Consequences Are the Final Knowledge Boundary

Consequences belong to Discoveries because an entity can learn or project what may happen without yet deciding what to do.

Consequences must originate through consumer-supplied meaning, entity experience, Evidence, policy, scoped Truths, learned relationships, or another authorized source.

Discoveries must not secretly know that a particular consumer noun implies a particular outcome.

The current product roadmap ends at:

**Knowledge → Scoped Truth → Consequence**

A Consequence describes what may result. It does not prescribe an action.

## 13. Agency Is Downstream and Pluggable

Planning, behavior selection, persistent agency, and action are intentionally downstream of the locked flagship scope.

Consumers may connect the flagship stack to:
- behavior trees;
- GOAP planners;
- utility AI;
- state machines;
- LLM agents;
- proprietary game AI;
- custom planners;
- future Wilds-native agency tooling.

No one agency system is required to complete QSL, Wilds Core, Wilds.Discoveries, or the flagship integrated stack.

Behavioral output does not automatically become Evidence, Fact, Discovery, Truth, or Consumer Reality. Any return into the knowledge chain requires an explicit authorized transition.

## 14. Knowledge Is Local Until Explicitly Transferred

Knowledge acquired by one entity, subject, or population does not automatically belong to another.

Sharing, reporting, sampling, aggregation, corroboration, adoption, social influence, or other knowledge transitions must remain explicit and attributable.

A scoped Truth from one knowledge context may become Evidence belonging to another entity only through an explicit transition. The source Truth remains the source Truth. Its ownership, series, clusters, Discoveries, and history are not transferred into the receiving entity. The receiving entity receives new Evidence that can preserve an exact reference to that source Truth, and its own consumer policies decide whether that Evidence contributes to later Facts or Discoveries.

This permits social influence and peer pressure without making consensus an automatic overwrite rule.

**Transporting information is not the same as knowing it. Aggregating information is not the same as declaring it world truth.**

## 15. Consumer Policies May Contain Domain Semantics

Consumers are explicitly permitted to provide domain-aware Evidence, Proof, contribution, vectorization, metric, curve, congealment, and Consequence-projection policies.

**The semantic authority must remain attributable to the consumer.**

The platform can execute a consumer rule without adopting that rule as platform knowledge.

## 16. Authoring Convenience Must Not Hide Meaning

The platform may remove mechanical authoring work when doing so does not erase semantic or structural responsibility.

Generated identifiers, safe mechanical defaults, and deterministic history-key helpers may make ordinary authoring easier. Consumer-owned policies, semantic keys, scopes, relationships, history identity, and other meaningful configuration remain explicit where they define interpretation or provenance.

A convenience API must not weaken structural integrity. When identity is already part of a dependent structure, as with identity-bound geometry mappings and profiles, the invariant takes precedence over convenience.

**Remove paperwork, not meaning.**

## 17. Standalone Product Boundaries Matter

The product family contains independent technical stopping points while sharing a common packaging strategy.

### QSL — Stateful Processing

A consumer can use QSL and stop. QSL also ships as the processing enhancement included with Wilds product offerings.

### Wilds Core — Simulation Infrastructure

A consumer can use Core beneath its own scripting, knowledge, and agency systems and stop. Wilds product offerings include QSL as the common stateful processing enhancement.

### Wilds.Discoveries — Knowledge and Emergent Narrative

A consumer can use Discoveries as a knowledge-processing system and connect its output to any downstream agency stack. Wilds product offerings include QSL while Discoveries retains its own intrinsic knowledge boundary.

### Wilds + QSL — Programmable Simulation

Core simulation and QSL stateful execution form an integrated programmable simulation offering.

### Wilds + QSL + Discoveries — Flagship Integrated Stack

The flagship combines simulation, stateful processing, epistemic development, knowledge geometry, competing narratives, scoped Truth formation, and Consequence projection.

Agency remains downstream.

## 18. Explicit Out-of-Scope Rules

The following are **not required for completion of the current flagship roadmap**:
- a Wilds-native behavior tree;
- GOAP implementation;
- utility-AI implementation;
- planner implementation;
- LLM-agent implementation;
- persistent agency runtime;
- automated action selection;
- combat AI;
- NPC scheduling;
- a behavior-tree editor;
- a universal semantic ontology;
- built-in understanding of consumer nouns;
- automatic promotion of scoped Truth into Consumer Reality;
- automatic promotion of Consequence into action;
- redesign of Wilds Core for Discoveries convenience.

These may exist as consumer integrations, add-ons, experiments, or future separately scoped products. They are not blockers for completion of the locked product tree.

**No new required product layer is added beyond Consequence projection without a future explicit scope decision.**

## 19. Gateways Prefer Explicit Failure Over Invention

When required state, vocabulary, authority, identity, provenance, knowledge, geometry, or processing conditions are unavailable, the architecture should prefer an explicit unresolved or failed result over silently manufacturing what is missing.

The standing requirements are:
- explicit availability;
- explicit ownership;
- explicit transitions;
- preserved identity;
- preserved provenance;
- entity-local knowledge;
- contradiction preservation;
- failure instead of invention.

## 20. Results Are Attributable, Not Opaque

Wilds must not produce knowledge as an unexplained black-box conclusion.

Every meaningful epistemic result must remain attributable through Consumer Reality, Evidence, Proof, Facts, Discoveries, consumer policies, geometric development, clusters, scoped Truths, and other legitimate inputs that produced it.

Where consumers supply typed objects, state, vocabulary, and domain-aware policies, that information remains consumer-owned and attributable through processing. Wilds may process those values without collapsing them into hidden platform semantics.

Provenance is therefore not merely diagnostic history. It is part of the usable structure of developed knowledge.

Consumers must be able to retain the identity and provenance necessary to understand how an available result was produced, subject to legitimate knowledge boundaries and the capabilities of the result contract. An entity may only receive information it is entitled to know, but Wilds must not hide the attributable path behind knowledge it legitimately provides.

This requirement continues through knowledge geometry, clustering, scoped Truth formation, and Consequence projection. A higher-order result does not erase the attributable structures that produced it.

**Wilds may develop an answer. Wilds must not hide where that answer came from.**

### Provenance Chain

**Consumer Reality → Evidence → Proof → Facts → Discoveries → Knowledge Geometry / Momentum → Clusters → Scoped Truths → Consequences**

Authorization and observation describe how Evidence may be admitted; they do not introduce another canonical knowledge object into this chain.

At every transition: **preserve identity • preserve provenance • preserve consumer ownership • preserve legitimate knowledge boundaries**.

## The Architectural Test

When evaluating a feature, processor, integration, policy, or future product proposal, ask:

**Where did this meaning come from? Who is entitled to know it? Does it belong inside the locked product scope?**

If meaning originates from identifiable consumer state, Evidence, policy, knowledge, or another authorized boundary, semantic custody remains intact.

If entity knowledge arises through legitimate epistemic processing, epistemic custody remains intact.

If the answer becomes **“Wilds just knew,” “the entity knew because the world knew,”** or **“we need another required subsystem because it would be cool,”** the boundary has failed.

## Architectural Thesis

- **QSL processes state.**
- **Wilds Core processes simulation.**
- **Wilds.Discoveries develops knowledge, narratives, scoped Truths, and Consequences.**
- **QSL enhances Wilds product offerings while remaining a standalone processing product.**
- **Wilds + QSL provides programmable simulation.**
- **Wilds + QSL + Discoveries is the flagship integrated stack.**
- **External agency decides what to do next.**
- **Add-ons and integrations specialize the system.**
- **The consumer owns meaning and Consumer Reality.**

The current roadmap ends at Consequence projection. That is a complete product boundary, not an unfinished step toward a mandatory behavior system.
