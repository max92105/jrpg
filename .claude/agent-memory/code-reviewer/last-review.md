---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, M3b (Legacy theme, message box and found items)
Round: 1 (verdict: Ready to commit; build 0 warnings, format exit 0; tests 875/876 on first full run: Game test host crashed with AccessViolation in pre-existing FontLibraryTests.Load_FileThatIsNotAFont, then 9 clean Game reruns, one rerun hung)

Accepted before coding (not findings): message box copies legacy in-game DialogueBox doubled (1200x240, 40 px bottom, radius 20, border 4, text 32) instead of menu DialogStyle; GradientStrip + UiTextures moved to G1, PanelView bakes lazily via pure PanelStyle/BakePixels.

- open (Warning): ExplorationScene.cs:57-70 wasBusy rule (closing Confirm must not interact) has no test; start chest isOnce=true so reverting is invisible; needs a repeatable give-item fixture
- open (Suggestion): FoundItemText all-lost text omits the item name ("3 didn't fit...") - ask developer (decision 55 wording)
- open (Suggestion): EventPresenter.Blackness now only read by a test
- open (Suggestion): M3b slice note: test names (Update_ConfirmOnFoundItem_ContinuesEvent etc.), extra tests, GradientStrip/UiTextures to G1, DialogueBox source, checkbox (DoD 5)
- follow-up: FontLibraryTests.Load_FileThatIsNotAFont feeds a 23-byte file to StbTrueType, which reads out of bounds -> intermittent AccessViolation/hang of the whole Game test host
- verified: BakePixels border exactly BorderWidth px (depth 0.5..3.5 border, 4.5 fill), premultiplied lerp correct; give-item quantity >=1 validated so FoundItemText never returns empty; MessageIdsTests covers new ids in ui.json; Dialogue->Ui/Rendering and Exploration->Dialogue edges match plan
