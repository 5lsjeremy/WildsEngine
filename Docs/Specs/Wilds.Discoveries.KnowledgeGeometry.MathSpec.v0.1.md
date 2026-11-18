# Wilds.Discoveries Knowledge Geometry Mathematics Specification

**Version:** 0.1  
**Status:** Working Mathematical Specification  
**Scope:** Discovery Geometry through Consequence Projection

---

## Purpose

This document defines the mathematical invariants for Wilds.Discoveries knowledge geometry and the default/correction mathematical profile used when certification or a neutral implementation requires a concrete strategy.

It does **not** assign consumer meaning to dimensions.

It does **not** establish one universal mathematical model for all consumers.

Consumer-defined mathematical policies may replace the default strategies provided they preserve the structural, provenance, numeric, and epistemic invariants defined here.

---

# 1. Fundamental Model

Wilds.Discoveries treats developing knowledge as geometry rather than reducing all knowledge development to binary resolution.

The canonical progression relevant to geometry is:

**Facts -> Discoveries -> Vector Contributions -> Vector Points -> Ordered Vector Series -> Motion / Momentum -> Clusters -> Scoped Truth -> Consequences**

Fact resolution and Discovery resolution remain epistemic processes.

Geometric development is related to resolution but is not identical to it.

An unresolved Discovery may have meaningful geometric position and trajectory without being promoted to resolved knowledge.

---

# 2. Mathematical Objects

## 2.1 Contribution

A Discovery-vector contribution is:

$$c_i = (d_i, v_i, w_i, p_i)$$

where:

- $d_i$ is the consumer-defined dimension;
- $v_i$ is the contribution value;
- $w_i \ge 0$ is its weight;
- $p_i$ is its preserved provenance through the authoritative vector input and optional Fact.

The weight must be finite and nonnegative.

The value must be finite.

A contribution may identify a Fact, but that Fact must legitimately belong to the authoritative Discovery's epistemic chain.

Evidence is not a direct geometric contributor.

---

# 3. Vector Point

At an evaluation time $t$, a Discovery occupies a point:

$$\mathbf{x}_t = [x_{1,t}, x_{2,t}, \ldots, x_{n,t}]^T$$

Each coordinate corresponds to a consumer-defined named dimension.

The point has:

- coordinates;
- dimensionality.

The point does **not** independently own:

- identity;
- time;
- motion;
- semantic meaning;
- independent provenance.

Those belong to the surrounding evaluation structures.

---

# 4. Contribution Composition

There is intentionally no universal Wilds equation mapping contributions to coordinates.

For a dimension $d$, let the relevant contributions be:

$$C_d = \{(v_i,w_i)\}$$

The coordinate is:

$$x_d = F_d(C_d)$$

where $F_d$ is consumer-authorized composition behavior.

Permitted strategies include, but are not limited to:

## 4.1 Weighted Sum

$$x_d = \sum_i w_i v_i$$

## 4.2 Weighted Mean

$$x_d = \frac{\sum_i w_i v_i}{\sum_i w_i}$$

when the denominator is nonzero.

## 4.3 Nonlinear Composition

$$x_d = f\left(\sum_i w_i v_i\right)$$

where $f$ may be a bounded or unbounded consumer-selected curve.

## 4.4 Matrix Transformation

Let the raw contribution state be $\mathbf{r}$.

A consumer may define:

$$\mathbf{x} = A\mathbf{r} + \mathbf{b}$$

This allows dimensions to interact rather than requiring independent scalar reductions.

More generally:

$$\mathbf{x}=F(\mathbf{r},\theta)$$

where $F$ is the supplied transformation and $\theta$ represents its consumer-owned parameters.

Wilds validates the resulting numeric structure.

Wilds does not infer what $A$, $\mathbf{b}$, $\theta$, or the named dimensions mean.

---

# 5. Default / Correction Composition Profile

The formulas in this section are **not universal Wilds semantics**.

They define the deterministic correction profile used when:

- certification requires concrete mathematics;
- no consumer-specific mathematical strategy is under test;
- a neutral implementation needs an explicit fallback;
- the operation can be resolved without inventing domain meaning.

## 5.1 Single Contribution

For one contribution into dimension $d$:

$$x_d = wv$$

This gives both Value and Weight observable mathematical effect without assigning semantic meaning to either.

## 5.2 Multiple Contributions

For multiple contributions into the same dimension, use a weighted mean:

$$x_d = \frac{\sum_i w_i v_i}{\sum_i w_i}$$

provided:

$$\sum_i w_i > 0$$

If all contributing weights are zero, the coordinate remains unresolved and processing must fail rather than invent a value.

A supplied consumer policy may replace this strategy.

---

# 6. Magnitude and Metric

Magnitude belongs to the supplied vector metric.

For a point:

$$\mathbf{x} = [x_1,\ldots,x_n]^T$$

the metric defines:

$$m = M(\mathbf{x})$$

Wilds does not universally require Euclidean geometry.

The standard correction/certification metric is Euclidean magnitude:

$$\|\mathbf{x}\|_2 = \sqrt{\sum_{i=1}^{n}x_i^2}$$

Consumer metrics may instead implement other norms, weighted metrics, transformed spaces, or other valid geometric models.

The resulting magnitude must be finite and nonnegative.

---

# 7. Ordered Evaluations

Discovery geometry is evaluated through immutable snapshots.

Let:

$$\mathbf{x}_0,\mathbf{x}_1,\ldots,\mathbf{x}_t$$

represent an ordered series of evaluations.

Historical points are not rewritten when new information arrives.

The ordered series provides the basis for temporal knowledge geometry.

---

# 8. Motion

Position and motion are distinct.

Given consecutive evaluations $\mathbf{x}_{t-1}$ and $\mathbf{x}_t$, separated by:

$$\Delta t=t_t-t_{t-1}$$

the default velocity estimate is:

$$\mathbf{v}_t = \frac{\mathbf{x}_t-\mathbf{x}_{t-1}}{\Delta t}$$

for $\Delta t>0$.

Given a previous velocity $\mathbf{v}_{t-1}$, the default acceleration estimate is:

$$\mathbf{a}_t = \frac{\mathbf{v}_t-\mathbf{v}_{t-1}}{\Delta t}$$

A first isolated vector has position but insufficient historical information to claim observed velocity or acceleration.

Therefore absent history produces **no motion**, not an invented zero-motion observation.

---

# 9. Curves

Successive evaluations need not be interpreted as piecewise-static state.

The ordered series may be modeled as a curve:

$$\mathbf{x}(t)$$

with:

$$\mathbf{v}(t)=\frac{d\mathbf{x}}{dt}$$

and:

$$\mathbf{a}(t)=\frac{d^2\mathbf{x}}{dt^2}$$

The architecture permits nonlinear interpolation and smoothing strategies.

Previous mathematical work explicitly included linear interpolation, Bezier curves, Hermite interpolation, and Catmull-Rom interpolation.

No one curve family is universal Wilds canon.

The default/correction profile begins with direct finite differences because they preserve observed samples without inventing a fitted trajectory.

Curve fitting becomes an explicitly supplied mathematical strategy.

---

# 10. Momentum

Knowledge momentum represents sustained directional development rather than binary resolution.

In the minimal profile, momentum is represented by the vector's observed temporal motion together with the strength and persistence of that motion across the ordered series.

It must not be inferred from one isolated point.

A consumer may define a richer momentum function:

$$\mathbf{p}_t = G(\mathbf{x}_{0:t},\mathbf{v}_{0:t},\mathbf{a}_{0:t})$$

The implementation may incorporate:

- persistence;
- weighting;
- decay;
- damping;
- curvature;
- confidence;
- sample density;
- other consumer-authorized terms.

No universal momentum equation is locked at H1.

---

# 11. Directional Similarity

When comparison between vectors is required, geometric similarity may be evaluated independently from magnitude.

A permitted strategy is cosine similarity:

$$\cos(\theta) = \frac{\mathbf{a}\cdot\mathbf{b}}{\|\mathbf{a}\|\|\mathbf{b}\|}$$

This distinguishes:

- moving in a similar direction;
- moving in opposing directions;
- being geometrically orthogonal;

without requiring equivalent magnitudes.

Cosine similarity is an available mathematical strategy, not mandatory universal Discoveries semantics.

---

# 12. Distance

A geometric distance function is supplied by the chosen metric or strategy.

The standard certification profile may use Euclidean distance:

$$d(\mathbf{a},\mathbf{b}) = \sqrt{\sum_i(a_i-b_i)^2}$$

Consumers remain free to supply another legitimate distance model.

---

# 13. Clustering

A collection of vectors may form zero, one, or multiple coherent structures.

Clustering must not manufacture consensus merely because multiple vectors were supplied.

For members:

$$\mathbf{x}_1,\ldots,\mathbf{x}_N$$

the standard certification centroid is:

$$\boldsymbol{\mu} = \frac{1}{N}\sum_{i=1}^{N}\mathbf{x}_i$$

The existing architecture expects clusters to expose:

- members;
- centroid;
- density;
- dispersion;
- coherence.

The centroid is a geometric point, not an independently identifiable vector.

Multiple competing clusters are legitimate.

Outliers are legitimate.

Divergence is legitimate.

---

# 14. Dispersion

For default certification, dispersion may be measured as mean distance from the centroid:

$$D = \frac{1}{N}\sum_{i=1}^{N}d(\mathbf{x}_i,\boldsymbol{\mu})$$

A consumer may substitute another dispersion statistic or metric appropriate to its geometry.

---

# 15. Density and Coherence

Density and coherence are normalized cluster characteristics exposed in the current contracts.

Their final production formulas are **not currently canonical**.

Until explicit formulas are certified:

- consumers or clustering policies own their calculation;
- outputs must satisfy their established numeric bounds;
- Wilds must not silently infer them through undocumented equations.

The certification suite may use transparent fixture formulas, but fixture formulas do not become universal platform semantics.

---

# 16. Convergence and Divergence

The series and cluster systems must permit detection of geometric development toward or away from a structure.

Conceptually, convergence occurs when relevant distances and/or dispersion decrease through ordered evaluations.

Divergence occurs when they increase.

A consumer may evaluate this through:

- distance derivatives;
- dispersion derivatives;
- velocity orientation;
- angular similarity;
- covariance;
- fitted curves;
- another explicit mathematical strategy.

No simple threshold is universally canonical.

---

# 17. Matrix-Capable Geometry

Discovery geometry shall not assume dimensions are independent.

Consumer strategies may use matrices to:

- rotate knowledge space;
- project into another basis;
- scale dimensions;
- represent relationships among dimensions;
- transform raw contribution space into geometric space.

A general linear transform is:

$$\mathbf{y}=A\mathbf{x}$$

and an affine transformation is:

$$\mathbf{y}=A\mathbf{x}+\mathbf{b}$$

More sophisticated nonlinear transforms remain permitted behind explicit mathematical policy boundaries.

Matrix operations must preserve dimensional validity and finite numeric results.

---

# 18. Shape Recognition

Clusters and trajectories may form recognizable geometric structures.

Shape recognition is therefore allowed to consider combinations of:

- centroid;
- relative position;
- distance;
- density;
- dispersion;
- coherence;
- orientation;
- angular similarity;
- velocity;
- acceleration;
- curvature;
- persistence;
- convergence/divergence;
- multiple competing clusters.

Recognition does not itself manufacture Truth.

Recognized geometry becomes input to consumer-authorized congealment.

---

# 19. Truth Congealment

A scoped Truth represents sufficiently stable composition of Discoveries and their geometry.

Congealment may therefore evaluate a function of the form:

$$T = H(\text{series},\text{clusters},\text{stability},\text{confidence},\text{provenance})$$

The consumer-authorized congealment policy determines the acceptance criterion.

Wilds preserves the exact series, clusters, and authoritative Discoveries that produced the Truth.

There is no universal Wilds equation declaring Truth.

---

# 20. Consequence Projection

Consequences are projections from established knowledge, not actions.

A consequence projection may consume the resulting Truth and its retained geometric composition.

Conceptually:

$$\mathbf{c} = P(T,\mathcal{G},\theta)$$

where:

- $T$ is scoped Truth;
- $\mathcal{G}$ represents preserved geometry;
- $\theta$ is consumer-owned policy or state.

The projection may use scalar, vector, matrix, nonlinear, probabilistic, or other consumer-authorized mathematics.

Wilds does not supply hidden domain semantics.

---

# 21. Correction Hierarchy

When implementation encounters an undefined mathematical operation, use this order.

## 21.1 Existing Certified Invariant

Existing Wilds.Discoveries contracts and certified behavior win.

Do not replace them for convenience.

## 21.2 Explicit Consumer Policy

If the consumer supplied the mathematical strategy, use it.

## 21.3 Existing Geometric Policy Boundary

If a metric, vectorization, clustering, congealment, or consequence policy owns the decision, defer to that boundary.

## 21.4 Default / Correction Profile

When certification requires deterministic mathematics and no consumer strategy exists, use:

- single contribution: $x=wv$;
- multiple contributions: weighted mean;
- magnitude: Euclidean norm;
- distance: Euclidean distance;
- velocity: first finite difference;
- acceleration: second finite difference through velocity;
- centroid: arithmetic mean;
- no fabricated first-sample motion.

## 21.5 Explicit Unresolved / Failure

If applying the correction profile would require inventing semantic meaning or invalid mathematics, processing fails unresolved.

Wilds does not guess.

---

# 22. Numerical Invariants

All geometric processing must preserve:

- finite coordinate values;
- finite contribution values;
- nonnegative contribution weights;
- unique component dimensions;
- contribution/component dimension alignment;
- finite velocity;
- finite acceleration;
- no duplicate motion record per dimension;
- finite, nonnegative magnitude;
- valid provenance and exact ownership.

When a mathematical operation produces `NaN`, infinity, dimensional inconsistency, division by zero, or otherwise undefined output, the evaluation fails.

Invalid mathematics must never be converted into semantic certainty.

---

# 23. Provenance Invariants

Mathematical transformation does not erase epistemic history.

Forward contribution remains:

**Evidence -> Fact -> Discovery -> Geometry -> Cluster -> Scoped Truth -> Consequence**

Within geometry:

**Discovery -> Vector Input -> Contribution -> Component -> Vector -> Series -> Cluster**

Backward provenance remains available through those legitimate structures.

Higher-order processing must not bypass the immediate epistemic contributor simply because deeper provenance remains inspectable.

---

# 24. Canonical Principle

**Resolution tells us what the entity can presently resolve.**

**Geometry tells us where that knowledge is located.**

**Motion tells us how that knowledge is changing.**

**Curves tell us the shape of that change.**

**Clusters tell us what structures are forming.**

**Congealment tells us when those structures are sufficiently stable to constitute scoped Truth.**

**Consequence projection tells us what that developed knowledge says may result.**

At every stage:

**The consumer owns meaning. Wilds owns validated mathematical machinery and attributable processing.**

---

## Version 0.1 Boundary

The following are landed architectural principles:

- vector geometry rather than binary-only knowledge development;
- position separated from temporal motion;
- immutable ordered evaluations;
- injected metrics;
- consumer-owned vectorization semantics;
- consumer-extensible mathematical composition;
- nonlinear curves as legitimate strategy;
- matrix-capable transforms as legitimate strategy;
- clustering of geometric structures;
- centroid represented as a point;
- competing geometric structures remain legitimate;
- scoped Truth congeals from preserved geometry and provenance;
- Consequence projection is the current terminal knowledge boundary.

The following belong to the default/correction profile rather than universal Wilds semantics:

- weighted contribution composition;
- weighted-mean multi-contribution reduction;
- Euclidean magnitude;
- Euclidean distance;
- finite-difference velocity;
- finite-difference acceleration;
- arithmetic centroid;
- mean-distance dispersion.

These correction formulas may be replaced by explicit consumer-supplied mathematical policies without changing the intrinsic Wilds.Discoveries knowledge model.
