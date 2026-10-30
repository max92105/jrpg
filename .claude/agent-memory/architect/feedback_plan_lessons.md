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
- Loot retro (2026-10-29):
  - a method that takes `ContentCatalog` lives in Content or Session, never in a feature folder Content references (S1a's planned `GearPiece.FromSaved` and B1's `DropRoll.For` would each have made a cycle);
  - when a model change gets ahead of the save format, grep the tests that save and restore (`StartMapScenarios.StartWithWholeParty`), then plan a stopgap or put the save change in the same slice (G1b-1 needed a stopgap the plan said wasn't needed);
  - before budgeting seeded-test fixes, grep which repository tests win a battle and then draw from the random or read the inventory (D2 and R3b budgeted fixes and needed none).
- Forge review (2026-10-29):
  - "can it be done" rules return a Core value (cost/yield + refusal reason enum) the screen only formats; never a Game-side check;
  - a content check that needs another file (rules vs items) is called from `ContentCatalogLoader` after both load, not from the file's own validator;
  - an event step opening a screen: `EventPresenter` once flag -> `ExplorationScene` returns the request before its busy check -> explicit `GameFlow.Start` cases (the default branch fades and throws) -> a `...Transitions` that pops and calls `ContinueEvent`; test files over 300 get a sibling file;
  - order so a hand check exists early (bare screen, step, editor, NPC, then the list/look/actions);
  - the 1.6x applies to Game slices that add a view type (loot README rule), still forcing screen splits: shell / list / detail / look / actions / each action.
- Parallel drafts (2026-10-29): when several plans are drafted at once, don't touch `docs/plans/README.md`; number questions Q1.. locally and list decisions as "proposed, numbered at approval"; ADR candidates without a number.
- Enemy-skills review (2026-10-29): never put a test or boss encounter in a start-area map's random table (ADR 0017: a lone level 1 leader must win there); park it in `encounters.json` unrolled, pinned by a "no map rolls it" repository test (decisions 226, 399), hand checks edit the map locally and revert. A rule that must see every HP loss goes at one place, the end of `BattleState.Execute`, not per path in `TurnFlow`. A new playback cue gets its own beat (as `AddSkipCue`), routed through `Read`'s default arm. A boss's size comes from its art, no code flag. Check which other in-flight plans target the same near-300 test file.
- Before planning a greyed-entry guard test, check `ListCursor`: it skips disabled entries, so the test asserts the cursor never selects it, not "Confirm does nothing".
- My Write tool writes LF; repo docs are CRLF: tell the caller to run `sed -i 's/\r*$/\r/'` on new plan files.
- Affix-stats review (2026-10-29):
  - a new enum member read by a "for every kind" loop (`StatComparison` over `StatSheet.ValueOf`) needs its switch arm in the same slice; check the default arm throws;
  - a weighted-pick boundary test must draw where the change moves the edge (a draw on an unchanged first weight can't fail);
  - when applying a review, the developer pre-assigns decision numbers and edits `docs/plans/README.md` himself; my own picks stay unnumbered "planner's picks";
  - the Write tool writes LF even over CRLF files and I have no shell: name the files needing `sed -i 's/\r*$/\r/'` in the report.
- State with no reader yet (a list written, never read) fails the build (IDE0052): land the field with its first reader's slice, not earlier (quests QL/SV).
- Quests review (2026-10-29): parallel plans collide on the save format number and on hot files (`ExplorationState` 299, `EventRun.ApplyAndCheckWait` CA1502, `EventStepViewModel`): read the sibling plans' slices touching them and state run order; ordered progress needs a "reached (at or past)" gate; "first event that may run wins" needs a content rule refusing an ungated event that shadows later ones; a cell holds one event (gating it only switches it off); a placeholder wired early beats an empty page; editor `Events` reads `Catalogs`, so `Catalogs` can't use `Events` types.
- A public Core record's method taking an internal type (`QuestLog`) must itself be internal; check accessibility when a public type gains a Core-state parameter.
- Before giving a presenter a new view or dependency, count its constructor (EventPresenter, ExplorationScene, EventRun are at 4) and GameFlow.Start's CA1502 budget (each `Session ?? throw` is a branch): plan the parameter object or the helper in the slice, not mid-slice.
- `docs/ui-reference` has only `shop-sell.webp` for shops (no buy shot), whatever a caller says.
- Game traps found planning audio (2026-10-29): most scenes and pages are already at 4 ctor params (BattleScene, ForgeScene, GameMenuScene, Inventory/Party/Status/System pages), so a cross-cutting output goes out as a value the flow collects (default interface member), not a new ctor dependency; `PlayerSettings` isn't persisted (question 89), so check "saved like the others" requests; `FinishBattle` runs when the outcome screen opens, so "until the battle screen closes" must read the stack, not `PendingEncounter`.
- Music review (2026-10-29): files the code itself plays are code constants checked by one existence test (decision 84, `UiArtFiles`), not a content table with an id list; a per-update output channel is cleared first and filled right after `_scenes.Update`, before a request is applied; an ADR also says where a future content-driven version would live (Core state + save, never a scene). The developer's "every X has a Y" means every kind, exhaustive (default arm throws), with cross-plan cases noted.
- Affix stats retro (2026-10-29): when a slice's rule names a case it doesn't apply to (items, heals, enemies, reserve members), the slice's test list names that case's test (reviews added one in A2, A3 and A4b); a layout decision that narrows a column measures the real text at the new width (a probe test with the real fonts), not only the heights (ST1a's two-line descriptions became decision 373 mid-slice).
