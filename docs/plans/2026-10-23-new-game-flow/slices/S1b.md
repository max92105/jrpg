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
