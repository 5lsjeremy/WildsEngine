E7-B - EVIDENCE INTEGRITY CERTIFICATION

Drop the two files in Cert Tests into your chosen E7-B certification folder.

Expected classes:
- EntityEvidenceCertificationTests
- EntityEvidenceReadableTests

Suggested runner wrapper:

& $Runner `
    -Gate "E7-B" `
    -Title "Evidence Integrity" `
    -AssertionFilter "FullyQualifiedName~EntityEvidenceCertificationTests" `
    -ReadableFilter "FullyQualifiedName~EntityEvidenceReadableTests"

This gate intentionally does not certify ProofResult semantics. Those belong to E7-C.
