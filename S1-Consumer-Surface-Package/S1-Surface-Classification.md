# S1A Consumer Surface Classification

Based on the current S1 crawl:

- **Options:** runtime/machinery configuration. S1 introduces `WildsDiscoveriesOptions`; no consumer semantics are placed there.
- **Definitions:** `IDiscoveryClusterMapping`, mapping documents, vector inputs/contributions, consumer-owned geometric semantic keys.
- **Policies:** existing `IEvidenceContributionPolicy`, `ITruthCongealmentPolicy`, and `IConsequenceProjectionPolicy`, plus their current concrete Evidence policy compositions. Policies remain decision contracts and are not converted into options.
- **Services:** `IWildsService`, vectorization service, motion service, and cluster mapping import service. Service contexts/journals remain infrastructure behind the surface, except the execution journal is exposed read-only through its interface for provenance queries.
- **Profiles:** S1 introduces a strongly typed `IDiscoveryGeometryProfile` composition for geometry definitions.
- **Registrations:** S1 introduces immutable `IDiscoveryGeometryRegistration` snapshots with profile identity/version/scope and mapping IDs.
- **Results:** existing vectorization, motion, momentum, spatial relationship, trajectory projection, Proof, Truth, and Consequence contracts remain result/domain surfaces.
- **Internal machinery:** concrete registries, service context factories, execution context objects, registration catalogs, JSON parser, and the concrete geometry surface are hidden behind `WildsDiscoveries`.

S1 does not alter Fact/Discovery resolution, Truth semantics, Consequence semantics, or Core.
