# CI/CD Strategy

## Goals

- keep the codebase buildable and testable
- secure the release pipeline
- support Windows development and packaging

## Recommended flow

### Continuous integration
- build on every pull request
- restore packages
- run unit and integration tests
- run static analysis
- ensure no secrets are committed

### Continuous delivery
- build MSIX packages for Windows release
- validate installers in a staging environment
- sign packages where required
- deploy official release artifacts to the project website

## Security gates

- prevent plaintext secrets in commits
- scan dependencies for known vulnerabilities
- validate package provenance
- require code review for release branches

## Suggested pipeline stages

1. restore
2. build
3. test
4. security scan
5. package
6. release validation

## Packaging

- MSIX for Microsoft Store submission
- standalone installer package for direct website download
