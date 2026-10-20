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
