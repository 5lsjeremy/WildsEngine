# Wilds Engine - Architecture Introduction

Game worlds have a habit of becoming more than we originally planned for.

An NPC walks into an art gallery. A fish encounters a fishing lure. A patron finds himself at a poker table with a cranky miner.

None of those ideas is especially difficult to imagine. The difficult part is letting them belong naturally to the same world without scripting every possible interaction, building increasingly specialized data structures, or handing more and more responsibility to processing that becomes difficult to understand later.

And then tomorrow you add something else.

Maybe it's an object. Maybe it's a relationship, a signal, a new capability, or an entirely different social situation. It wasn't there when you first imagined the world, but now it belongs there.

**Your engine has to let it belong there too.**

If every new idea requires tearing down, rebuilding, or rebalancing the engine's understanding of the world, eventually the architecture starts deciding what that world is allowed to become.

That's a problem.

An NPC shouldn't feel alive only because we managed to anticipate enough reactions. A world becomes much more interesting when its inhabitants can participate in what is happening around them, including situations we didn't explicitly script for them.

That's where Wilds takes a different position.

**Wilds cannot be your game world, and it doesn't want to be.**

You know what an art gallery is. You know what the fishing lure means. You decide why the miner is cranky, why he's playing poker, what he wants, and whether anybody else in the room cares.

That's your world.

Describe those things. Give them properties, relationships, rules, capabilities, and meaning. Give your agents reasons to care about them.

Then hand them off.

Wilds works underneath that meaning. It deals with the machinery that lets those things participate together: state, relationships, work, provenance, propagation, causality, and history.

It doesn't need an `ArtGallery`, `FishingLure`, `CrankyMiner`, or `PokerTable` built into the engine.

It needs enough foundation for those things to exist, encounter one another, matter to one another, and create something that wasn't explicitly written as a path from A to B.

**Your game defines the world. Wilds gives its inhabitants room to experience it.**

## Surprise is part of the point

If we do this right, every once in a while Wilds should surprise you.

You should be playing your own game and find yourself asking:

> **"Did I code that?"**

Or maybe:

> **"Where is that NPC going?"**

Follow them.

There should be an answer.

Not because Wilds secretly decided what your NPC wanted. Not because some black box invented part of your game for you.

What you should find are things you understand: simple rules, state, relationships, previous experiences, and things happening in the world that came together in a way you never explicitly scripted.

**That's the kind of surprise we're after.**

You shouldn't have to write every interaction that can possibly happen. You should be able to describe enough of your world that interesting interactions have room to happen.

But there's another side to that bargain.

**Wilds spills its secrets. It has to.**

When something surprises you, follow it. Track the logs. Follow the provenance. Find out what happened and what brought the world to that Moment.

Because **"Did I code that?" should be a fun question, not a debugging nightmare.**

You still have a game to build. You have to tune it, balance it, understand it, and eventually put it in front of players.

So we don't get to hide behind emergence.

**If Wilds helps create the experience, Wilds should help you understand how you got there.**

That's how we leave room for surprise without asking you to surrender your vision.

## Take the ecosystem with you

Eventually, you finish the game.

The fish probably doesn't come with you.

The gallery closes. The lure gets packed away. The cranky miner can stay at the poker table. They belonged to that world.

That's okay.

**But why should everything you learned about making them matter to each other stay behind with them?**

What if, instead of starting over, the lessons from one finished world became part of the foundation for the next?

Your next game can have completely different objects, different relationships, different rules, and different meanings. In fact, we hope it does.

You don't need to bring the fish.

**Bring the ecosystem that taught the fish how to belong.**

Build on what worked. Change what didn't. Add things we haven't thought of yet. Let the next project teach you more about simulation without requiring Wilds to understand what that particular world means.

Wilds can move with you, not because it carries your old world into the new one, but because the machinery underneath that world was never supposed to belong to the fish in the first place.

## The problem underneath all of this

Wilds can surprise you, but it shouldn't leave you guessing. You should be able to follow what happened, understand why it happened, tune it when you need to, and take what you've learned with you into whatever world you build next.

Somewhere underneath all of this is the problem we've been chasing from the beginning:

> **A simulated entity should be able to encounter something the engine itself has never been taught to understand.**

The longer we worked on that problem, the clearer something else became.

**We couldn't solve it by teaching Wilds more and more about the world. We had to teach Wilds less about the world, and more about how a world works.**

That changes the questions the engine needs to ask:

> **What exists?**
>
> **What is happening?**
>
> **What work needs to be done?**
>
> **What is true right now?**
>
> **What is related to what?**
>
> **Can an effect reach something else?**
>
> **Does that effect matter enough to cause a response?**
>
> **If something responds, whose occurrence is it?**
>
> **What actually happened?**
>
> **Can we understand why it happened?**
>
> **How does it become part of history?**

The consumer supplies the answers that give those questions meaning. Wilds supplies the mechanics that allow those answers to participate in the same simulation.

**Wilds does not need to know what something is in order to coordinate what happens because of it.**

That leaves us with a useful boundary.

**The consumer owns the meaning of the world. Wilds owns the reusable simulation mechanics underneath it.**

Everything that follows comes from trying to preserve that boundary.

`IBaseObject` begins with **what exists**.

`IBaseUnit` begins with **what happens**.

From there, state, context, work, topology, propagation, provenance, causality, and history each answer another part of the problem.

**That's where the architecture begins.**

## Stateful processing and the Wilds model

Wilds does not exist because state machines or behavior trees are inherently wrong. They answer important questions. Wilds exists because a simulation can eventually need to ask a broader set of questions than behavior selection alone is intended to answer.

| Stateful / behavior-centered framing | Wilds framing |
| --- | --- |
| What state is the entity in? | What exists, and what is true here? |
| What condition or transition fired? | What occurrence has begun? |
| Which known behavior should execute? | What work must resolve? |
| What should this entity react to? | What is related to this entity or occurrence? |
| Which authored branch handles this stimulus? | How strongly does the stimulus propagate through the relationship? |
| Does the current behavior transition? | Is the receiving object eligible to respond? |
| Continue or replace the current behavior. | If it responds, begin an independently owned occurrence. |
| Preserve whatever history the consumer explicitly models. | Resolve the occurrence into a Moment and retain it through Timeline and Epoch history. |

The important difference is not that Wilds removes state. State remains fundamental. The difference is that **state is one question in the simulation rather than the container for every question the simulation can ask.**

An NPC does not need to live only as a continuously reacting state machine. It can participate in a world of relationships and occurrences whose concrete meanings remain consumer-owned.

An art gallery can matter without Wilds knowing what art is.

A fishing lure can matter without Wilds knowing what fishing is.

A cranky miner at a poker table can matter without Wilds knowing what a miner, mood, game of poker, or social encounter means.

Wilds concerns itself with the mechanics by which those things can exist, relate, influence, qualify, produce work, cause new occurrences, and leave history.

## The simulation question chain

The architecture is easiest to understand as a sequence of questions. Each major subsystem exists because a different question has become meaningful.

```text
What exists?
    |
    v
IBaseObject
    |
    v
What is happening?
    |
    v
IBaseUnit / Pulse
    |
    v
What work must resolve?
    |
    v
WorkUnit
    |
    v
What state and context does it encounter?
    |
    v
processing infrastructure
    |
    v
What actually happened?
    |
    v
Moment
    |
    v
What is related?
    |
    v
topology
    |
    v
How strongly does the effect reach it?
    |
    v
propagation
    |
    v
Is that enough to matter?
    |
    v
eligibility
    |
    v
Does another occurrence begin?
    |
    v
causal stimulus -> independent Pulse
    |
    v
What history did these outcomes create?
    |
    v
Timeline -> Epoch
```

Wilds deliberately avoids collapsing these questions into a single behavioral decision.

## Foundational runtime

The question-driven model begins with two parallel foundational abstractions: `IBaseObject` and `IBaseUnit`.

They have different purposes and different personalities in the runtime. Neither replaces the other.

### IBaseObject: what exists?

> **Wilds is asking:** *What is participating in the simulation?*

`IBaseObject` represents runtime existence. It provides stable identity for objects that participate in the simulation and forms the foundation for component ownership and relational topology.

The object-side runtime includes:

- `IBaseObject` for stable runtime identity and topology;
- `IComponent` for component-owned state, signal, and metadata;
- `IState` for state processing;
- `IContext` for variables and contextual metadata;
- `ISignal` for outward communication.

Component and context metadata can hold arbitrary consumer-owned values. The GUID remains canonical object identity; readable slugs can provide runtime context without replacing that identity.

An `IBaseObject` can therefore exist, hold runtime state, participate in relationships, and become involved in processing without becoming a work or history object.

### IBaseUnit: what is happening?

> **Wilds is asking:** *What occurrence is being coordinated or remembered?*

`IBaseUnit` represents runtime occurrence. It is the parallel foundation for coordinated work and temporal history.

```text
                         IBaseUnit
                            |
                         BaseUnit
                            |
          +---------+---------+--------+----------+
          |         |         |        |          |
        Pulse    WorkUnit    Moment  Timeline    Epoch
       request     work      result   history   history
```

Where `IBaseObject` identifies participants in the simulation, `IBaseUnit` identifies occurrences within the simulation.

```text
IBaseObject                         IBaseUnit
    |                                  |
    | runtime existence                | runtime occurrence
    |                                  |
    +-- identity                       +-- Pulse
    +-- component                      +-- WorkUnit
    +-- state                          +-- Moment
    +-- context / metadata             +-- Timeline
    +-- signal                         +-- Epoch
    +-- topology
```

An `IBaseObject` answers **what is participating**.

An `IBaseUnit` answers **what is happening, what work belongs to it, and how that occurrence becomes history**.

Objects can participate in work without becoming units. Units can carry references to arbitrary consumer-owned runtime objects without requiring those objects to become Wilds-specific gameplay types.

This is one of Wilds' foundational separations: **existence and occurrence are related, but they are not the same thing.**

## The IBaseUnit execution and temporal model

Wilds coordinates work and history through one unit family. Execution and history are not separate models joined afterward. They are stages and scales of the same unit model.

### Pulse: what is being asked to happen?

> **Wilds is asking:** *What coordinated occurrence has begun, and what must become true before it can honestly resolve?*

A `Pulse` represents the coordinated request.

The Pulse owns the `WorkUnit` instances required to resolve that request. It cannot resolve successfully merely because processing stopped or returned a value. Its required work must resolve consistently with the claimed outcome.

The Pulse establishes an execution boundary around an occurrence without needing to know the consumer-specific meaning of that occurrence.

### WorkUnit: what work must resolve?

> **Wilds is asking:** *What required work remains, and what happened to each required part?*

A `WorkUnit` represents individually tracked required work belonging to a Pulse.

WorkUnits can complete, fail, or be cancelled independently while remaining owned by the Pulse that requires them. A Pulse cannot honestly report successful resolution while required work remains unresolved or has produced an incompatible outcome.

This separates **execution machinery** from **execution truth**.

Tasks may perform lower-level schedulable work. A WorkUnit records whether a required part of the Pulse's obligation actually resolved.

### Moment: what actually happened?

> **Wilds is asking:** *What was the resolved outcome of this occurrence?*

A `Moment` represents a completed Pulse outcome.

Success, failure, and cancellation are historical truths. A failed computation is not rewritten into success merely because control returned from the processing route. The Moment preserves what actually happened.

Wilds therefore preserves the distinction between **what was requested** and **what became true**.

### Timeline and Epoch: what history did those outcomes create?

> **Wilds is asking:** *How does this resolved occurrence belong to a larger history?*

A `Timeline` collects Moments. An `Epoch` collects Timelines.

#### Concurrent Timeline ordering

Within a Timeline, concurrent terminal Moments are ordered by the serialization order in which they are successfully committed to that Timeline. Wilds does not guarantee request-submission order, Pulse-creation order, operation-completion order, or pre-commit finalization order across concurrent executions.

This preserves one coherent historical sequence without imposing artificial execution ordering on otherwise independent concurrent requests.

Timeline routing and Timeline ordering are separate concerns. Routing selects the destination Timeline. Once a destination Timeline has been chosen, successful commit determines the Moment's position within that Timeline.

```text
request              execution             result             history
   |                     |                    |                   |
 Pulse  ------------> WorkUnit ----------> Moment ----------> Timeline
                                                                  |
                                                                  v
                                                                Epoch
```

Consumer runtime-object references can survive this complete route without being converted into Wilds-specific gameplay types.

Wilds coordinates the occurrence and preserves its history. The consumer retains ownership of what the carried object or result means.

## Processing infrastructure beneath the units

> **Wilds is asking:** *What machinery is required to perform the work without confusing that machinery with the occurrence being coordinated?*

Instructions and tasks remain lower-level processing infrastructure.

```text
Instruction
    |
    v
coordinated processing intent
    |
    v
Task / Tasks
    |
    v
processors + registries
    |
    v
state / context
```

Instructions describe processing intent. Tasks provide schedulable work. Processors execute against the appropriate runtime structures. Registries preserve pending-work lifetime independently from processor leases.

Processor pools can therefore rent, execute, reset, and return processors without implicitly destroying the work being coordinated.

The lifetime of execution infrastructure is not necessarily the lifetime of simulation truth.

## State and context

> **Wilds is asking:** *What is true here, and what information is available to this work?*

State remains an important part of Wilds. It simply does not need to carry the explanatory burden of the entire simulation.

State and context provide runtime conditions encountered during processing. They do not define what those conditions mean to a particular game.

A consumer-owned value can represent an NPC's interests, an artwork, a lure, a social disposition, a physical object, or some future concept Wilds has never seen. That value may participate in processing without becoming a Wilds Core type.

This is how Wilds can use state without forcing every possible interaction to become another engine-known state.

## Relational topology

Once objects exist, the simulator can ask about their relationships without yet deciding what those relationships mean.

> **Wilds is asking:** *What is related to this object?*

Wilds objects can participate in directional `Neighbor` relationships.

Relationship identity is defined by:

```text
Neighbor.Id + Membership
```

Distance is independently mutable relationship state. The same pair of objects can participate through different memberships, and `A -> B` does not imply `B -> A`.

Topology mutation can flow through state processing, while reusable operators query relationship snapshots without mutating the source collection.

Topology answers which relationships exist and what their current relational state is. It does not decide what a gallery, lure, miner, NPC, social relationship, attraction, threat, or opportunity means.

That meaning remains consumer-owned.

## Propagation

Knowing that two objects are related does not yet mean that every occurrence involving one automatically forces a response from the other.

> **Wilds is asking:** *How much of this effect reaches the related object?*

Neighbor topology can feed a separate propagation model.

The current distance attenuation model is exponential:

```text
Iout = Iin * exp(-k * d)
```

Topology supplies relationship distance. Propagation interprets that distance and produces received intensity.

The meaning of intensity remains consumer-owned. It may represent any consumer-defined magnitude without becoming a Wilds Core gameplay concept.

Relationship and effect transmission are therefore different questions.

## Eligibility

Propagation determines what arrives. It does not determine whether the receiver must act.

> **Wilds is asking:** *Is what arrived sufficient for this Neighbor to qualify for a response?*

Eligibility evaluates the propagated result against the applicable threshold.

```text
source intensity
       |
       v
    topology
       |
       v
  propagation
       |
       v
received intensity
       |
       v
  eligibility
       |
       +---- eligible
       |
       `---- not eligible
```

The minimum-intensity threshold is inclusive. Intensity exactly at the threshold is eligible; intensity below it is not.

Eligibility is deliberately not behavior.

An eligible Neighbor has qualified for a subsequent response. Wilds has not declared what that response means, nor has it forced the Neighbor into a predetermined behavioral branch.

The consumer may ultimately interpret the qualifying occurrence as curiosity about a painting, interest in a lure, annoyance with another patron, concern about a threat, or something else entirely.

Wilds needs none of those concepts in Core.

## Causal execution boundary

The next architecture boundary begins after a Neighbor becomes eligible.

At this point Wilds encounters one of the most important questions in the execution model:

> **Wilds is asking:** *If this Neighbor responds, whose work is that response?*

The intended answer is: **the responding Neighbor's**.

The Neighbor's resulting work is not attached to the originating Pulse. Instead, the eligible Neighbor receives a causal stimulus and may originate an independent Pulse with its own WorkUnits and eventual Moment.

```text
Pulse A
   |
   v
topology
   |
   v
propagation
   |
   v
eligibility
   |
   v
causal stimulus to B
   |
   v
Pulse B
   |
   +--> WorkUnits B
   |
   v
Moment B
```

Pulse A and Pulse B remain independent execution units.

Causality answers:

> *What contributed to this occurrence beginning?*

Execution ownership answers:

> *Whose occurrence is this?*

Those are related questions, but they are not the same question.

**Causal provenance connects execution without merging execution ownership.**

Causal stimulus delivery and independent Neighbor Pulse creation are the next execution boundary to define and certify.

## From reaction to participation

This is the practical payoff of the architecture.

A reactive design can model an NPC responding to recognized inputs extremely well. Wilds adds a substrate through which the NPC can participate in a larger, consumer-defined world without requiring Wilds Core to enumerate that world's possible meanings.

Consider the opening examples conceptually.

### An art gallery

Wilds does not need to contain an `ArtGalleryState` or decide whether an NPC appreciates art.

The gallery and NPC can exist as runtime participants. Their consumer-defined data and relationships can enter processing. Propagation and eligibility can determine whether a relevant occurrence reaches the NPC strongly enough to qualify for a response. Consumer logic supplies what that response means.

Wilds coordinates the resulting work and history.

### A fishing lure

Wilds does not need a built-in concept of fishing.

A lure can participate as a consumer-owned runtime object. Relationships, context, propagated influence, and eligibility can determine whether another participant qualifies to respond. The consumer decides whether that means investigation, avoidance, pursuit, indifference, or something Wilds has never been designed to name.

### A cranky miner at a poker table

Wilds does not need to know what a miner is, what crankiness means, or how poker is played.

Those meanings belong to the consumer.

Wilds can provide identities, relationships, context, state, propagation, eligibility, independent execution, and historical outcomes around the encounter. The consumer's vocabulary determines what the interaction means.

The goal is not an NPC with infinitely more hard-coded reactions.

The goal is an NPC that can **participate in situations whose meaning exists outside the engine**.

## Why the questions stay separate

The separation between these questions is not accidental complexity. It prevents one answer from silently becoming another.

- Being related does not mean being affected.
- Being affected does not mean being eligible to respond.
- Being eligible does not prescribe a behavior.
- Responding does not make the response part of the originating Pulse.
- Completing processing does not imply success.
- A result does not disappear merely because execution has ended.
- Consumer meaning does not need to become engine vocabulary.

This lets Wilds model coordination, relationships, consequences, causality, and history while remaining deliberately ignorant of the consumer's specific world.

## QSL companion integration

**Wilds is asking:** _Who owns the request lifecycle, and where does QSL computation belong inside it?_

Wilds is the exposed consumer surface. QSL is computation delegated by Wilds during an active Wilds request. Consumers do not need to treat QSL as the simulation host, and `WildsQslExecutor` is integration machinery rather than the conceptual front door.

The certified request-envelope route is:

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
   +--> bind exact consumer-owned objects
   +--> receive attached runtime state
   `--> evaluate QSL source
            |
            v
       QSL result
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

That order matters. Wilds creates the Pulse and WorkUnit before QSL evaluates anything. QSL performs delegated computation while the WorkUnit is processing. The QSL result returns to Wilds, and Wilds then completes the WorkUnit, finalizes the Pulse into a Moment, and records that Moment into Timeline and Epoch history.

`WildsQslRequest` is the integration request envelope currently used to carry:

- QSL source;
- exact `QslConsumerBinding` instances;
- `QslStatePackage` instances containing approved attached runtime vocabulary.

The envelope is carried through Wilds as opaque request data. Wilds Core does not need to understand `QslConsumerBinding`, `QslStatePackage`, parser details, evaluator details, or consumer gameplay types.

The responsibility split is:

- **Consumer:** owns game-specific meaning, objects, rules, and vocabulary.
- **Wilds:** owns the request lifecycle, work coordination, relationships, propagation mechanics, causal boundaries, and history.
- **QSL:** parses and evaluates the computation Wilds delegates to it.
- **WildsEngine.Qsl:** bridges Wilds-owned work to QSL without moving QSL concepts into Wilds Core.

QSL processes the valid work it is given. Propagation is a separate Wilds concern. A request may be processed locally through QSL without implying that its outcome must propagate to another participant.

The canonical integration proof is the Space Dust expression:

```qsl
player.Health = player.Health - (action.Amount * (action.SpaceDustAppetite / 100))
```

In certification, `player.Health` begins at `50`, native consumer state supplies `action.Amount = 10`, attached runtime state supplies `action.SpaceDustAppetite = 87`, and QSL mutates the exact consumer-owned Player to `41.3`. Wilds then resolves that result into its Moment/Timeline/Epoch history.

## Certification boundary

**Wilds is asking:** _Which guarantees belong to the engine itself, and which depend on consumer-provided meaning?_

The current certification is intentionally layered.

The Wilds/QSL core statement suite remains certified at **156/156**, with zero certification failures and zero missing dependencies. It continues to exercise the source-only QSL route as a regression gate.

The request-envelope certification separately proves that:

- `WildsQslRequest` completes through the Wilds lifecycle;
- native consumer state and attached runtime vocabulary compose in one QSL expression;
- `QslConsumerBinding` retains the exact consumer-owned runtime objects;
- `QslStatePackage` targets the exact consumer-owned object;
- the Space Dust expression mutates `Player.Health` from `50` to `41.3` on that exact Player instance;
- the terminal result is preserved through Moment, Timeline, and Epoch history.

A dedicated Wilds/QSL end-to-end test additionally protects the architectural sequence: Wilds owns the active request lifecycle, QSL performs delegated work inside that lifecycle, and only after QSL returns does Wilds complete the WorkUnit and resolve the terminal Moment.

The full Release Wilds suite currently passes **50/50**. One unrelated nullable warning remains in `NeighborTraversal.cs` (`CS8604`).

The previously extracted consumer-dependent corpus should no longer be treated as evidence that QSL itself must become the consumer-facing simulation host. The certified Wilds request-envelope route now demonstrates the missing integration behavior at the intended boundary: consumer-owned objects and attached vocabulary enter through Wilds, QSL computes against them, and Wilds owns resolution and history.

Runtime outcome and certification outcome remain intentionally different concepts. A certification test can correctly succeed because Wilds produced the failure or cancellation required by the contract. Certification asks whether the engine accurately represented what happened.

## Consumer-facing API direction

**Wilds is asking:** _How can consumers provide meaning while Wilds remains the exposed surface?_

The ownership decision is now explicit: **Wilds is the front door.** `WildsQslExecutor` and QSL remain behind that boundary as integration and computation machinery.

The currently certified `WildsQslRequest` proves the request-envelope mechanics, including exact consumer bindings and attached state. It should not be interpreted as requiring consumers to construct or invoke the executor directly. Certification may instantiate integration machinery in order to prove the boundary, but that is not the intended product-facing usage model.

The exposed surface should preserve these rules:

- consumers submit work to Wilds;
- Wilds owns Pulse and WorkUnit creation and lifecycle transitions;
- QSL-backed work is delegated only after Wilds has established that lifecycle;
- QSL evaluates the source and runtime context it is given;
- QSL results return to Wilds for WorkUnit completion and Pulse finalization;
- propagation remains independent of whether local QSL computation occurs;
- consumer vocabulary remains consumer-owned rather than becoming Wilds Core vocabulary.

This keeps the art gallery, fishing lure, cranky miner, Player, Action, and future consumer types outside Wilds Core while still allowing their exact runtime objects and vocabulary to participate in certified processing.

## Architectural rule

**Wilds is the exposed surface and owns coordination, the request lifecycle, relationships, propagation mechanics, causal execution boundaries, and history. QSL owns delegated computation. Consumers own meaning.**

Or, stated as the thesis of the engine:

**Wilds does not need to understand the world in order to let the world interact.**
