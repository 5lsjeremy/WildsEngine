# Wilds Entity E7

## Native State, Evidence, Proof, Facts, Belief, Discoveries, and Wilds Projection

E7 defines the boundary between consumer-facing knowledge and Wilds processing identity.

The guiding rule is simple:

> Consumer-facing domain objects describe the world. Wilds projections allow those exact objects to participate in Wilds processing without replacing or reinterpreting them.

A second rule is equally important:

> Truth and believability are independent observations. Wilds does not decide which one a Fact should require.

This distinction allows the same infrastructure to represent objective knowledge, mistaken belief, convincing falsehoods, weakly believed truths, rumor, suspicion, deception, and other consumer-defined interpretations without teaching Wilds what any of those concepts mean.

## Knowledge pipeline

The Entity knowledge model has a one-way dependency chain:

```text
Entity
  |
  +-- Facts
  |
  +-- Discoveries
        |
        +-- Facts
              |
              +-- Evidence
                    |
                    +-- ProofResult
                          |
                          +-- Proof
```

There are no upward collections from Evidence to Fact or from Fact to Discovery. A Discovery can therefore trace its support downward without creating circular ownership.

## Native attribute capture

`IEntityNativeSurface` records what an Entity saw on the exact consumer-owned target when the surface is created.

`INativeAttribute` captures:

- attribute name;
- value type;
- creation-time value;
- readability;
- writability.

The captured value is historical. It is not an alternative authority for the live consumer-owned object.

The current native surface captures readable public instance properties and public instance fields. Indexer properties are skipped.

```text
consumer-owned object
        |
        v
EntityNativeSurface
        |
        +-- NativeAttribute
        +-- NativeAttribute
        +-- NativeAttribute
```

This establishes the baseline vocabulary the Entity arrived with.

## Evidence

`IDiscoveryEvidence` represents one consumer-facing observation gathered from the world.

Evidence may carry consumer-provided metadata such as `ExternalId` and `Name`. Wilds assigns no meaning to those values.

Evidence also preserves:

- the exact Entity;
- conditions;
- outcome;
- raw strength;
- normalized strength;
- neighbor references;
- an optional source unit;
- validation state;
- the resulting ProofResult.

Conditions and neighbor collections are snapshotted when Evidence is created so later mutation of the caller's collection does not rewrite the observation.

Evidence does not know which Fact will consume it.

## Proof

`IProof` validates one exact Evidence item.

A Proof does not establish a Fact directly and does not know which Discovery may eventually consume that Fact.

`IProofResult` carries two independent consumer-facing conclusions:

- `Believability`, a normalized number from `0.0` through `1.0`;
- `IsTrue`, the Proof's truth conclusion for the Evidence.

These properties deliberately separate observation magnitude from epistemic judgment:

```text
Evidence.Strength
    what was observed

Evidence.NormalizedStrength
    normalized observation magnitude

ProofResult.Believability
    how believable the Proof says the observation is

ProofResult.IsTrue
    whether the Proof resolves the observation as true
```

No invariant links `Believability` to `IsTrue`.

All four combinations are meaningful:

```text
high Believability + true
high Believability + false
low Believability  + true
low Believability  + false
```

A highly believable claim may still be false. A true claim may remain difficult to believe.

Wilds does not impose a universal Believability threshold and does not declare that truth is the universal requirement for Fact resolution.

A ProofResult must reference the exact Evidence instance it validates.

## Evidence contribution policy

`IEvidenceContributionPolicy` determines whether one validated Evidence item contributes toward a particular Fact.

This is the key separation between proof and belief:

```text
Evidence
    |
    v
Proof
    |
    +-- Believability
    +-- IsTrue
    |
    v
EvidenceContributionPolicy
    |
    v
Does this Evidence support this Fact?
```

`FactBase` remains responsible for infrastructure invariants before policy evaluation:

1. the Evidence exists;
2. the Evidence belongs to the exact Fact Entity;
3. the Evidence has been validated;
4. the ProofResult references the exact Evidence instance.

Only after those checks does the consumer-supplied contribution policy decide whether the Evidence counts toward that Fact.

The policy can use any consumer-visible Evidence or ProofResult information it needs, including:

- `IsTrue`;
- `Believability`;
- raw or normalized strength;
- conditions;
- outcome;
- source;
- neighbors;
- combinations of other policies.

Wilds owns none of those interpretations.

### Example interpretations

An objective-truth Fact may count only Evidence whose Proof resolves true.

A belief-oriented Fact may count Evidence above a Believability threshold regardless of objective truth.

A deception-oriented Fact may specifically count highly believable Evidence that is false.

A skeptical Fact may require both truth and high Believability.

A Fact representing a dismissed warning could count Evidence that is true but remains poorly believed.

These are consumer policies, not built-in meanings.

## Rumor, deception, and mistaken belief

Because Believability and truth are independent, E7 can represent rumors and social deduction without introducing a special Wilds concept called `Rumor`.

For example:

```text
claim:
    Blue was seen leaving Electrical

ProofResult:
    Believability = 0.91
    IsTrue        = false
```

A truth-oriented Fact can reject this Evidence.

A belief-oriented Fact can accept the exact same Evidence because it was convincing enough to the participant represented by that Fact.

A deception-oriented Fact can explicitly care that the Evidence was both convincing and false.

The same Evidence can therefore support different Facts for different Entities or different consumer policies without changing the Evidence or ProofResult.

This allows consumers to model concepts such as:

- rumors;
- lies;
- bluffing;
- mistaken inference;
- unreliable witnesses;
- suspicion;
- persuasion;
- misinformation;
- secrets that are true but unbelievable;
- competing interpretations of the same event.

Wilds does not label any Evidence as a rumor, lie, suspicion, or belief. The consumer constructs those meanings by selecting contribution policy.

## Facts

`IFact` is a consumer-facing assertion supported by Evidence.

A Fact owns its Evidence collection but has no reference to the Discoveries that may consume it.

A Fact also owns or receives an `IEvidenceContributionPolicy` describing what counts as support for that Fact.

Fact resolution is threshold-based:

```text
policy-accepted Evidence count
             /
required Evidence count
             |
             +-- CurrentEvidenceCount
             +-- EvidencePercentage
             +-- IsProven
```

The count, percentage, and `IsProven` state are derived from the authoritative Evidence collection plus the Fact's contribution policy. They are never independently incremented or toggled.

`IsProven` therefore means that the Fact has satisfied its configured evidence standard. It does not universally mean that Wilds has declared the Fact objectively true.

That distinction is intentional.

## Reusable contribution policies

E7 supports reusable policy primitives rather than baking domain meaning into `FactBase`.

Representative policy types include:

### Truth and deception

- `TrueEvidencePolicy`
- `FalseEvidencePolicy`
- `BelievableTrueEvidencePolicy`
- `BelievableFalseEvidencePolicy`
- `TrueButUnbelievableEvidencePolicy`

### Believability

- `MinimumBelievabilityPolicy`
- `MaximumBelievabilityPolicy`
- `BelievabilityRangePolicy`

### Observation magnitude

- `MinimumStrengthPolicy`
- `MinimumNormalizedStrengthPolicy`

### Evidence content

- `ConditionExistsPolicy`
- `ConditionEqualsPolicy`
- `OutcomeEqualsPolicy`
- `HasSourcePolicy`
- `MinimumNeighborCountPolicy`

### Composition

- `AllEvidencePolicies`
- `AnyEvidencePolicies`
- `NotEvidencePolicy`
- `DelegateEvidencePolicy`
- `AnyEvidencePolicy`

Policy composition allows a designer to create complex standards without adding new semantics to Wilds Core.

For example:

```text
Believability >= 0.70
AND
Target.Flying == true
AND
Outcome == Success
```

can be expressed as a composition of reusable policies.

## Discoveries

`IDiscovery` represents knowledge made available by enough proven Facts.

A Discovery owns Facts belonging to the same exact Entity.

A Fact contributes to Discovery resolution when `Fact.IsProven` is true.

Discovery resolution mirrors Fact resolution:

```text
proven Fact count
       /
required Fact count
       |
       +-- CurrentFactCount
       +-- FactPercentage
       +-- IsAvailable
```

The count, percentage, and availability state are derived. There is no separate `MakeAvailable()` lifecycle.

Because Facts can use different Evidence contribution policies, two Discoveries can emerge from different standards of belief while using the same underlying Evidence and Proof machinery.

## Entity ownership

`IEntity` is the stable aggregate for Entity knowledge.

It owns collections of:

- Facts;
- Discoveries.

Facts, Discoveries, and Evidence must belong to the exact Entity instance expected by their owning layer.

The Entity remains the stable identity anchor through `IIdentifier` and `IdentifierBase`.

Facts and Discoveries intentionally do not inherit `IdentifierBase`. They may carry consumer-facing `ExternalId` or descriptive names, but Wilds identity is not introduced until projection.

## Resolution abstraction

Facts and Discoveries share the same mechanical threshold model through `IResolutionThreshold<TContributor>` and `ResolutionThresholdBase<TContributor>`.

The reusable model provides:

- contributors;
- required count;
- current valid count;
- normalized completion percentage;
- resolved state.

Each domain layer defines what makes a contributor valid:

```text
Fact
  contributor = Evidence
  infrastructure requirements:
      validated
      exact Entity ownership
      exact ProofResult provenance
  semantic contribution:
      IEvidenceContributionPolicy

Discovery
  contributor = Fact
  valid when  = IsProven
```

The threshold abstraction owns no consumer meaning.

## Wilds projection

Consumer-facing objects remain domain objects until they need to participate as ordinary Wilds processing objects.

`IWildsProjection<TSource>` is the common promotion boundary.

Projection adds:

- Wilds `IBaseObject` identity;
- component ownership;
- layer/runtime semantics;
- access to the exact authoritative source object.

Projection does not copy or reinterpret source meaning.

The E7 projection family is symmetrical:

```text
DOMAIN                    WILDS PROJECTION

IEntity                 -> IWildsEntity
IDiscoveryEvidence      -> IWildsEvidence
IFact                   -> IWildsFact
IDiscovery              -> IWildsDiscovery
```

The base implementations share `WildsProjectionBase<TSource>`:

```text
WildsProjectionBase<TSource>
    |
    +-- WildsEntityBase
    +-- WildsEvidenceBase
    +-- WildsFactBase
    +-- WildsDiscoveryBase
```

Concrete projections preserve the same arrangement:

```text
WildsEntity
WildsEvidence
WildsFact
WildsDiscovery
```

## Proof is intentionally not projected

Proof and ProofResult are currently validation policy and provenance, not independently processable Wilds objects.

There is therefore intentionally no `IWildsProofResult` or `WildsProofBase` in E7.

If a future processing requirement demonstrates that Proof or ProofResult needs independent Wilds identity, topology, propagation, or history, it can use the same projection model then.

## Emission compatibility

`EntityEmissionDataSource` exposes selected Entity-owned Facts through the existing `IEmissionDataSource` contract.

Facts remain authoritative. The data source reads the current Fact value when requested and rejects Facts belonging to another Entity.

This keeps Entity knowledge and emission processing separate.

## E7 invariants

E7 certification should preserve all of the following:

1. Native attribute capture never becomes authoritative live consumer state.
2. Evidence records observations and has no upward Fact or Discovery dependency.
3. Proof independently resolves Believability and truth without selecting a Fact or Discovery.
4. Believability does not imply truth, and truth does not imply Believability.
5. FactBase enforces validation, exact Entity ownership, and exact ProofResult provenance before contribution policy is evaluated.
6. Consumer-supplied `IEvidenceContributionPolicy` decides whether validated Evidence contributes to a Fact.
7. Wilds does not universally require `IsTrue` for Evidence to contribute.
8. Fact progress is derived from policy-accepted Evidence.
9. `Fact.IsProven` means the configured evidence standard has resolved, not that Wilds has declared objective truth.
10. Facts have no back-reference to consuming Discoveries.
11. Only proven Facts contribute to Discovery resolution.
12. Discovery availability is derived from proven Facts.
13. Entity ownership preserves exact Entity identity throughout the knowledge chain.
14. Consumer metadata remains opaque to Wilds.
15. Wilds identity is introduced by projection rather than by Fact or Discovery domain objects.
16. Projection preserves the exact authoritative source object.
17. Projection adds processing identity and runtime participation, not meaning.
18. Proof remains unprojected unless a real processing requirement emerges.
19. The same Evidence may contribute differently under different consumer policies.
20. Rumor, deception, suspicion, belief, and similar concepts remain consumer meanings composed from generic Evidence, Proof, and policy primitives.

## Certification target

The closing E7 certification path is:

```text
ordinary consumer-owned object
        |
        v
native attributes captured
        |
        v
world observations
        |
        v
Evidence
        |
        v
Proof
  Believability
  IsTrue
        |
        v
consumer Evidence contribution policy
        |
        v
policy-accepted Evidence threshold
        |
        v
Fact.IsProven
        |
        v
proven Fact threshold
        |
        v
Discovery.IsAvailable
        |
        v
optional IWildsProjection<TSource>
        |
        v
ordinary Wilds processing participation
```

E7 is successful when this route preserves exact ownership and provenance while keeping consumer vocabulary, belief standards, and meaning outside Wilds Core.
