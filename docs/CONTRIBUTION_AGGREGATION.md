# Provenance-Preserving Contribution Aggregation

**Status:** Implemented CPU-side grouping policy with automated tests. This is not geometry, volume, particle, pixel, or GPU aggregation.

## Contract

`RenderContributionAggregator.Aggregate` accepts the contributions already selected as visible for one consumer. Every contribution contains:

- an authoritative `SourceId`;
- the exact `SourceRevision` used for this observation;
- a caller-defined `AggregationKey`;
- a declared source representation error;
- hazard and interaction-critical flags.

The aggregation key is a compatibility assertion from the caller. It must encode all domain-specific constraints needed to make contributions safe to combine for that consumer: phenomenon/type, region or spatial partition, units, coordinate frame, temporal window, and any other semantic constraints. Renderer does not infer that two contributions are physically or semantically compatible.

A source may occur only once in an input snapshot. Repeated source IDs are rejected even when revisions differ, rather than silently mixing multiple versions of one authoritative source.

## Deterministic grouping and safeguards

The aggregator:

1. Orders contributions by compatibility key and then ordinal source ID.
2. Separates hazard-bearing and interaction-critical contributions; those always remain individual.
3. Groups only ordinary contributions sharing the same compatibility key.
4. Applies an explicit minimum group size and a maximum aggregate-error budget.
5. Splits a group when its next member would exceed that budget.
6. Preserves every source individually when it cannot safely form a group, including a source whose own declared error already exceeds the aggregate budget.
7. Retains every `(SourceId, SourceRevision)` pair in the result, enabling a caller to promote back to the exact contributing sources.

For a group of (n) sources, the current conservative estimate is:

[
E_{total} = sum_{i=1}^{n} E_{source,i} + (n-1)E_{additional}
]

`additionalSourceError` is an explicit policy input, not a calibrated physical error model. The group is emitted only when its total estimate is within `maximumAggregateError`. Single-source results are preserved as-is and are not rejected merely because their existing source error exceeds the aggregation budget.

## Example

```csharp
var result = RenderContributionAggregator.Aggregate(
    visibleContributions,
    maximumAggregateError: 0.08,
    additionalSourceError: 0.01,
    minimumSources: 2);

foreach (var aggregate in result.Aggregates)
{
    // aggregate.Sources preserves every authoritative source and revision.
    // aggregate.EstimatedTotalError is the declared conservative estimate.
    // aggregate.IsAggregated distinguishes a real multi-source group.
}
```

## Evidence

`RenderContributionAggregatorTests` covers:

- deterministic source ordering and revision preservation;
- hazard and interaction-critical source isolation;
- compatibility-key isolation;
- error-budget splitting without dropping sources;
- preservation of individually over-budget contributions;
- duplicate source rejection;
- minimum group size and individual remainder handling;
- invalid budget rejection.

This is a deterministic CPU policy proof. It does not combine actual visual data, establish a GPU savings figure, or validate a domain's compatibility key. Those require a consumer-specific adapter and measured integration.

## Promotion and disaggregation

The aggregate is not assigned a synthetic authoritative source identity. Its `Sources` list is the complete membership/provenance record. A higher-detail consumer can request the listed source revisions from their authoritative owners and construct individual representations. Availability and reconstruction correctness remain the responsibility of those owners and the host; this class does not fetch or persist source state.
