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
- Game/Editor slices: split each screen into a pure presenter (tested in Jrpg.Game.Tests / Jrpg.Editor.Tests) and a thin draw part with a manual checklist; build a Game folder-dependency table too (scenes return requests to a flow, or Title->Exploration->Battle->Title cycles).
- Order Game slices so shared pieces (fade, panels, text) arrive with their first real caller; note any intermediate "freeze" gap in the slice. Keep transition handlers <= 4 ctor params via a SceneFactory, not a bag.
- Game UI: developer wants the legacy WPF look ported (Theme.xaml tokens, layouts), not invented; Esc = Cancel+Menu needs an explicit "Cancel wins when nested" rule; known-gap lines when a slice leaves a step unhandled (exhaustive switch throws).
- Flow requests: a scene must return a transition request only once (GameFlow's fade throws on a second Start); headless flow tests must be seed-independent since GameFlow seeds from the clock (and its ctor is at 4 params).
- Put each Core change in (or right before) the slice that first uses it, not in an up-front "Core seams" batch (battle-plan review C-1).
- Tests needing a battle outcome use a test catalog; developer: balance isn't done, plans implement and test systems. Pixel art is point-scaled by whole factors, only painted art is smoothed.
- Cleanup plans: grep earlier plans' "Done, with deviations"/follow-up lines and the code-reviewer memory first; they hold the diagnosis (e.g. font crash cause) and the exact follow-up wording.
- Developer wants UI built from many small reusable Ui/ components (card, meter, title); put cross-screen parts in Ui/, not Menus/, so Battle can reuse them.
- Test seams: a small internal read accessor (CardOf, ShownStats) beats a vacuous behavior test; drop tests that only exercise a library (MeasuredText.Of).
- Refactor slices: "existing tests pass unchanged" is the guard; draw-only moves get before/after screenshots, not invented tests.
- Packages are never assumed: font package, MonoGame content pipeline and CommunityToolkit.Mvvm are open questions until the developer approves.

**Why:** part 1 split slices mid-way; the part 2 devil's advocate (2026-10-17) raised exactly these as blockers/concerns.

**How to apply:** check every plan against this list before handing it back. See [[migration-roadmap]].
