# EN8. The Enemies window: Tactics and Phases tabs

**Later: not in the approved plan** (README, "Later"); a draft kept with EN4; EN7's tactics summary line goes when it lands.

Editor, about 180 lines (XAML and window code). Builds on EN5 and EN7.

## Goal
Two more tabs. **Tactics**: the action list (Up, Down, Add, Remove), the chosen action's skill box ("Attack" first), weight, condition box ("Always" then every condition kind) and the percent box shown only for a kind that reads one. **Phases**: the phase list (threshold shown "below 50 %"), Add and Remove, and for the chosen phase its threshold and the same action list control as Tactics.

## New types
- `Editor/Battle/TacticListView` (`.xaml` + `.xaml.cs`, a `UserControl` over `TacticListViewModel`): the action list and its editor, used twice (Tactics tab, and inside Phases).

## Changes
- `Editor/Battle/EnemyCatalogWindow.xaml`: the two tabs.

## Tests
None new: EN4 and EN5 test the rules.

## Verify by hand
1. Database > Enemies..., goblin_chief: Tactics lists its actions with weights and conditions; Phases shows its phase(s) with their own actions.
2. Choose the wolf (EN7): Tactics, Add: "Attack, weight 1, Always"; Add again, pick a skill the wolf could use, weight 2, condition "a member below a share of HP" (or whichever kind reads a percent), 50: the percent box shows only for such kinds.
3. Phases, Add: below 50 %, add one action; set the threshold to 0: Data's message, OK greyed; 40, OK.
4. Ctrl+S; `git diff content/enemies.json` shows the wolf's tactics and phase.
5. Revert with `git checkout -- content` if this was only a check.
