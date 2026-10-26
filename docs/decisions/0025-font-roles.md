# 0025. Font roles

Status: accepted
Date: 2026-10-25

## Context
ADR 0009 loads every TTF in `content/art/fonts/` into one FontStashSharp font system: the first file in name order is the font, the others supply characters it lacks. Every screen draws in that one face (Lato).

The UI second pass wants a serif for big titles: the game's name, "Victory!", the defeat title, the leader's name. The reference UI uses a serif there and a clean sans everywhere else. Decision 250 picked **Cormorant Garamond SemiBold** (SIL Open Font License, Google Fonts), with no bold header role, and damage numbers left in the sans.

The question is how content says which file plays which role, and how code asks for it. Constraints: a solo developer swaps fonts by dropping files; a missing font must stop the start with a clear message, like every other content problem; no new NuGet package.

## Options
### A. A subfolder per role
`content/art/fonts/*.ttf` stays the body font and its fallbacks. `content/art/fonts/titles/*.ttf` is the title face, loaded as a second font system whose fallbacks are the body files. `FontLibrary.TitleFontAt(pixels)` sits beside `FontAt(pixels)`.
- Pros: no new content file or schema. Swapping the serif is a file swap. The folder says the role at a glance, and today's rule for the body folder is unchanged.
- Cons: a role is a folder name the code knows. A third role means a third folder and a method.

### B. `fonts.json` naming a file per role
A small content file maps roles to file names (`{ "body": "Lato-Regular.ttf", "title": "CormorantGaramond-SemiBold.ttf" }`), with a loader and a validator in Data.
- Pros: roles are data, and one folder holds every file.
- Cons: a schema, a loader, a validator and their tests for two entries. The fallback order needs its own field.

### C. A file-name prefix
`title-*.ttf` in the same folder is the title face; everything else is the body.
- Pros: one folder, no new file.
- Cons: a naming rule hidden in code. Renaming a downloaded font becomes part of installing it, and the sorted fallback order mixes the two roles.

## Decision
A. Each role has a folder: `content/art/fonts/` is the body, `content/art/fonts/titles/` the title face (Cormorant Garamond SemiBold, with its `OFL.txt` beside it). The title face falls back to the body files, so a character it lacks still shows. Screens ask `FontLibrary` by role (`FontAt`, `TitleFontAt`), never by file.

## Consequences
- **Start-up checks:** both folders must hold a font. A missing or broken one stops the start with the folder or file named (`ContentValidationException`), as today's body folder does.
- **Changing fonts:** swapping the serif is a file swap, and a third role is a third subfolder plus one method.
- **Watch:** the title face's metrics differ from Lato's (Cormorant's x-height is much smaller), so screens that switch a text to the title role re-check its size and baseline against the captures.
