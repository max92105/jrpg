---
name: review-slice
description: Independent fresh-context review of the current uncommitted changes against this project's rules.
context: fork
agent: code-reviewer
background: false
disable-model-invocation: true
---

Review the current uncommitted changes in this repository.

Current working tree:
!`git status --short`

Extra focus requested by the developer (may be empty): $ARGUMENTS

Follow your review process and output format exactly. Be strict about architecture boundaries and test coverage.
