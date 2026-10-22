# B5b. Event-driven beats and the banner

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B5b. Event-driven beats and the banner** (Game, about 260 lines)
  - **Goal:** each turn's events become beats, and the hold lasts as long as its beats (key decision 4).
    - A skill or item shows its name on the banner, and Defend shows "{name} defends.".
    - An impact beat holds 1.0 s; a defeat in it extends it to 2.0 s.
    - Defend and Run have no hold.
  - **New types (Game):**
    - `Battle/PlaybackBeat`: one timed step of a turn's playback: banner, impact or defeat.
    - `Battle/ActionPlayback` (pure): one turn's events as beats, and how far through them the playback is.
      - It is built from `Execute`'s events.
      - Its switch over `BattleEvent` types is exhaustive; anything else throws naming it.
      - `IsFinished`, `CurrentBeat`, `BannerText`.
    - `Battle/BattleBanner` (pure timing): the banner's opacity over time, and its text.
  - **Changes:** `BattleTurnSequence` holds until its `ActionPlayback` is finished.
  - **Tests:**
    - Beats: `ActionPlayback_Start_AttackLanded_HasOneImpactBeat`, `ActionPlayback_Start_TargetFell_AddsDefeatBeat`, `ActionPlayback_Start_Defend_HasNoHold`, `ActionPlayback_Start_SkillCommand_ShowsSkillName`.
    - Unknown events: `ActionPlayback_Start_UnknownEvent_Throws`, with a test-only `BattleEvent` subtype. A comment in the test says why `BattleEvent` stays open (unlike `BattleCommand`): new events come with new mechanics (ADR 0004), and the playback must refuse what it can't show.
    - Banner: `BattleBanner_Update_PastHold_FadesOut`.
  - **Verify by hand:**
    - [ ] Defend and Run feel immediate.
    - [ ] Fireball shows its name.
- **Done, with deviations:** `ActionPlayback` is an immutable list of beats with its `Duration`; the sequence holds for that duration, and `IsFinished`/`CurrentBeat` wait for their first user (B5c). The beat kind is a separate `PlaybackBeatKind` enum; the banner beat lasts 0 s (the banner fades on its own clock), the impact 1.0 s, a defeat adds 1.0 s; one beat of each kind per turn. The banner is drawn by a new `BattleBannerView` (legacy band and hairlines, `UiTheme.Banner`); "{name} defends." is the message `battle_defends`. The scene shows the banner when a new playback appears, not at a stage change, since Defend passes Holding within one update. `BattleTurnSequence` takes `UiMessages`. Added banner tests for hidden-before-any-message and fully-shown-during-hold; review added `ActionPlayback_Start_TwoTargetsFell_HoldsOneImpactAndOneDefeat` and `ActionPlayback_Start_ItemCommand_ShowsItemName`, and `BattleScenarios.Catalog` is public for them.
