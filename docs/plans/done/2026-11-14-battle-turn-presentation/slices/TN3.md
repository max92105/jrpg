# TN3. The plaque names every action from its wind-up's start, popping in and out

Game, about 170 lines (about 100 of them tests, 35 removed). Needs TN2. Decision 646; tunable "Plaque time";
replacement mapping rows TN3.

## Goal
The banner shows the action's name the moment its wind-up starts: "Attack" for an attack (the command's own
`battle_attack`), the skill's or item's name, "{name} defends.", "Move"; nothing for Run. It pops in at full opacity,
stays 1.5 s and vanishes at once. The playback no longer knows about the banner. Skills, items, Defend and Move share
the plaque (its look is TN5's).

## New types
- `Game/Battle/ActionBannerText` (internal static): the words the plaque shows for an action, from its actor and
  command. `static string? For(Combatant actor, BattleCommand command, UiMessages messages)`, moved from
  `ActionPlayback.BannerTextFor` with `AttackCommand` giving `MessageIds.BattleAttack`; an unknown command throws.

## Changes
- `ActionWindUp.Of(Combatant actor, BattleCommand command, UiMessages messages)`; `string? BannerText`. Its summary
  names the plaque.
- `BattleTurnSequence`: passes its `_messages` to the wind-up (its `messages` param doc: "how the plaque writes what an
  actor does").
- `ActionPlayback` (292 → about 272): `BannerText`, `BannerTextFor` and `AddBanner`'s beat go; a private action name,
  read through `ActionBannerText.For`, stays for the placeholder cue a non-damage effect shows (`AddLanding`). Its
  summary drops "a banner for a skill, an item or Defend".
- `PlaybackBeatKind.Banner` goes (no beat starts with the name now); grep `PlaybackBeatKind.Banner` and "banner" in
  `Game/Battle/` comments.
- `BattleScene` (about 195 after TN0): `ShowPlayback` and `_shownPlayback` become `ShowWindUp` and `_shownWindUp` (the
  banner shows when a new wind-up appears); net 0 lines.
- `BattleBanner`: shown for 1.5 s at full opacity, then hidden; `Opacity` stays the view's input (1 or 0). Its summary
  gives the clip's reason (no fade seen at 33 ms).

## Tests
- `ActionBannerTextTests` (new): `For_Attack_IsAttack`; `For_Skill_IsTheSkillsName`; `For_Item_IsTheItemsName`;
  `For_Defend_NamesTheActor`; `For_Move_IsMove`; `For_Flee_IsNothing`. These replace `ActionPlaybackTests`'
  `Start_MoveCommand_ShowsMoveBanner`, `Start_ExtraTurnReadied_ShowsSkillBanner` (its point moves to
  `For_Skill_IsTheSkillsName`), `Start_SkillCommand_ShowsSkillName` and `Start_ItemCommand_ShowsItemName`, deleted with
  the banner's move (said in the Done note); `Start_AttackLanded_HasOneImpactBeat` loses its `BannerText` line;
  `Start_HealLanded_ShowsPlaceholderCueNotStrip` still proves the action's name reaches the cue.
- `ActionWindUpTests`: `Of_Attack_NamesItAttack`, `Of_Skill_NamesTheSkill` (the wind-up carries the shared text).
- `BattleBannerTests` (35): `Update_DuringHold_IsFullyShown` stays; `Show_AtOnce_IsFullyShown` (no fade-in: 1 at 0 s);
  `Update_PastHold_FadesOut` becomes `Update_PastItsTime_IsHidden` (1 just before 1.5 s, 0 at 1.5 s: both sides).
- The scene's one line showing the wind-up's text on the banner has no test of its own (the banner is drawn, never read
  back); the hand check carries it.

## Verify by hand
1. Attack a goblin: "Attack" appears as soon as the command is confirmed, not 0.7 s later; it disappears at once about
   1.5 s later (no fade).
2. A goblin's turn: "Attack" right after its pause.
3. A skill shows its name, Defend "Sarxian defends.", Run nothing.

- **Done:** as planned (`ActionPlayback.cs` 292 → 272). Deleted with the banner's move: `ActionPlaybackTests`' `Start_MoveCommand_ShowsMoveBanner`, `Start_ExtraTurnReadied_ShowsSkillBanner`, `Start_SkillCommand_ShowsSkillName` and `Start_ItemCommand_ShowsItemName` (their points now in `ActionBannerTextTests`), and the Defend test's banner line; the banner beat leaves `ActionPlaybackFallTests`' and `ActionPlaybackPhaseTests`' expected beats. An attack would now name itself on a non-damage cue, but none exists (its damage always takes the strip). Review: a stale comment in `AddLanding` fixed; `BattleBanner._shown` renamed `_showFor`.
