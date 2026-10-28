---
name: plan-lessons
description: Planning rules from the developer and the devil's-advocate reviews (slice size, ADR placement, placeholders, failable tests, folder cycles, no-caller APIs, render-only content)
metadata:
  type: feedback
---

When planning slices in this repo:
- Split Core logic and content/Data loading into separate slices up front whenever one would exceed ~300 lines; reviewers check sizes (part 2 review split 1c, 4b, 5b).
- Estimate includes call-site and fixture churn: grep the constructor or method being changed and count the test sites (the new-game flow split C1b, about 27 test sites, and A1a, shared helpers).
- Content that uses a new event step lands in the same slice as the editor support for it, or after: the editor throws on a step it can't open (A1a's split).
- ADRs are checkbox steps right before the slices that depend on them.
- Port mechanisms, keep values in content, mark undesigned rules as placeholders; tunables get a named table (name, where, placeholder value, meaning).
- Only list tests that can fail (FixedSequenceRandom, pinned values); simulations must not claim checks their bound can't detect.
- Build an explicit folder-dependency table and check every edge; put snapshots/records in the lower folder (PartyMemberProgress in Party/, not Saving/).
- Cut or defer anything with no caller in the plan: interfaces with one implementation (ISaveStore), UI projections (SaveSlotSummary, prompts), public members only part 3 needs, render-only content fields.
- Converters: handle only inputs the data actually has; absent cases are errors naming the file; inexpressible content is skipped into a report. Converter code lives in a tool project, not in Data.
- Per-frame code: no allocation; bounding-box before polygon tests. Fixed technical limits become named Core constants.
- Game/Editor slices: split each screen into a pure presenter (tested in Jrpg.Game.Tests / Jrpg.Editor.Tests) and a thin draw part with a manual checklist; build a Game folder-dependency table too (scenes return requests to a flow, or Title->Exploration->Battle->Title cycles).
- Order Game slices so shared pieces (fade, panels, text) arrive with their first real caller; note any intermediate "freeze" gap in the slice. Keep transition handlers <= 4 ctor params via a SceneFactory, not a bag.
- Game UI: Esc = Cancel+Menu needs an explicit "Cancel wins when nested" rule; known-gap lines when a slice leaves a step unhandled (exhaustive switch throws).
- Flow requests: a scene must return a transition request only once (GameFlow's fade throws on a second Start); headless flow tests must be seed-independent since GameFlow seeds from the clock (and its ctor is at 4 params).
- Put each Core change in (or right before) the slice that first uses it, not in an up-front "Core seams" batch (battle-plan review C-1).
- Tests needing a battle outcome use a test catalog; developer: balance isn't done, plans implement and test systems. Pixel art is point-scaled by whole factors, only painted art is smoothed.
- Cleanup plans: start from `docs/plans/backlog.md`, then grep earlier plans' "Done, with deviations"/follow-up lines and the code-reviewer memory; they hold the diagnosis (e.g. font crash cause) and the exact follow-up wording.
- When a slice splits drawing out of a page, estimate what stays by counting its input, sub-state and command methods, not by subtracting the drawing (menu cleanup: EquipmentPage planned ~170, ended 215; PartyPage ~180, ended 250).
- Developer wants UI built from many small reusable Ui/ components (card, meter, title); put cross-screen parts in Ui/, not Menus/, so Battle can reuse them.
- Test seams: a small internal read accessor (CardOf, ShownStats) beats a vacuous behavior test; drop tests that only exercise a library (MeasuredText.Of).
- Refactor slices: "existing tests pass unchanged" is the guard; draw-only moves get before/after screenshots, not invented tests.
- Editor dialogs follow `StartX()` returns a view model / `TryApplyX(vm)` on the owning view model, with `XProblem` properties and `ToX()` throwing on a problem; plan new windows that way. WPF dark themes: ComboBox/ScrollBar need templates (size risk).
- Reviews want splits planned up front (writer vs first user, behavior vs look, layout vs theme), not "split if it grows"; view models take lookups (image sizes) instead of reading files; one responsibility per panel VM.
- Developer wants Ctrl+Z to cover every edit (map and catalog alike); expect "undo everything" in editor features.
- Packages are never assumed: font package, MonoGame content pipeline and CommunityToolkit.Mvvm are open questions until the developer approves.
- Code-health plans:
  - check each backlog line against the code;
  - put a bug slice before the refactor of the same file;
  - keep it lean: only the slices the next plans touch, and the rest go to backlog "When next touched" with a reason (the 2026-10-23 review cut 26 slices to 13);
  - keep fixes at bug size, with no new type just to test window code;
  - before planning a failing-first test, check that it can actually fail (F3 couldn't);
  - test splits allow a mechanical prefix edit, guarded by test counts before and after, with no permanent forwarders;
  - verify the direction of a decision before citing it (144 was cited backwards).
- Prefer a Core record field (ExperienceGain.IsAtMaxLevel) over handing a catalog to a scene.
- UI feedback: look at the input bindings first (the inventory rail had no keyboard key); read docs/ui-reference (the .webp files open with Read).
- Removal/wording plans: count with Grep count mode (it counts matching lines, not occurrences), slice by folder, list each reason-carrying comment with its replacement, "don't invent a reason"; a text guard must skip itself and history paths (docs/plans, done ADRs).

- Before changing repository content (classes, members, new-game), grep the Game/Data tests that assume it (Sarxian as warrior leader, Teelle, StartNew call sites) and budget the fixes in the slice.
- Devil's-advocate sizing: count ~13 lines per planned test plus broken existing tests; split Core state from content+Game up front. Fold implementation choices (timing, refresh rules) into ADRs, keep questions for real design; number decisions/questions contiguously from the free number.
- I have no delete tool: when re-planning, reuse slice file names where possible and overwrite orphans with a "Superseded: delete this file" stub, then tell the caller which to delete.
- Battle mechanics: a new BattleEvent's Core slice may land without Game support only if no content produces it yet (ActionPlayback throws on unknown events); the content lands with the playback case. Check mechanics are observable with repository enemies (goblin hits were 3 % of HP: heavy-hit threshold untestable) and plan a practice enemy when fights are too short.

**Why:** part 1 split slices mid-way; the part 2 devil's advocate (2026-10-17) raised exactly these as blockers/concerns.

**How to apply:** check every plan against this list before handing it back. See [[migration-roadmap]].
- Game UI facts that shape plans: `SceneStack` draws every scene, so a closing menu fades over the map; verify fails a changed file crossing 300 lines, so list files at 280+ and split before the slice that grows them.
- The export of the reference game's UI is huge: plan slices by named subfolder, never "scan it all".
- UI plans (developer, 2026-10-25): "coherent with itself everywhere" (one selection look, a rule every look slice follows); take the vibe, not one-for-one (kept the diamond, not the hand); everything was too small (plan a sizing pass early); plan a capture key first so look slices are specced from real screens; extend an interface with `Advance(delta)` rather than changing `Update`'s signature (170 test sites).
- A Game look slice that adds a view type costs the view (~120 lines) plus its wiring in every caller plus the tests it replaces: LK1 (est. 240, real ~400) and LK2 (220, ~375) in the class-mechanics plan each came out 1.6x their estimate and were split at the slice step; estimate them at that rate, or split them up front.
- When a new panel takes screen space an existing one uses, settle which one stays during planning (question 265 came up mid-slice and split M2b in three).
- Big model swaps (stats plan): add new record fields as init props with neutral defaults to avoid ctor churn; keep `Stats` a leaf (factories take numbers, not PartyMember/items); random rolls draw only when 0<chance<100 so seeded fixtures survive; re-pin test expectations in a tests-only mechanical slice before the removal slice; Game `BattleScenarios` (65 uses) needs a certain-chance copy of content.
- Loot review (2026-10-27): split a big plan into "playable loop" + follow-up plans; put new fields on a model from birth (no add-later slice); no interim save bridges (save slice right after the swap); keep Battle free of loot (Session returns spoils beside rewards); saves store ids+values and resolve kinds from content; make every implicit behavior a numbered decision or question.
- Model swaps with heavy test churn (loot plan): keep the most-called signature and convert inside (`Inventory.Add` makes plain pieces), swap in two steps (worn, then owned) with a named interim wrap, and land the save shape once, before the slice that makes the new data appear in play.
- Stats-plan review (2026-10-26): prove a dense page fits with written pixel arithmetic in the slice; hold percents in permille with one `long` round-down; record-struct init defaults are a trap (`default` = 0), use sealed records; a status that skips turns (Stun) must not gain duration from scaling; fold removed stats into test setups instead of re-pinning; every Game slice states raw size and the 1.6x.
