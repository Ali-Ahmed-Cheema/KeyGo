# Risk Register

## High-risk items

- API key leakage
- unsafe autonomous tool execution
- provider compatibility mismatches
- cost overruns from model misuse
- data exposure through project context selection
- unsupported or undocumented provider behavior

## Medium-risk items

- streaming failures or partial responses
- permission boundary mistakes
- file-system access issues
- SQLite schema drift during early iterations
- inconsistent model capability reporting

## Low-risk items

- UI churn during early phases
- package dependency sprawl
- mismatched naming conventions

## Mitigations

- secure credential storage
- explicit permission checks
- provider verification and compatibility reporting
- budget checks and usage monitoring
- project data minimization before cloud calls
- ADR-driven architecture changes
- early, focused test coverage
