E7-E - FACT RESOLUTION

Uses the current FactBase behavior supplied in conversation:
- ResolutionThresholdBase-derived current counts are computed from current Evidence.
- FactBase delegates valid-contributor interpretation to ContributionPolicy after validation/provenance checks.
- Evidence membership and contributor count remain distinct.
- Current percentage is capped at 1.0.
- IsProven/IsResolved derive from CurrentCount >= RequiredCount.

Requires existing fixtures:
- TestEntity
- TestProof
- TestFact

Assertion cases: 10
Readable cases: 3

Drop tests into your E7-E certification folder and Invoke-E7E.ps1 beside Invoke-E7Gate.ps1.
