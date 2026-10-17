---
name: type-folder-name-clash
description: Naming rule used in plans - never name a type the same as its feature folder/namespace (CA1724, warnings are errors)
metadata:
  type: feedback
---

Don't name a type after its own feature folder (e.g. `Party/Party.cs`, `Battle/Battle.cs`, `Inventory/Inventory.cs`). Use `PartyState`, `BattleState`, or put `Inventory` under `Items/`.

**Why:** namespace = folder is enforced (IDE0130), `latest-recommended` analyzers plus TreatWarningsAsErrors make CA1724 and ambiguous references fail the build, and CLAUDE.md itself suggests `PartyState`/`BattleState` names.

**How to apply:** check every new type name against its folder when choosing names in a plan. See [[migration-roadmap]].
