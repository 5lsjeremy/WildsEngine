# Wilds Entity E7 abstraction drop

This package contains interfaces only. Concrete and base implementations are intentionally deferred to the next slice.

## Dependency direction

```text
IDiscovery
    -> IFact[]
        -> IDiscoveryEvidence[]
            -> IProofResult
                -> IProof
```

There are no upward collections. A Discovery can trace its complete support chain downward through Facts, Evidence, Proof results, and source processing units.

## Native surface

`IEntityNativeSurface` exposes the attributes observed on the exact consumer-owned target when the Entity surface is created. `INativeAttribute.InitialValue` is a creation-time snapshot, not a competing authority for live consumer state.

## Threshold model

`IFact` and `IDiscovery` both extend `IResolutionThreshold<TContributor>`.

- A Fact resolves from valid Evidence.
- A Discovery resolves from proven Facts.
- Current counts and percentages are derived from authoritative collections.
- No independent mutable counter is introduced.

The concrete implementations should map the generic members as follows:

```text
IFact.Contributors       == valid Evidence collection
IFact.RequiredCount      == RequiredEvidenceCount
IFact.CurrentCount       == CurrentEvidenceCount
IFact.Percentage         == EvidencePercentage
IFact.IsResolved         == IsProven

IDiscovery.Contributors  == Facts collection
IDiscovery.RequiredCount == RequiredFactCount
IDiscovery.CurrentCount  == CurrentFactCount
IDiscovery.Percentage    == FactPercentage
IDiscovery.IsResolved    == IsAvailable
```

## Domain objects and Wilds projections

Facts, Evidence, Proofs, and Discoveries remain consumer-facing domain objects. They may carry external IDs and names, but Wilds treats that metadata as opaque.

`IWildsProjection<TSource>` is the promotion boundary. A domain object receives Wilds identity only when it must independently participate in topology, emission, propagation, sampling, temporal history, or other Wilds processing.

Included projections:

- `IWildsFact`
- `IWildsDiscovery`
- `IWildsEvidence`

Do not instantiate a projection merely because a domain object exists.

## Integration note

The package assumes the repository already provides:

- `IEntity`
- `IBaseObject`
- `IBaseUnit`

Review namespaces if those contracts have moved before dropping the files into the current project.
