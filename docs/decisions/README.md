# Architecture decisions

One line per ADR: what was decided. Read the ADR itself before changing that area. New ADRs come from `/adr` (template: `_template.md`); add a line here when one is written.

| ADR | Area | Decided |
|---|---|---|
| [0001](0001-engine-free-architecture.md) | Architecture | `Jrpg.Core` holds every game rule with no rendering reference; `Jrpg.Game` (MonoGame) and `Jrpg.Editor` (WPF) are thin shells over it. |
| [0002](0002-content-schema-mapping.md) | Content | Content JSON loads straight into Core records; a Data DTO only where a file's format doesn't match the model, with the reason in that loader. |
| [0003](0003-equipment-ownership-model.md) | Items | Equipping takes the copy out of the inventory; unequipping puts it back. |
| [0004](0004-battle-command-flow.md) | Battle | One `Execute(BattleCommand)` per turn returns the ordered `BattleEvent`s; `EnemyTactics`, outside `BattleState`, chooses enemy commands. |
| [0005](0005-map-content-source-of-truth.md) | Content | *Superseded by 0011.* The WPF demo's `Assets/` and the converter were the source of truth for maps during the migration. |
| [0006](0006-map-event-flow.md) | Map events | `EventRun` steps through an event; swaps apply at once, dialogue, give-item and warp steps wait for `Continue()`; a "once" event is marked done when it starts. |
| [0007](0007-save-format-and-versioning.md) | Saves | Versioned JSON saves; a format change bumps `formatVersion` with a migration and a test on the old format; writes are atomic. |
| [0008](0008-game-shell-structure.md) | Game shell | Scenes return `SceneRequest`s that `GameFlow` applies through transition handlers; one `GameSession` at a time; the update path never touches the graphics device, so the flow runs headless in tests. |
| [0009](0009-art-location-and-asset-loading.md) | Art | Art lives in `content/art/<kind>/` as PNGs loaded at runtime through `TextureLibrary`; TTF fonts through FontStashSharp; `ArtReferenceCheck` checks references at startup. |
| [0010](0010-player-facing-text.md) | Text | Player-facing text lives in `content/text/ui.json`, with `{name}` placeholders; ids are constants in `Ui/MessageIds`, each checked by a test. |
| [0011](0011-content-after-the-converter.md) | Content | `content/` is the only source of truth: two files per map (settings and layout), catalogs at the content root, the editor writes through Data's `ContentWriter`. |
| [0012](0012-editor-architecture.md) | Editor | MVVM with `CommunityToolkit.Mvvm`; editing logic is headless (`MapDraft`, `IMapEdit`, `EditHistory`) and tested; content I/O only through Data. |
| [0013](0013-battle-presentation-and-playback.md) | Battle | The battle HUD reads live Core state (`Execute` at the moment of impact); the battle scene stacks over the map; playback is event-driven beats, exhaustive over `BattleEvent`. |
| [0014](0014-editing-shared-catalogs.md) | Editor | Catalogs (sheets, tiles, objects) are edited in one `CatalogDraft` read by every panel, saved with the map by Ctrl+S (`WriteMapCatalogs`, all three files or none) and undone by Ctrl+Z through a journal over map and catalog histories. |
| [0015](0015-npcs-on-maps.md) | Content | An NPC is an object drawn from a field sprite (`objects.json` `fieldSpriteFile`), placed with an Interact event; NPCs share a placeholder sprite until real art. |
| [0016](0016-intro-cinematics.md) | Map events | A character's intro is a map cutscene: an arrival-activated event (gated on the leader) of still steps (image, seconds, caption) and a voice step, run by `EventRun`, ending with a warp; skipping runs past presentation steps only. |
