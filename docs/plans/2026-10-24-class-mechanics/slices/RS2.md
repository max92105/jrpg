# RS2. The editor authors the rest step; a placeholder inn

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Tunables and placeholders" (Inn). Decision 198; question 222. ADR 0012, ADR 0015.

- **RS2. The editor authors the rest step; a placeholder inn on the start map** (Editor + content, about 160 lines)
  - **Goal:**
    - The event dialog offers "Rest" as a step kind with no fields, undoable like any step edit.
    - The start map gets a placeholder inn: an "Innkeeper" NPC (the shared placeholder sprite, ADR 0015) whose Interact event says one line, then rests the party. It's free (question 222).
  - **Changes:**
    - Editor: `Events/EventStepViewModel` and `Events/EventPresets` know `RestStep` (label "Rest").
    - Content: `content/maps/start.json`, edited in the editor and saved with Ctrl+S: the Innkeeper and its event.
  - **Tests:**
    - `EventStepViewModelTests`:
      - `FromStep_RestStep_ShowsRestWithNoFields`
      - `ToStep_Rest_ReturnsRestStep`
    - `EventEditingViewModelTests`: `AddStep_Rest_UndoRemovesIt`
    - `ContentCatalogLoaderTests`: `Load_StartMap_HasARestEvent` (a `RestStep` exists on the start map, not a count)
  - **Verify by hand:**
    - The editor opens the start map, and the Innkeeper's event shows "Rest".
    - In the game, wear Omer's clarity down, talk to the Innkeeper, fight: mana starts at 3, and HP is full.
- **Done, with deviations:**
  - The Innkeeper went into `content/map-layouts/start.json` by hand rather than through the editor: Claude can't drive the editor. The new object matches the format the editor writes (a `villager` at (30, 26), the end of the recruit row, with a talk event of one line, then `rest`).
  - `EventPresets` is unchanged: no preset needs a rest step, and the step kind, label and Add button cover authoring. The "Rest" button sits in both the cell and the object event dialogs.
  - Extra tests: `RestStep` joins the every-kind round trip, and `StartMapVillagerTests.Interact_FacingInnkeeper_ReadsALineThenRests`, which uses a new `StartMapScenarios.BelowInnkeeper`.
