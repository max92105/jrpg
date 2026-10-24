# K2b. The editor authors the arrival activation, the leader condition and the still step

Part of [the new-game flow plan](../README.md). Design: [design.md](../design.md), "Structure" (Editor). ADR 0016 (option B).

- **K2b. The editor authors the arrival activation, the leader condition and the still step** (Editor, about 200 lines; decision 185)
  - **Goal:** in the object and cell event dialogs:
    - "Arrive" is an activation, beside Interact and Touch;
    - "Only when leader" is an optional member picker, beside "only when object";
    - "Still" is a step kind with an image file, seconds and an optional caption.

    A map holding any of them opens, edits and saves without loss. This comes before K3 because the editor throws on step kinds it doesn't know, and K3's intro map uses them.
  - **Changes (Editor):**
    - `Events/EventEditingViewModel`:
      - the activation list gains Arrive;
      - an `OnlyWhenLeaderId` field with the members to pick from (the catalog the dialog already gets);
      - the prompt field is disabled for Arrive, matching Data's rule.
      - `EventViewModel`'s summary of an event (today `Press "{Prompt}"` for anything not Touch) reads "On arrival" for Arrive (K1 review).
    - `Events/EventStepViewModel`:
      - `EventStepKind.Still` with `ImageFile`, `SecondsText` and `Caption` fields;
      - `From(StillStep)` and `ToStep()` cases (seconds above 0, else invalid);
      - the label in the step list ("Still Placeholder1.png, 4 s").
    - The XAML for the new fields, shown for their kind, as the item picker is shown for Give item.
    - `Events/EventPresets`: none.
  - **Tests:**
    - `EventStepViewModelTests`:
      - `From_StillStep_KeepsImageSecondsAndCaption`
      - `ToStep_StillWithZeroSeconds_IsInvalid`
      - `ToStep_StillWithoutCaption_HasNullCaption`
    - `EventEditingViewModelTests`:
      - `Build_ArriveWithLeader_KeepsBoth`
      - `Prompt_ArriveActivation_IsDisabled`
  - **Verify by hand:**
    - After K3, open the `intro` map and open an intro event: it shows Arrive, the leader, and its stills.
    - Change a still's seconds, save, reopen: the change is kept.
    - Look at the dialog in the editor's dark theme.
  - **Done, with deviations:**
    - The cell dialog also offers Touch or Arrive, and both dialogs have "Only while led by", since K3's intros are cell events. `StepChoices` gains `MemberIds`.
    - Switching an event to Arrive disables the prompt box (`CanHavePrompt`), and `ToEvent` drops the prompt, so the game's "arrive has no prompt" rule can't be broken from the dialog. The label reads "On arrival", with " while {leader} leads" when the event waits for a leader (the K1 review's `Press ""`).
    - The still's seconds are typed as decimals with a dot ("2.5"); the editor checks above 0, and Data's upper bound applies on save.
    - No editor screenshot: the capture would take the whole screen while the developer works. The dialog check is the developer's.
    - Tests: still label, seconds problem (theory), blank caption, round trip of two stills, a cell event set to Arrive for Omer with a still, the cell's activation and leader choices, the arrival label, and the prompt dropped on Arrive.
    - Review: a test checks that the leader picker lists the catalog's members (`StartEditingCellAt_AnyCell_OffersTheCatalogsMembersAsLeaders`), and one test event id is now neutral. The map's event markers now have one color per trigger (cyan touch, gold press, orchid arrival) on objects and cells, so arrival events are visible. The K2a items the reviewer listed as open were fixed in K2a.
