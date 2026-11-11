E7-C - PROOF SEMANTICS

Grounded against the current E7 crawl:
- ProofBase.Validate returns ProofResult with exact Evidence and Proof.
- Believability is validated to finite [0,1].
- ProofResult independently carries Believability and IsTrue.
- DiscoveryEvidence.ApplyProof delegates to the one-result EvidenceBase lifecycle.
- EvidenceBase rejects a ProofResult belonging to different Evidence and refuses silent replacement.

Cert tests:
- 4 quadrant theory rows plus provenance, attachment, replacement, null, range, NaN/infinity, and constructor defenses.

Readable stories:
- Tom's believable lie
- Tom's unbelievable truth because Jerry heard North Cove was closed for kelp removal
- all four truth/believability quadrants
- exact immutable proof attachment

Runner filters:
FullyQualifiedName~EntityProofCertificationTests
FullyQualifiedName~EntityProofReadableTests
