E7-D CERTIFICATION PACKAGE

Uses only concrete information supplied in conversation and README-Policies.md.

Production policies exercised:
- TrueEvidencePolicy
- FalseEvidencePolicy
- MinimumBelievabilityPolicy
- BelievableFalseEvidencePolicy
- AllEvidencePolicies
- ConditionEqualsPolicy
- OutcomeEqualsPolicy

Test doubles are limited to the IEvidenceContributionPolicy / FactBase integration seam.

Drop:
- Fixtures/*.cs into Entities/Fixtures
- E7D/Cert Tests/*.cs into your E7-D certification folder
- Runners/Invoke-E7D.ps1 beside Invoke-E7Gate.ps1

Expected current count:
- 13 assertion executions (12 facts plus one theory-style count is not used)
- 4 readable tests

Run:
.\Entities\Runners\Invoke-E7D.ps1
