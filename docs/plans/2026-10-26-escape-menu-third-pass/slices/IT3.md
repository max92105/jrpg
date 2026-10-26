# IT3. The aimed cards show HP now and after

Part of [the escape menu third pass plan](../README.md). Decisions 267, 287. Design: [design.md](../design.md), "Core change", "Tunables and placeholders" (HP preview), "Key decisions" (`ItemUse` exposes the picker and an HP-after lookup; `UiTheme.PreviewColorOf`). After IT2. Look at `docs/ui-reference/menu-inventory-target.webp` (the "now ▸ after" values on the cards) first.

- **IT3. The aimed cards show HP now and after (`FieldItemUse.HpAfterUse`)** (Core + Game, about 210 lines)
  - **Goal:** while an item is aimed, every lit card's HP row reads "now ◆ after", the after value in the rise color when it is higher, so the player sees what the potion will do before choosing; after a use (IT2), the previews show the new values.
  - **New types:** Game `Ui/HpPreview` (internal sealed record): a card's HP after an item, measured once: the text and its color. `HpPreview(MeasuredText After, Color Color)`.
  - **Changes (Core):** `Party/FieldItemUse`:
    - `HpAfterUse(ItemDefinition item, PartyMember target)`: the HP the target would have after the item, without using it. Throws as `Use` does for an item with no effect or a rest item.
    - One private helper lands the effect (`EffectResolver.Apply` with the target's defense) on given vitals; `Use` calls it on the target's, `HpAfterUse` on a copy (`new Vitals(hp, maxHp)`), so the two can't drift.
  - **Changes (Game):**
    - `Menus/ItemUse`: when aiming starts and after each use, fills an HP-after lookup for the candidates (`HpAfterUse`); `int HpAfter(PartyMember member)`. No separate aim record.
    - `Ui/UiTheme.PreviewColorOf(int change)`: `Positive`, `Negative` or `TextPrimary`; `Menus/StatPreviewText` uses it instead of its own `ColorOf`.
    - `Ui/MemberCardView`: `HpPreviewOf(int hp, int hpAfter)` measures with the card's value font; `Place` gains `HpPreview? Preview`. With a preview, the HP row's value column (140 px, unchanged, so the bar keeps its length) reads the current HP, a 6 px diamond (Equipment's preview mark), then the after value in its color. Callers without a preview pass null (`PartyColumnsView`).
    - `Menus/PartySidebarView`: when `ItemUse.Picker` changes (by reference), formats each candidate's `HpPreview`, as it formats cards when they change; draws them on every lit card (decision 287).
  - **Tests:**
    - Core, new class `FieldItemUseHpAfterTests` (`FieldItemUseTests` is full), with test items of pinned power:
      - `HpAfterUse_PotionOnHurtMember_AddsThePotionsPower`;
      - `HpAfterUse_PotionNearFullHp_StopsAtMaxHp`;
      - `HpAfterUse_PotionOnFallenMember_StaysAtZero`;
      - `HpAfterUse_ReviveOnFallenMember_IsTheRevivedHp`;
      - `HpAfterUse_AnyItem_LeavesTheMembersHpAsItWas`.
    - Game:
      - `UiThemeTests`: `PreviewColorOf_Rise_IsPositive`, `PreviewColorOf_Drop_IsNegative`, `PreviewColorOf_NoChange_IsPrimaryText`;
      - `InventoryPageAimTests`: `Update_ConfirmPotionWithTheLeaderHurt_AimsWithTheLeadersHpAfter` (the value Core gives, not a copy of the formula); `Update_UsePotionWithCopiesLeft_HpAfterFollowsTheNewHp`.
    - `FieldItemUseTests` and `StatPreviewTextTests` pass unchanged.
  - **Verify by hand:** hurt two members, aim a potion: each lit card reads "now ◆ after" with the after value in cyan; a member at full HP reads the same number twice in white; a revive on a fallen member shows 0 ◆ its revive HP; the bar doesn't change length; after a use the previews update; they go when aiming ends. F12, compare with `menu-inventory-target.webp`.
