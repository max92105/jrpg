---
name: review-slice
description: Independent fresh-context review of the current uncommitted changes against this project's rules. Run only when the user explicitly asks for /review-slice.
context: fork
agent: code-reviewer
background: false
---

Review the current uncommitted changes in this repository.

Current working tree:
!`git status --short`

Extra focus requested by the developer (may be empty): $ARGUMENTS

Follow your review process and output format exactly, including the re-review rules: be strict on Criticals, and make each round's list shorter than the last.
