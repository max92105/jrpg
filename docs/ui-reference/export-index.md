# Export index

The developer's extract of Octopath Traveler II's UI is at `C:\Users\max92\Downloads\umodel_win32\Export\Game\UserInterface\`. It holds textures only: no layouts, and most files are white masks the game tints. It is **reference only** (decision 264): never committed, loaded or shipped. Take the idea, draw our own version, and take sizes from our captures and the plan's tunables, never from these files.

Paths below are under that folder, in the 1x `Texture` folders (`Texture_x2` and `Texture_x3` hold the same files larger). Walking the export whole times out: list one subfolder at a time. The Read tool shows a PNG.

## Title

Screenshots: `title-screen.jpg`.

- `Title/Texture/UiTX_Title_SelectMenu_Focus.png`: the selected entry is a soft blue glow rising from a bright hairline under it, fading out to both sides.
- `Title/Texture/UiTX_Title_SelectMenu_OutFocus.png`: the other entries keep only the hairline, with no glow.

## Shared pieces: windows, rows, bars

Screenshots: `dialog-confirm.webp`, `menu-skill-learned-dialog.webp`.

- `Common/Texture/UiTX_Common_Dialog_Frame.png`: a window's frame is a double hairline, the two lines offset so the corners cross and open.
- `Common/Texture/UiTX_Common_DialogLine_CeltCenter.png`: a separator with a small knot of interlaced diamonds at its middle.
- `Common/Texture/UiTX_Common_ListFocus_Base.png`: the selected row is a band that fades out to the right, not a box.
- `Common/Texture/UiTX_Common_HeaderTitle_Base.png`: the header bar fades from dark on the left to nothing on the right.
- `Common/Texture/UiTX_Common_Footer_Base.png`: the footer is a dark bar under a soft pale fade, so it melts into the screen above it (the title's footer line too).
  - Ours already does this: `UiTextures.DrawSelectedRow`, `HeaderBar` and `FooterBar`.

## Dialogue box

Screenshots: none of the dialogue box; `dialog-confirm.webp` for the window style.

- `Narration/Texture/UiTX_Narration_Frame_Line2_1M.png`: a single long hairline over the text, with a curl dipping at the left end and a small leaf at the right.
- `Narration/Texture/UiTX_Narration_Note_Base02.png`: a letter or note read in a scene is a torn parchment sheet with darkened edges, unlike the dark windows.

## Battle: party panel and turn order

Screenshots: `battle-party-panel.webp`, `battle-turn-order.webp`.

- `Battle/CharacterStatus/Texture/UiTX_Battle_Dot_Boost001.png`: a boost pip is a small glowing ember, not a flat dot.
  - Ours already has pips: `PipRow`.
- `Battle/ActionOrder/Texture/UiTX_Battle_Oder_Ply.png`: a turn-order diamond is lit pale at the top and shaded blue toward the bottom.
  - Ours already has the diamonds: `TurnOrderView`, `DiamondCrop`.
- `Battle/ActionOrder/Texture/UiTX_Battle_Order_NextTurn_Plate.png`: the "next turn" label sits on a soft blue oval glow, with no plate edge.
- `Battle/CharacterStatus/Texture/UiTX_Battle_IconStatus_Down_Poison.png`: a status mark is a small icon on a rounded grey square.
  - Ours already has marks: `StatusMarksView`.

## Battle: command panel, numbers, action name

Screenshots: `battle-command-menu-1.webp` to `-4`, `battle-damage-numbers.webp`, `battle-target-enemy.webp`.

- `Battle/Command/Texture/UiTX_Battle_CommandFocus_Gradation.png`: the focused command's glow shifts from violet to pale blue.
- `Battle/Command/Texture/UiTX_Battle_Command_Detail_Base.png`: the skill's description strip is dark in the middle and fades out at both ends.
- `Battle/Effect/Texture/UiTX_Battle_DamageFont_atlas.png`: numbers are bold italic numerals with a dark outline and a light-to-color gradient, one color per kind (white, red, green, blue, orange).
  - Ours already tints numbers by kind: `FloatingNumberTone`.

## Battle: victory and defeat

Screenshots: `battle-victory.jpg`.

- `Battle/Result/Texture/UiTX_Battle_Result_TextBase01_Base.png`: a reward row's band is solid in the middle and fades out to the right.
- `Battle/Result/Texture/UiTX_Battle_Result_Item_Line.png`: a long hairline starting from a small arrowhead and diamond.
- `GameOver/Texture/UiTX_GameOver_Frame_center.png`: the defeat frame's ornament is a braid of interlaced diamonds.
  - Ours already has the reward bars and member cards: `VictoryScreenView`.

## Game menu, Status, Equipment

Screenshots: `menu-tabs.webp`, `menu-status.webp`, `menu-equipment.webp`, `menu-inventory-description.webp`.

- `MainMenu/Other/Texture/UiTX_MainMenu_MenuList_Base.png`: the arrow-shaped tab in four states, from dim to selected.
  - Ours already has the tab shape: `NavTabShape`.
- `Common/Texture/UiTX_Common_MenuListIcon_Inventory.png` (and the other `MenuListIcon_*` files): each tab's icon is a simple line drawing (a bottle for items) in a single color. M1's placeholder glyphs take that idea.
- `MainMenu/Status/Texture/UiTX_MainMenu_Status_Detail_Base01.png`: a Status section is a pale panel with a soft shadow along its top edge.
- `MainMenu/Equip/Texture/UiTX_MainMenu_EquipAll_Line.png`: the equipment window is the shared double-hairline frame, the lines offset at the corners.

## Save and load

Screenshots: none; `dialog-confirm.webp` for the yes/no.

- `SaveLoad/Texture/UiTX_SaveLoad_List_Focus.png`: the selected save is a dark band lit blue at two opposite corners, with a bright hairline along the bottom.
- `SaveLoad/Texture/UiTX_SaveLoad_List_Base.png`: every save row is a plain band whose ends fade to dark, with a hairline along its top and bottom edges.

## The encounter shatter (B1a-B1c)

Screenshots: none.

- `Battle/ActionOrder/Texture/UiTX_Battle_Oder_Crack.png`: a crack radiating from one point, thin and glowing pale blue.
- `Battle/ActionOrder/Texture/UiTX_Battle_Oder_Fragment_03.png` (`_01` to `_17`): each shard is a small, pale, irregular triangle-like piece, lit at one edge.
