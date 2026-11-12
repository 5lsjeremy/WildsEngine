# Wilds Entity E7 Evidence Contribution Policies

This drop provides reusable consumer-facing policies for deciding whether validated Evidence contributes toward a Fact.

## Core rule

`ProofResult` reports independent properties:

- `Believability`
- `IsTrue`

No universal relationship is imposed between them. A believable claim may be false, and a true claim may be unbelievable.

`FactBase` should enforce infrastructure invariants before invoking a policy:

1. Evidence is non-null.
2. Evidence belongs to the exact Fact Entity.
3. Evidence is validated.
4. `ProofResult.Evidence` is the exact Evidence instance.

The policy then decides whether that Evidence contributes to the Fact.

## Included policies

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

### Composition and escape hatches

- `AnyEvidencePolicy`
- `NotEvidencePolicy`
- `AllEvidencePolicies`
- `AnyEvidencePolicies`
- `DelegateEvidencePolicy`

## Social-deduction example

```csharp
var convincingLie =
    new BelievableFalseEvidencePolicy(
        0.80);
```

This policy accepts Evidence when:

```text
Believability >= 0.80
IsTrue == false
```

That supports mistaken beliefs, deception, rumor, bluffing, unreliable witnesses, and social deduction without embedding those meanings in Wilds.

## Composite example

```csharp
var policy =
    new AllEvidencePolicies(
        new IEvidenceContributionPolicy[]
        {
            new MinimumBelievabilityPolicy(0.70),
            new ConditionEqualsPolicy(
                "Target.Flying",
                true),
            new OutcomeEqualsPolicy(
                "Success")
        });
```

## Integration

Add `IEvidenceContributionPolicy` to `IFact`, pass it into `FactBase`, and invoke it only after validation and exact ProofResult provenance have been confirmed.
