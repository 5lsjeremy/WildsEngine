# Wilds Architecture

> **Start where you need us. Build on what we built.**

Wilds is designed as a layered technology family rather than a monolithic AI framework.

You can use the language. You can use the runtime. You can use the Entity processor and bring your own behavior system. Or, as the product family grows, you can compose the complete stack.

The important part is that you should not have to surrender the architecture you already trust just to use ours.

## Architecture at a glance

```text
                         Your Application
                               |
                    choose the layers you need
                               |
          +--------------------+--------------------+
          |                    |                    |
         QSL              Wilds Entities      Agent Behavior
          |                    |                    |
          |              discoveries,         your system or
          |              evidence, state      optional Wilds
          |                    |               behavior layer
          |                    +---------+----------+
          |                              |
          +----------------------- Wilds Core
                                         |
                              consumer-owned world
```

The architecture is intentionally open at its boundaries.

A behavior tree, utility system, planner, state machine, custom agent, or LLM-backed strategy can consume the Entity state you choose to expose. Wilds does not require one canonical model of intelligence.

## QSL

QSL is the semantic and execution language used throughout the broader architecture.

It is also useful on its own.

QSL operates against legitimately available state. Native properties on a bound consumer object are available immediately. Runtime properties become available through explicit state attachment. Properties that are neither native nor attached remain unresolved.

That rule matters because QSL does not get to invent state simply because an expression asks for it.

If QSL is your starting point, that is a valid stopping point too.

Build on it.

## Wilds Core

Wilds Core is the common runtime substrate for the Wilds product family.

Core provides the shared foundations used by higher-level Wilds projects, including the common object model centered on `IBaseObject` and the generic processing infrastructure built around it.

The goal of Core is not to know what your world means.

Your application owns its objects and domain meaning. Wilds provides machinery for processing those objects without requiring Core to become game-specific or consumer-specific.

Higher-level Wilds projects may depend on Core. That is intentional. Product modularity does not require duplicating the common runtime foundations in every package.

## Wilds Entities

Wilds Entities is the agent-facing layer of the current architecture.

Entities depend on Wilds Core and provide the place where a participant in the world can carry the state needed to become an agent.

An Entity can represent things such as:

- identity;
- capabilities;
- discoveries;
- evidence;
- resolved and unresolved state;
- profile state;
- accumulated experience;
- relationships to the world around it.

The Entity processor does **not** have to own behavior selection.

That distinction is intentional.

> **Entities can tell you who the participant is and what state it legitimately carries without dictating how you must decide what it does next.**

## Bring your own behavior

Wilds is designed so that the Entity layer can be a stopping point.

If your application already uses a behavior system you trust, keep it.

Examples include:

```text
Behavior Tree
Utility AI
GOAP / Planner
State Machine
Scripted Controller
Custom Agent Runtime
LLM-backed Agent
```

Your behavior system can consume appropriate Entity state and propose intent through the capabilities made available by the consumer.

Wilds remains responsible for its own runtime guarantees. Your behavior implementation remains yours.

This gives the architecture a simple boundary:

```text
Entity state
     |
     v
Your behavior system
     |
     v
proposed intent / action
     |
     v
Wilds / consumer world
     |
     v
actual outcome
```

A decision system can propose what should happen. It does not get to rewrite what actually happened.

## Discoveries are native to Entities

Discoveries are not being treated as a detached knowledge database added from the outside.

They belong naturally to the Entity model.

An Entity can receive discoveries, accumulate evidence, retain unresolved state, and change as additional information arrives.

The developing vector model is intended to preserve not only a final result but the movement toward that result.

Conceptually:

```text
base discovery
      |
incoming evidence
      |
accumulated movement
      |
partial state
      |
additional evidence
      |
resolution
```

An unresolved state can therefore still be meaningful. It can express direction and accumulated movement without falsely claiming resolution.

## Profile, bias, and experience

An Entity profile can provide initial conditions without becoming a hard-coded personality engine.

For example, a consumer might establish that an Entity begins with tendencies equivalent to:

```text
likes fishing
accepts some risk
is cautious about unfamiliar information
```

Those starting conditions can participate in the same evolving Entity state that later world experience influences.

The intended rule is simple:

> **Bias can influence how strongly information moves an Entity. Bias must not rewrite the evidence itself.**

The consumer establishes where an Entity begins. Experience can influence where it goes from there.

## A world the agent does not already know

The architecture is intended to support agents that do not arrive with privileged knowledge of a consumer world.

Consider an Entity named Jerry.

Jerry does not need built-in knowledge of Miller's Point, fishing, sharks, flooding, underwater logs, or any other consumer-specific concept.

The world can expose legitimate information to Jerry over time:

```text
Miller's Point has good fishing.
A report says sharks are present.
Jerry is tolerant of some risk.
A flood occurred recently.
Downstream conditions may be different.
Some information remains unverified.
```

Those pieces of information do not require Wilds Core to understand fishing.

They give Jerry state from which a behavior system can eventually make a decision.

The significant demonstration is intentionally ordinary:

> **Eventually, Jerry may put a line in the water.**

The interesting part is how he got there without Wilds Core being taught what fishing means.

## Optional behavior and agency layer

A future Wilds behavior/agency project can sit beside Entities rather than being embedded inside it.

Conceptually:

```text
                    Wilds Core
                    /         \
                   /           \
             Entities        Behavior
                 |               |
           what I know       what I try
           who I am          what I choose
                 \               /
                  \             /
                   composition
```

That layer can provide opinionated behavior strategies for customers who want them while leaving the Entity processor usable with third-party or consumer-authored behavior systems.

Candidate strategies for later certification include:

- ReAct-style reason / act / observe loops;
- Plan-and-Execute;
- reflection and feedback-driven adaptation;
- orchestrator / worker models;
- multi-agent strategies;
- custom LLM-backed strategies.

These strategies are not requirements imposed on Wilds Core.

They are replaceable ways to turn legitimate Entity state into proposed behavior.

## The QSL bridge

The existing Wilds/QSL integration layer is allowed to be integrated and dependency-aware.

That is its job.

The bridge can depend on the Wilds capabilities it integrates and on QSL without forcing those same dependencies down into Wilds Core.

This keeps the foundational dependency direction clean while allowing the product experience to be cohesive.

Conceptually:

```text
QSL -------------------+
                       |
Wilds Core ------------+--> Wilds/QSL integration
                       |
Wilds Entities --------+
```

As new Wilds capability projects appear, integration should continue to happen at an appropriate bridge or composition layer rather than by creating unnecessary sibling-to-sibling dependencies.

## Full-suite composition

When multiple higher-level Wilds capabilities need to cooperate directly, the full suite can provide a composition project above them.

That project is allowed to know about the systems it composes.

For example:

```text
                    Wilds Core
                    /         \
                   /           \
             Entities        Behavior
                   \           /
                    \         /
                  Suite Bridge
                       |
                      QSL
```

The suite bridge exists specifically to compose behavior that does not belong inside either sibling project.

This protects a critical architectural rule:

> **Sibling products should not acquire direct dependencies merely because the full suite can do something more powerful when they are combined.**

## Product entry points

The architecture supports useful stopping points.

### QSL

Use the language/runtime and build everything else yourself.

```text
QSL
```

### Wilds Core + QSL

Use the common Wilds runtime and build higher-level systems yourself.

```text
QSL
+ Wilds Core
```

### Wilds Entities

Use the Wilds runtime and Entity processor, including the Entity-native discovery/state model, while bringing your own behavior system.

```text
QSL
+ Wilds Core
+ Wilds Entities
+ Wilds/QSL integration
+ your behavior system
```

### Full Wilds agent stack

Use the runtime, Entity processor, QSL integration, optional Wilds behavior/agency capabilities, and suite-level composition.

```text
QSL
+ Wilds Core
+ Wilds Entities
+ Wilds Behavior / Agency
+ integration bridges
+ optional add-ons
```

These are not intended to be crippled tiers whose only purpose is to force an upgrade.

Each stopping point should provide legitimate value for developers who want to build the remaining layers themselves.

## Add-ons

The add-on ecosystem should follow the same philosophy.

An add-on should depend on the minimum capabilities it actually needs rather than requiring an entire suite by default.

Some add-ons may target QSL.

Some may target Core.

Some may target Entities.

Some may target the future behavior layer.

Some may require a full-suite bridge because their value genuinely comes from cross-product composition.

The dependency should reflect the engineering requirement, not a packaging strategy.

## Architectural dependency rules

The current direction can be summarized by a few rules.

1. **QSL remains independently useful.**
2. **Wilds Core remains the common Wilds runtime substrate.**
3. **Core should not depend upward on higher-level Wilds capability projects.**
4. **Higher-level Wilds projects may legitimately depend on Core and `IBaseObject`.**
5. **Discoveries remain native to the Entity model unless certification gives us a concrete reason to move them elsewhere.**
6. **Entities must not require one canonical behavior strategy.**
7. **Consumers can bring their own behavior system.**
8. **Optional Wilds behavior/agency capabilities should compose with Entities rather than redefine them.**
9. **Cross-product behavior belongs in an integration or suite bridge rather than forcing sibling dependencies.**
10. **Add-ons should depend on the minimum capabilities they genuinely require.**
11. **QSL must not invent unavailable consumer state.**
12. **Agent reasoning must not become simulation truth merely because an agent proposed it.**

## Certification philosophy

Architecture promises should become executable certification wherever practical.

Examples include proving that:

- Core builds and certifies without optional higher-level capability projects;
- Entities use Core without requiring a particular external behavior architecture;
- Entity state remains usable by consumer-supplied behavior systems;
- QSL state availability rules remain intact through Wilds integration;
- agent strategies receive only legitimate state;
- proposed actions do not masquerade as successful outcomes;
- cross-product composition does not introduce accidental sibling dependencies;
- consumer-owned object identity survives the processing paths that promise to preserve it.

The goal is not merely modular-looking source code.

The goal is to make the boundaries trustworthy.

## Build on what we built

Wilds is not intended to demand that every developer adopt the same stack from language to cognition.

Use QSL if QSL solves your problem.

Use Core if you need the runtime.

Use Entities if you want the Entity processor and evolving Entity state.

Bring your own behavior tree, planner, state machine, agent runtime, or LLM if that is where your expertise already lives.

Use additional Wilds behavioral capabilities when they provide more value than the system you would otherwise build yourself.

> **Start where you need us. Stop where you have what you need. Build on what we built.**
