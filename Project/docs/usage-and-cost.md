# Usage and Cost

## Implemented

- `IUsageTracker` abstraction and `UsageSummary` record
- `UsageRecord` data model for persisted usage metrics
- Basic request-level usage tracking and session totals at the service layer

## Experimental

- Cost estimation for model requests based on pricing metadata
- Mapping provider-reported usage to normalized output values

## Planned

- Full budget controls, daily/monthly limits, per-request thresholds, and confirm-before-send enforcement
- Accurate model pricing cache and normalized cost calculations
- UI panel for session usage and budget status
