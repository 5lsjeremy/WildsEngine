# Wilds Core Getting Started

Wilds Core is the domain-neutral simulation substrate. It coordinates consumer-owned work without learning what the consumer's objects mean.

## Temporal model

`Pulse → WorkUnit → Moment → Timeline → Epoch`

- **Pulse** owns one coordinated request and its required WorkUnits.
- **WorkUnit** records required work belonging to that Pulse.
- **Moment** is the terminal historical outcome: completed, failed, or cancelled.
- **Timeline** collects committed Moments.
- **Epoch** collects Timelines.

A Pulse cannot honestly resolve as successful while required work is unresolved, failed, or cancelled.

## Consumer-owned objects

Wilds can carry arbitrary consumer runtime references through processing and history. Those objects retain their identity and meaning; Wilds does not convert them into domain-specific Core types.

## Topology

Neighbor relationships are directional. Membership establishes neighborhood; distance expresses proximity within that relationship. The consumer owns what the relationship and distance mean.

## Emissions and propagation

Wilds provides generic machinery for resolving consumer-owned values into emission magnitudes, composing topology-derived context, propagating through relationships, and evaluating eligibility. The meaning of every magnitude and channel remains consumer-owned.

## QSL

QSL-backed computation belongs in the companion integration layer rather than Core. Core does not require QSL semantics to process ordinary Wilds work.

## Where Core stops

Core supplies simulation infrastructure. It does not own Discoveries epistemology, consumer-domain semantics, or mandatory agency.
