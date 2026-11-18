# Wilds Product Family — Executive Summary

The Wilds product family is a modular architecture for stateful processing, dynamic simulation, knowledge development, emergent narrative, and agent-ready systems without requiring the underlying platform to understand the consumer's domain.

The product family is intentionally built around **independent stopping points**. A customer can adopt one foundation, combine multiple layers, or use the flagship integrated stack while retaining its existing planner or behavior technology.

Two principles remain constant:

**The consumer owns meaning. Wilds provides machinery for working with it.**

**Information existing in the world does not automatically become knowledge available to an entity.**

The current product roadmap is now explicitly locked. It ends at **Consequence projection**. Planning, behavior selection, persistent agency, and action systems are downstream integrations rather than requirements for completing the flagship stack.

---

## QSL — Stateful Processing

QSL is a standalone stateful, domain-neutral processing runtime designed to operate against consumer-owned objects and explicitly supplied runtime state.

Consumer objects can be bound directly into QSL processing. Native properties become available to the runtime, while additional runtime vocabulary can be introduced through explicit state attachment.

The processing boundary is straightforward:

- **Native state is available.**
- **Explicitly attached state is available.**
- **Everything else remains unresolved.**

QSL may read, evaluate, compare, calculate against, compose, and mutate authorized state without needing to understand what the consumer's vocabulary means.

**QSL does not define the world. It provides a controlled processing environment through which consumers can expose, evaluate, and change their own world.**

A customer can use QSL and stop there.

---

## Wilds Core — Simulation Infrastructure

Wilds Core is the standalone domain-neutral simulation substrate.

It provides machinery for processing units, temporal activity, relationships and topology, emissions, propagation, coordinated work, and movement of consumer-owned information through a simulation.

Core does not require consumer information to be stripped of domain meaning. It treats that meaning as externally owned.

**The consumer defines the world. Wilds Core provides machinery through which that world can operate.**

Core is also treated as established canon during current Discoveries development. Discoveries may integrate with Core, but Core is not redesigned for Discoveries convenience without a separate architectural discussion and decision.

A customer can use Core beneath its own scripting, knowledge, and AI stack and stop there.

---

## Wilds.Discoveries — Knowledge and Emergent Narrative

Wilds.Discoveries is a standalone knowledge-processing product for turning legitimately available information into structured, attributable, evolving knowledge.

Although Core is currently a dependency, that dependency does not define the Discoveries ontology. The intrinsic knowledge model is designed to remain separable so the dependency can be reduced or broken in the future without redefining what Discoveries is.

The target knowledge progression is:

**Consumer Reality → Evidence → Proof → Facts → Discoveries → Knowledge Geometry / Momentum → Clusters → Scoped Truths → Consequences**

### From facts to developing knowledge

Facts accumulate validated Evidence, but different entities do not have to reach the same conclusion from the same information. **Consumers define resolution policies that use legitimately available knowledge together with the entity's own native properties to bias how that information resolves.**

Discoveries does not need to understand what traits such as caution, trust, experience, stubbornness, curiosity, or any other consumer\-defined property mean. It only needs to expose those authorized values to the policies the consumer supplied. The policy owns the interpretation.

That means identical Evidence can legitimately develop differently for different entities. One may resolve a Discovery quickly, another may remain uncertain, and another may trend toward a competing conclusion. The Evidence and Facts remain attributable throughout that process rather than being rewritten to justify the eventual result.

An unresolved Discovery can also remain useful. Consumer\-defined vector dimensions can represent how its contributing knowledge is developing over time. Successive immutable evaluations can expose direction, velocity, acceleration, convergence, divergence, reversal, and other curve\-driven behavior without falsely claiming resolution.

**Resolution tells us how the entity currently resolves what it knows. Geometry and momentum tell us where that knowledge is heading.**

### From geometry to competing narratives

Vectors can form recognizable geometric structures. Clustering allows Discoveries to preserve tight consensus where it really exists while also retaining divergent or opposing structures where evidence supports more than one narrative.

The system does not need to manufacture an average simply because several entities disagree.

### Truth as congealed composition

A scoped Truth is produced when consumer-authorized congealment rules determine that a composition of Discoveries has become sufficiently stable.

That Truth retains the series, clusters, Discoveries, stability, confidence, and provenance that produced it.

Critically, a scoped Truth is **not automatically Consumer Reality**.

That distinction allows true and false narratives to emerge honestly. A population may develop a coherent narrative from incomplete, misleading, or false information. That narrative may gain momentum, cluster, stabilize, and project Consequences while still remaining distinguishable from authoritative consumer world state.

This makes Discoveries useful not only for knowledge accumulation but for emergent narrative, rumor, competing interpretation, revision, and consequence-aware agent context without requiring the runtime to understand the domain itself.

### Consequence as the stopping boundary

Consequences describe what available knowledge says may happen next under consumer-provided relationships and policies.

Discoveries does not secretly know that a stove burns, soup poisons, electricity hurts, or any other consumer noun implies a particular outcome. Those relationships must be supplied, observed, learned, or authorized by the consumer.

**Discoveries stops at what the entity has grounds to expect may happen. It does not decide what the entity should do.**

A customer can use Discoveries with its own planner, behavior tree, utility system, LLM agent, or proprietary AI stack.

---

## Wilds + QSL — Programmable Simulation

Wilds Core and QSL combine into a programmable simulation offering.

Core supplies the simulation substrate. QSL supplies stateful execution against consumer-owned objects and explicitly available runtime vocabulary.

Together they allow consumers to expose, process, and change their own world while preserving ownership and domain neutrality.

A customer can use this combination and provide its own knowledge and agency systems.

---

## Wilds + QSL + Discoveries — Flagship Integrated Stack

This is the locked flagship product boundary.

The flagship combines:

- domain-neutral simulation;
- controlled stateful processing;
- Evidence, Proof, and Fact development;
- entity-local Discoveries;
- knowledge geometry and momentum;
- competing geometric narratives;
- scoped Truth congealment;
- Consequence projection;
- exact provenance across those transitions.

The flagship output can tell a downstream agent:

- what state is legitimately available;
- what knowledge has been developed;
- what remains unresolved;
- which way that knowledge is moving;
- what competing narratives exist;
- what has stabilized into scoped Truth;
- what Consequences are currently projected.

**That is the completion boundary. Agency begins after it.**

---

## Agency and Behavior Integrations

Agency is intentionally pluggable and downstream of the flagship stack.

Consumers may connect Wilds outputs to:

- behavior trees;
- GOAP planners;
- utility AI;
- state machines;
- LLM agents;
- custom planners;
- proprietary game AI;
- future Wilds-native agency tooling.

A Wilds-native behavior system may be explored in the future, but it is **not required to complete the current product roadmap**.

This protects both product scope and customer choice. Wilds provides sophisticated simulation and knowledge context without requiring customers to discard the decision technology they already trust.

---

## Add-ons and Integrations — Specialization

Specialized capabilities can be delivered as add-ons, adapters, tools, engine integrations, consumer-specific policies, debugging/visualization tools, or domain-aware modules.

Add-ons are allowed to understand domains. Generic Wilds machinery is not.

**Specialized systems may depend upon generic Wilds machinery. Generic Wilds machinery must not silently become dependent upon their specialized meaning.**

---

# Locked Product Tree

## 1. QSL

**Stateful Processing**

Standalone controlled execution over consumer-owned objects and explicitly supplied runtime state.

## 2. Wilds Core

**Simulation Infrastructure**

Standalone generic infrastructure for temporal processing, coordination, topology, propagation, emissions, and simulation mechanics.

## 3. Wilds.Discoveries

**Knowledge and Emergent Narrative**

Standalone epistemic processing from Evidence through Discoveries, knowledge geometry, competing narratives, scoped Truths, and Consequences.

## 4. Wilds + QSL

**Programmable Simulation**

The simulation substrate plus stateful execution.

## 5. Wilds + QSL + Discoveries

**Flagship Integrated Stack**

Simulation + stateful processing + evolving knowledge + emergent narrative + scoped Truth + Consequence projection.

## Downstream ecosystem

**Agency and specialization**

Behavior systems, planners, LLM agents, engine integrations, and domain-specific add-ons consume the flagship output but are not required product layers.

---

# Explicit Out-of-Scope Rules

The following do **not** belong to the required completion scope of the current flagship roadmap:

- building a Wilds-native behavior tree;
- building a GOAP planner;
- building a utility-AI framework;
- building an LLM-agent framework;
- building a state-machine framework;
- building a universal planner;
- persistent agency as a required platform layer;
- automatic action selection;
- combat-AI systems;
- NPC scheduling systems;
- behavior-tree editors;
- a universal semantic ontology;
- built-in semantic definitions for consumer nouns;
- automatic promotion of scoped Truth into Consumer Reality;
- automatic promotion of Consequence into action;
- redesigning Wilds Core to make Discoveries implementation easier.

Those capabilities may exist in consumer applications, optional integrations, add-ons, experiments, or future separately approved products.

**No required product layer is added beyond Consequence projection without an explicit future scope decision.**

---

# Semantic and Epistemic Custody

The architecture uses three questions to protect its boundaries:

**Where did this meaning come from?**

**Who is entitled to know it?**

**Does this capability belong inside the locked product scope?**

The architecture is operating as intended when meaning originates from identifiable consumer state, observation, Evidence, policy, or another authorized source; entity knowledge develops through legitimate epistemic transitions; and new capabilities respect the locked stopping boundary.

The architecture has failed when the effective answer becomes:

- **“Wilds just knew.”**
- **“The entity knew because the world knew.”**
- **“A scoped Truth became reality because the system said so.”**
- **“We added another required product layer because it would be interesting.”**

---

## Results Are Attributable, Not Opaque

Wilds must not produce knowledge as an unexplained black-box conclusion.

Every meaningful epistemic result must remain attributable through the authorized information, consumer-supplied state, Evidence, Proof, Facts, Discoveries, policies, geometric development, clusters, scoped Truths, and other legitimate inputs that produced it.

Where consumers supply typed objects, state, vocabulary, and domain-aware policies, that information remains consumer-owned and attributable through processing. Wilds may process those values without collapsing them into hidden platform semantics.

Provenance is therefore not merely diagnostic history. It is part of the usable structure of developed knowledge.

Consumers must be able to retain the identity and provenance necessary to understand how an available result was produced, subject to legitimate knowledge boundaries and the capabilities of the result contract. An entity may only receive information it is entitled to know, but Wilds must not hide the attributable path behind knowledge it legitimately provides.

This requirement continues through knowledge geometry, clustering, scoped Truth formation, and Consequence projection. A higher-order result does not erase the attributable structures that produced it.

**Wilds may develop an answer. Wilds must not hide where that answer came from.**

### Provenance Chain

Consumer Reality → Authorized State / Observation → Evidence → Proof → Facts → Discoveries → Knowledge Geometry / Momentum → Clusters → Scoped Truths → Consequences

At every transition: **preserve identity • preserve provenance • preserve consumer ownership • preserve legitimate knowledge boundaries**.

# One Architecture, Five Product Offers

- **QSL processes state.**
- **Wilds Core processes simulation.**
- **Wilds.Discoveries develops knowledge, narratives, scoped Truths, and Consequences.**
- **Wilds + QSL provides programmable simulation.**
- **Wilds + QSL + Discoveries is the flagship integrated stack.**
- **External agency decides what to do next.**
- **Add-ons and integrations specialize the system.**
- **The consumer owns meaning and Consumer Reality.**

The result is a product family with useful places to enter, integrate, extend, and stop, and a flagship whose completion does not depend on owning the customer's agency stack.
