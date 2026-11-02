# SH1. The Shop's gear preview moves into `Trade/GearPreview`

Part of [the sound pass plan](../README.md). Phase 3's first slice, after the developer's go (Phases 1 and 2 heard). The backlog's line on `Trade/ShopScene` (the music plan's S4). Design: [design.md](../design.md), "Key decisions" 5, "Replacement mapping".

- **SH1. The Shop's gear preview moves into `Trade/GearPreview`** (Game refactor, about 110 lines; no behavior change)
  - **Goal:** `ShopScene` (296 lines) leaves room for its sounds (SH2, SH3) by handing the member preview to its own type.
  - **New types:**
    - `Trade/GearPreview`: what wearing the highlighted piece would change for the previewed member, whom Left and Right switch through the active party (decision 383); rebuilt only when the row or the member changes. Takes the session's party and the messages (for its `EquipmentComparison`).
      - `PartyMember Member`; `StatComparisonText? Changes`; `void SwitchMember(InputFrame input)` (moves from `ShopScene.SwitchMember`); `void Follow(TradeRow? row)` (from `RefreshPreview`).
      - SH2 needs to know a switch happened for the move sound: `SwitchMember` keeps its shape; SH2 compares `Member` before and after, so nothing is added for it here.
  - **Changes:**
    - `Trade/ShopScene` (296 → about 255): `_memberIndex`, `_previewedRow`, `_previewedMember`, `_comparison`, `SwitchMember`, `RefreshPreview` leave; a `_preview` field; `PreviewedMember` and `Preview` stay as one-line reads of it (the Draw and `ShopScenePreviewTests` read them); the class summary keeps the preview sentence, pointing to `GearPreview`.
  - **Tests:** existing tests pass unchanged (the guard): `ShopScenePreviewTests`, `ShopSceneTests` and the other `ShopScene*Tests`. No new test: the move adds no behavior.
  - **Verify by hand:** at the shopkeeper: on the Buy list's gear rows, Left and Right switch the previewed member and the changes follow; moving to a stack row clears them. Screenshots (F12) before and after match.
