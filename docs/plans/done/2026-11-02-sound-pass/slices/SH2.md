# SH2. The Shop's move, confirm and cancel sounds on the tabs and in the lists

Part of [the sound pass plan](../README.md). After SH1. Decisions 421, 430. Design: [design.md](../design.md), "Key decisions" 4.

- **SH2. The Shop's move, confirm and cancel sounds on the tabs and in the lists** (Game, about 190 lines; adds no view type)
  - **Goal:** the Shop sounds as the Forge does (the music plan's S2):
    - **Tabs:** Left or Right that changes the tab asks the move sound (Left on Buy and Right on Sell don't wrap, so they stay silent); Confirm entering the list asks the confirm sound; Cancel closing the Shop asks the cancel sound.
    - **List:** Up or Down that moves the selection asks the move sound (`AskMoveIfChanged`, so a list of one is silent); a tab button that changes the category asks the move sound; Left or Right that changes the previewed member asks the move sound (a party of one is silent); Cancel back to the tabs asks the cancel sound; Confirm on a greyed row asks nothing.
    - The deal itself, the picker and the sale question are SH3's.
  - **Changes:**
    - `Trade/ShopScene` (about 255 → 275): an `AskedSounds` cleared as `Update` starts (before the closing check, so a closing Shop asks nothing); the asks above, each beside the change it follows; `Sounds` overrides `IScene.Sounds`. Tab, row and member compared before and after their moves. If the scene would pass about 285, the list's moves (`MoveCursor`, `TryChangeCategory` and their asks) move into a small `ShopListInput` first; say so in the Done note.
  - **Tests** (`ShopSceneSoundTests`, new; pick entries by what they are, never by fixed presses):
    - `Update_RightOnTheBuyTab_AsksTheMoveSound`
    - `Update_RightOnTheSellTab_AsksNothing`
    - `Update_ConfirmOnTheTabs_AsksTheConfirmSound`
    - `Update_CancelOnTheTabs_AsksTheCancelSound`
    - `Update_DownInAList_AsksTheMoveSound`
    - `Update_DownInAListOfOne_AsksNothing`
    - `Update_NextTabButtonInAList_AsksTheMoveSound`
    - `Update_RightOnAGearRowWithTwoMembers_AsksTheMoveSound`
    - `Update_RightWithOneActiveMember_AsksNothing`
    - `Update_CancelInAList_AsksTheCancelSound`
    - `Update_ConfirmOnAGreyedRow_AsksNothing`
    - `Update_UpdateAfterAMove_AsksNothing` (the per-update clear)
  - **Verify by hand:** the interface sounds were heard in Phase 1, so this slice's ear check is folded into Phase 3's listening (SH3's hand check). Before committing: at the shopkeeper, a quick listen: each action above sounds once and the silent cases stay silent.
  - **Done, with deviations:** `ShopScene` 257 → 286, at the slice's "about 285" point for a `ShopListInput`; the split was skipped, since SH3 only passes the list to the counter and adds no line. After the review, a member switch ticks only on a gear row, where the member shows (`Update_RightOnAStackRowWithTwoMembers_AsksNothing`). `Update_DownInAListOfOne_AsksNothing` became `Update_DownInAnEmptyList_AsksNothing` (the Sell list's Materials category, empty at the start; no content gives a list of one by itself); `AskMoveIfChanged` treats both alike. A shared `Trade/ShopDriver` (open the store, move down until a row, show a category) instead of a fourth copy of the "move down until" helper; the backlog line now names it. Tests 3687 → 3700. Not heard: the hand check is folded into Phase 3's listening.
