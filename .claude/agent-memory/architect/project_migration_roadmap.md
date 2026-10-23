---
name: migration-roadmap
description: The WPF demo migration is finished (demo deleted, tag wpf-demo-final); shared vocabulary, next decision/ADR numbers, what the next plans are
metadata:
  type: project
---

The migration from the WPF demo is finished (2026-10-23): every plan is in docs/plans/done/, the demo was deleted in commit cbaaba5 and its last state is the annotated tag `wpf-demo-final`. `verify.ps1` fails on the retired word outside history (decision 136). Next question/decision number 148, next ADR 0016.

Open questions carried by the demo retirement plan (placeholders in code): 145 party sidebar placement, 146 sidebar card content, 147 key names in the prompt; 69-rest dropped.

Next plans are new systems only: cutscenes on map events (sketch in done/2026-10-22-demo-retirement/design.md, needs an ADR), map name banner (q53), autosave (q44, q67), event battles (q30), gold and drops (decision 103), animated battle sprites (decision 100), NPC movement (q127), audio (q76), loading screen (q66). Code-health leftovers: docs/plans/backlog.md.

**Why:** the developer wanted nothing of the demo left before new features ("ready to grow and change").

**How to apply:** reuse vocabulary (PartyMember, Combatant, *Definition, BattleState, PartyState, ContentCatalog, ExplorationState, EventRun, GameSession, FileSaveStore; Game: SceneRequest records in Scenes/, GameFlow + SessionTransitions/MenuTransitions/BattleTransitions, SceneFactory, EventPresenter, ScreenFade, BattleScene, CommandMenu, ActionPlayback, Ui/ components such as MemberCardView and StatBar). Earlier decisions live in the done plans' READMEs. See [[content-quirks]], [[plan-lessons]].
