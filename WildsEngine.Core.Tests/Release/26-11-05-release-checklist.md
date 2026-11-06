# WildsEngine Release Checklist

## Gate 1: Existing Foundation

- [x] Engine instances are isolated.
- [x] Initialization is idempotent.
- [x] Shutdown is idempotent.
- [x] Use before initialization is rejected.
- [x] Use after disposal is rejected.
- [x] Instruction processor leasing is bounded.
- [x] Processor instances return after success, failure, and cancellation.
- [x] Registry reference identity is preserved.
- [x] Duplicate registry entries are rejected.
- [x] Task registry is certified through 1,000,000 entries.
- [x] Pulse lifecycle order is enforced.
- [x] WorkUnit lifecycle order is enforced.
- [x] Foreign and duplicate WorkUnits are rejected.
- [x] A Pulse cannot resolve before all required work terminates.
- [x] Completed, failed, and cancelled outcomes become honest Moments.
- [x] Unresolved work cannot produce a Moment.
- [x] Timeline preserves sequential Moment order.
- [x] Epoch preserves Timeline identity.
- [x] Arbitrary consumer-owned object identity survives the temporal route.
- [x] Neighbor topology is state-driven.
- [x] Neighbor relationships are directional and membership-specific.
- [x] Neighbor operators and pipelines are deterministic and source-safe.
- [x] Neighbor propagation mathematics and eligibility are certified.
- [x] A topology-selected component survives Wilds execution and history unchanged.
- [x] Wilds/QSL request-envelope integration is certified.
- [x] Native and attached runtime vocabulary compose correctly.
- [x] Exact consumer-owned objects are mutated through Wilds/QSL.
- [x] Parser and runtime diagnostics survive failed Wilds history.
- [x] QSL core suite passes 156/156.

---

## Gate 2: Temporal Concurrency

- [x] Concurrent `ExecuteAsync` requests all complete.
- [x] Every request produces exactly one Moment.
- [x] No Moment is lost.
- [x] No Moment is recorded twice.
- [x] Every Moment has a unique Pulse identity.
- [x] Every single-work execution has one unique WorkUnit identity.
- [x] Every result remains associated with its exact request object.
- [x] Timeline count equals the submitted request count.
- [x] Timeline contains every returned Moment by reference.
- [x] Epoch continues to own the engine Timeline.
- [x] Epoch exposes the same complete Moment set as Timeline.
- [x] No concurrent request produces a false failed or cancelled outcome.
- [x] Concurrent Timeline ordering semantics are explicitly chosen and separately certified.

> **Release blocker:** Concurrent temporal integrity is certified. Timeline ordering semantics remain to be explicitly defined and certified.
---

## Gate 3: Terminal-State Races

- [x] Concurrent complete/fail/cancel attempts against one WorkUnit produce exactly one winner.
- [x] The winning status agrees with `Result` and `Exception`.
- [x] A successful WorkUnit cannot carry an exception.
- [x] A failed WorkUnit cannot carry a successful result.
- [x] A cancelled WorkUnit carries neither result nor exception.
- [x] Repeated terminal transitions are rejected.
- [x] Pulse finalization observes the authoritative WorkUnit outcome.
- [x] Mixed WorkUnit result precedence is independent of completion order.
---

## Gate 4: Active Lifecycle Interruption

- [x] Cancellation during active `ExecuteAsync` work produces a cancelled Moment.
- [x] Active processing cancellation returns rented processors and removes transient work.
- [x] Cancellation does not record a successful result.
- [x] Shutdown during active execution has deterministic behavior.
- [x] Disposal during active execution has deterministic behavior.
- [x] No active request is silently lost during shutdown.
- [x] The engine can recover after a cancelled operation when recovery is contractual.

---

## Gate 5: Multi-Work Decomposition

- [x] One public request can own multiple required WorkUnits.
- [x] Every WorkUnit retains the same Pulse identity.
- [x] Successful completion produces one authoritative Moment.
- [x] The Moment contains every required WorkUnit identity.
- [x] Partial completion cannot resolve the Pulse.
- [x] Partial completion followed by failure produces a failed Moment.
- [x] Partial completion followed by cancellation produces the intended terminal outcome.
- [x] No subordinate result leaks out as the final result unless explicitly selected.

---

## Gate 6: Wilds/QSL Runtime Isolation

- [x] Two Wilds engines maintain independent histories.
- [x] Two QSL runtimes maintain independent consumer bindings.
- [x] Identical aliases can target different objects in isolated runtimes.
- [x] State packages do not cross runtime boundaries.
- [x] Diagnostics/failures do not contaminate another runtime.
- [x] Clearing one runtime does not affect another.
- [x] Removing one consumer does not remove similarly named consumers elsewhere.

---

## Gate 7: Package Lifecycle

- [x] Multiple consumers can be bound in one request.
- [x] Multiple state packages can target distinct consumers.
- [x] Attached properties resolve only against their exact target.
- [x] Package application does not contaminate another consumer.
- [x] Duplicate target/property behavior is explicitly defined.
- [x] Conflicting packages are deterministically rejected rather than overwriting accepted state.
- [x] Removing one consumer invalidates only that binding and retires attached state belonging to that exact consumer.
- [x] Clear removes all attached runtime vocabulary.
- [x] Clear does not damage native consumer-owned properties.
- [x] A resolved zero-value removal package removes watched attached state, if that behavior is implemented.
- [x] Removal of attached state cannot remove a native property accidentally.

---

## Gate 8: Extreme-Input Safety

- [x] Propagation rejects `NaN`.
- [x] Propagation rejects positive and negative infinity.
- [x] Propagation rejects negative distance where prohibited.
- [x] Propagation rejects negative decay where prohibited.
- [x] Propagation rejects negative source intensity where prohibited.
- [x] Extreme finite inputs do not silently produce non-finite output.
- [x] Equal-distance neighbor ordering is deterministic.
- [x] Threshold comparisons remain inclusive at the exact boundary.

---

## Gate 9: Release Reproducibility

- [ ] Entire Wilds suite passes in Debug.
- [ ] Entire Wilds suite passes in Release.
- [ ] Wilds/QSL core passes 156/156 in Release.
- [ ] No missing certification dependencies.
- [ ] Build warnings are captured separately.
- [ ] Known warnings are documented and intentionally accepted.
- [ ] Certification output records commit SHA.
- [ ] Certification output records build configuration.
- [ ] Certification output records target frameworks.
- [ ] Certification output records runtime and architecture.
- [ ] Certification output records UTC timestamp.
- [ ] Certification output lists every expected gate.
- [ ] Every expected gate reports pass, fail, skipped, or missing.
- [ ] Parallel log records include a test or correlation identity.
- [ ] Release artifacts are generated only after every blocking gate passes.