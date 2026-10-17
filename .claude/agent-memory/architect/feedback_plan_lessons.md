---
name: plan-lessons
description: Planning rules from the developer and the devil's-advocate reviews (slice size, ADR placement, placeholders, failable tests, folder cycles, no-caller APIs, render-only content)
metadata:
  type: feedback
---

When planning slices in this repo:
- Split Core logic and content/Data loading into separate slices up front whenever one would exceed ~300 lines; reviewers check sizes (part 2 review split 1c, 4b, 5b).
- ADRs are checkbox steps right before the slices that depend on them.
- Port mechanisms, keep values in content, mark undesigned rules as placeholders; tunables get a named table (name, where, placeholder value, meaning).
- Only list tests that can fail (FixedSequenceRandom, pinned values); simulations must not claim checks their bound can't detect.
- Build an explicit folder-dependency table and check every edge; put snapshots/records in the lower folder (PartyMemberProgress in Party/, not Saving/).
- Cut or defer anything with no caller in the plan: interfaces with one implementation (ISaveStore), UI projections (SaveSlotSummary, prompts), public members only part 3 needs, render-only content fields.
- Converters: handle only inputs the data actually has; absent cases are errors naming the file; inexpressible content is skipped into a report. Converter/legacy code lives in a tool project, not in Data.
- Per-frame code: no allocation; bounding-box before polygon tests. Fixed technical limits become named Core constants.

**Why:** part 1 split slices mid-way; the part 2 devil's advocate (2026-10-17) raised exactly these as blockers/concerns.

**How to apply:** check every plan against this list before handing it back. See [[migration-roadmap]].
