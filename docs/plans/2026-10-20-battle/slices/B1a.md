# B1a. A request during the flow's fade waits for it

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B1a. A request during the flow's fade waits for it** (Game, about 90 lines; devil's advocate B-1)
  - **Goal:** a scene's request returned while the flow's fade plays starts once the fade ends, instead of throwing. Today that's the T1a follow-up; with battles, an outcome can arrive mid-fade.
  - **Changes:** `GameFlow` holds one waiting request; a second one throws `InvalidOperationException`.
  - **Tests:** `GameFlow_Update_RequestDuringFade_StartsAfterFadeEnds`, `GameFlow_Update_SecondRequestWhileOneWaits_Throws`.
