# File Transactions

Agent edits are represented as `FileChangeProposal` objects. A proposal contains the intended goal, each relative path, original content, and proposed content.

The workflow is:

1. Generate proposal.
2. Create a unified review diff.
3. Obtain explicit approval.
4. Check write permission.
5. Validate every path against the project root.
6. Confirm the on-disk content still matches the proposal snapshot.
7. Write through a temporary file and replace the target.
8. Restore captured originals if a later write fails.

Unapproved, stale, or out-of-root proposals are rejected. The current implementation does not use destructive Git reset for rollback.
