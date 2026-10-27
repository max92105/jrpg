# PD1. The Party page's refusals open a titled framed dialog with one OK

Part of [the escape menu third pass plan](../README.md). Decisions 268, 282, 283. Design: [design.md](../design.md), "Tunables and placeholders" (notice dialog), "Key decisions" (the dialog holds no state). After C2. Look at `docs/ui-reference/menu-skill-learned-dialog.webp` (title, separator, message, one OK) and `dialog-confirm.webp` first, and `docs/captures/2026-10-26-menu/menu-party-action.jpg` (the red line this slice replaces).

All three refusals open the dialog (decision 282); its title is "Party" (decision 283). Equipment's and the save page's red lines stay (decision 284).

- **PD1. The Party page's refusals open a titled framed dialog with one OK** (Game, about 230 lines with the 1.6x for a new view)
  - **Goal:** Confirm on the leader (or on a full party, or on the last member standing) opens a centered framed dialog over the dimmed menu: "Party" over the separator, the reason, and an OK button in the selection look with the pointer. Confirm or Cancel closes it; nothing else does anything while it is open.
  - **New types:**
    - Game `Ui/NoticeDialog` (internal sealed record): a notice the player closes with OK: its title and message, and which presses close it. `NoticeDialog(string Title, string Message)`; `static bool IsClosedBy(InputFrame input)`: Confirm or Cancel.
    - Game `Ui/NoticeDialogView` (internal sealed): draws a `NoticeDialog`: the screen dimmed (`UiTheme.Scrim`), a titled `FramedWindowLayout` drawn by `FramedWindowView`, the message centered, the OK button through `DialogButtonView`, always selected. It measures and lays out when the dialog changes (by reference), as `ConfirmPromptView` does. `NoticeDialogView(ScreenText text, UiTextures ui)`; `Draw(SpriteBatch batch, TextureLibrary textures, NoticeDialog dialog, float opacity)`.
  - **Changes (Game):**
    - `Menus/PartyPage`: `NoticeDialog? _notice` replaces `PartyColumns.Notice`. The three refusals open it (title `MessageIds.MenuParty`); while it is open, `Update` only checks `NoticeDialog.IsClosedBy`; `LeavesOnCancel` is false while it is open (Cancel wins when nested); `Draw` draws it last. `Notice` stays as the open dialog's message, so the existing refusal tests read it unchanged. The `Notice = null` clears on moving go. Expect about 268 lines.
    - `Menus/PartyColumns`: `Notice` goes; `PartyColumnsView.ColumnsOf(PartyState party)`; its red notice line, `NoticeGap` and body font go.
    - `ui.json` `notice_ok` "OK"; `MessageIds.NoticeOk`.
  - **Tests:**
    - New class `NoticeDialogTests`: `IsClosedBy_Confirm_IsTrue`, `IsClosedBy_Cancel_IsTrue`, `IsClosedBy_Down_IsFalse`.
    - Replaced in `PartyPageTests`, as decision 268 changes the behavior they pin:
      - `Update_MovingAfterRefusal_ClearsTheNotice` becomes `Update_DownWhileNoticeOpen_KeepsTheNoticeAndTheCursor` (Down does nothing while the dialog is open);
      - `Update_LeaderSelected_OffersNoMove`'s `Assert.True(page.LeavesOnCancel)` goes: with the dialog open, Cancel closes it first, so `LeavesOnCancel` is false; the check moves to `PartyPageNoticeTests.LeavesOnCancel_NoticeOpen_IsFalse` and `Update_CancelOnNotice_ClosesIt`. Its other two assertions stay.
    - New class `PartyPageNoticeTests` (`PartyPageTests` is at 251):
      - `Update_ConfirmOnNotice_ClosesIt`;
      - `Update_CancelOnNotice_ClosesIt`, after which `LeavesOnCancel` is true again;
      - `LeavesOnCancel_NoticeOpen_IsFalse` (so the menu passes Cancel to the dialog and keeps the page open);
      - `Update_ConfirmOnNotice_DoesNotConfirmTheMemberAgain` (the closing press doesn't reopen it).
    - The three refusals' message assertions (`page.Notice` equals the message) pass unchanged.
  - **Verify by hand:** Confirm on the leader: the dialog opens over the dimmed menu, "Party", the separator, the sentence, OK selected with the pointer; Confirm closes it, the cursor still on the leader; Esc closes it without leaving the page; a full party and the last one standing open it too. F12, compare with `menu-skill-learned-dialog.webp`.
- **Done, with deviations:** `PartyPage`'s `MoveInColumn` and `SwitchColumn` wrappers went with the notice clears they held; the page calls `PartyCursor` directly. `Update_DownWhileNoticeOpen_…` builds a party of three first, so Down would move the cursor if the dialog let it. `NoticeDialogView` sizes the window to the wider of the title and the message. `PartyPage` 257 → 260 lines. Tests: 3 in `NoticeDialogTests`, 4 in `PartyPageNoticeTests`, one replaced and one assertion moved in `PartyPageTests`; 2243 in the solution.
