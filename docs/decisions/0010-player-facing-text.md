# 0010. Player-facing text

Status: accepted
Date: 2026-10-18

## Context
Slice 1c draws the first text; part 3b adds menus, battle messages, the title screen, save slots and found-item messages. Content text (item, skill and enemy names, dialogue lines in map events) already lives in `content/`. What has no home yet is the game's own text: menu labels ("Inventory", "Load"), prompts, and messages built from values ("Obtained {item} x{count}!", "File {slot}").

In the legacy demo these were literals in XAML (about 40 `Text="..."`) and C# (`$"Obtained {item.Name}!"`, `$"File {n}"`). CLAUDE.md says no player-facing text in C#, and Data's save repair already returns its changes as English strings (`RepairedSave.Changes`), which part 3b may show the player (question 63).

Constraints: solo developer, so wording must be easy to find and change; a missing or misspelled text should fail early, not show blank; no string building per frame (`game.md`); localization isn't planned, but nothing should block it.

## Options
### A. Strings in Game code (constants per screen)
- Pros: nothing to load; the compiler finds every use; fastest to write.
- Cons: against CLAUDE.md; wording changes need a rebuild and a hunt through screens; translation later means extracting every string.

### B. One message table, `content/text/ui.json`
An id -> text map with named placeholders (`"found-item": "Obtained {item}!"`). Data's `UiMessageLoader` reads and checks it (no empty text, balanced braces); the Game holds it in `Ui/UiMessages` (`Text(id)`, `Format(id, arguments)`), with every id as a constant in `Ui/MessageIds`.
- Pros: all wording in one file next to the other content; a test checks every `MessageIds` constant exists in the file, so a typo fails the build's tests, not the game; named placeholders let the wording reorder values; a later language is another file.
- Cons: an id constant and a JSON entry per text (two places to touch); placeholders are checked when formatted (a test per message that formats), not by the compiler.

### C. One text file per screen (`content/text/battle.json`, `menu.json`...)
- Pros: smaller files that match the screens.
- Cons: more loading code and checks for the same thing; shared texts ("Back", "Cancel") have no obvious home; at this size one file is easy to read.

## Decision
**B** (developer, 2026-10-18).

Rules that come with it:
- Placeholders are `{name}`, filled by `MessageArgument(Name, Value)`; an unknown id or a missing argument throws, so it's caught by tests and the first run.
- Formatted text is built when the state changes (a menu opens, an item is found), never every frame.
- Data and Core don't word messages for the player: they return structured records (what happened, which ids), and the Game words them through the table. `RepairedSave.Changes` becomes structured records when part 3b shows them (question 63).
- Content names (items, skills, dialogue) stay in their own content files; the table holds only the game's own text.

## Consequences
- Easier: rewording without touching code; a review sees every text in one diff; translation stays possible.
- Harder: adding a text means an id constant plus a JSON entry; the `MessageIds` test keeps them in step.
- Watch: the table must not grow into game design (prices, counts); only text.
