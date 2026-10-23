# S1b. Choosing asks "Begin as X?" first

Part of [the new-game flow plan](../README.md). Design: [design.md](../design.md), "The flow".

- **S1b. Choosing asks "Begin as X?" first** (Game + content, about 90 lines; decision 170)
  - **Goal:**
    - Confirm on a character opens the existing yes/no prompt, "Begin as Zanyr?".
    - Yes returns `LeaderChosen`.
    - No, or Cancel, closes the prompt and leaves the cursor where it was.
    - While the prompt is open, Up and Down move the prompt's answer, not the list.
  - **Changes:**
    - `NewGame/LeaderChoiceScene`:
      - holds a `ConfirmPrompt?` and draws it with `ConfirmPromptView`, as `SavePage` does;
      - the question is formatted when the prompt opens;
      - Cancel on the list still returns to the title; Cancel in the prompt only closes the prompt.
    - `content/text/ui.json` and `Ui/MessageIds`: `leader_choice_confirm` ("Begin as {name}?").
    - `GameFlowTests.StartedFlow` answers Yes.
  - **Tests (`LeaderChoiceSceneTests`):**
    - `Update_Confirm_AsksBeforeChoosing` (returns nothing, prompt open)
    - `Update_ConfirmThenYes_RequestsThatLeader`
    - `Update_ConfirmThenNo_StaysOnTheSameCharacter`
    - `Update_CancelInPrompt_ClosesPromptOnly`
    - S1's confirm tests answer Yes.
  - **Verify by hand:** Confirm on Omer asks "Begin as Omer?"; No goes back to the list on Omer; Yes starts the game.
  - **Done, with deviations:**
    - The question's Yes and No are switched with Left and Right (`ConfirmPrompt`'s keys, as on the Save page), not Up and Down as the goal said. It starts on No, so a stray Confirm changes nothing. While it is open, the list takes no input (`Update_DownWhileAsking_KeepsTheCursor`).
    - The scene now takes `UiTextures` for the prompt's panel. The tests and the flow helper answer Yes with Left, then Confirm.
    - Added beyond the plan: `Update_DownWhileAsking_KeepsTheCursor`. `Update_ConfirmTwice_RequestsLeaderOnce` became `Update_ConfirmAfterYes_RequestsNothingMore`.
    - Review: the scene keeps only the names it shows, keyed by member id, not the whole member catalog. The third copy of holding an open yes/no question has a backlog line.
