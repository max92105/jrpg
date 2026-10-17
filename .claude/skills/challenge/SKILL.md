---
name: challenge
description: Get a skeptical second opinion on a plan, design idea, or decision.
argument-hint: "<plan file path, or the idea/decision to challenge>"
disable-model-invocation: true
---

Delegate to the **devils-advocate** subagent to challenge: $ARGUMENTS

If no argument was given, challenge the most recent decision or approach discussed in this conversation; summarize it in 3–5 lines for the subagent, since it can't see our conversation.

Relay its findings to me ranked (Blockers, Concerns, max 3 Nitpicks) and its verdict. Then give me your own honest take in 2–3 sentences: where you agree with it, and where you think it's wrong. Don't change anything until I decide.
