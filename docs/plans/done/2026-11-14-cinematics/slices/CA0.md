# CA0. One content commit: the stills listed, the copies deleted, the GIFs moved out

Content only, about 40 lines. Decisions 611, 612 (and 633 for the GIFs' names). The monsters plan depends on it.

## Goal
The tests pass again on the working tree. Today `ArtSourcesTests` fails on the six `cinematics/ragnir_intro/*.png`
(no entry in `content/art/art-sources.json`) and on the six GIFs at the root of `content/art/` (neither art the test
knows, `.png` or `.ttf`, nor a license file). The "art" commit (`2a7ac853`) is pushed, so this is a new commit, not an
amend. Nothing in code changes.

## Steps
All mechanical; the slice does them (the developer only confirms `git status` first).
1. `git status` shows the six French-named PNGs at the root of `content/art/` deleted (committed in `2a7ac853`; they are
   the same pictures as `ragnir_intro/`) and `content/art/cinematics/ragnir_intro/` untracked. Keep both: stage the
   deletions and the folder.
2. Move the six GIFs from `content/art/` to `art-src/gifs/` at the repository's root (decision 612), renamed (decision
   633): "Animation d'un archer gnoll en mouvement.gif" → `GnollArcher.gif`; "Boss hyène guerrier animé au fléau
   geschniegelt.gif" → `HyenaWarlord.gif`; "Gnoll guerrier animé à la hache.gif" → `GnollAxeWarrior.gif`; "Guerrier
   gnoll animé en combat.gif" → `GnollWarrior.gif`; "Shaman gnoll animé aux ornements oscillants.gif" →
   `GnollShaman.gif`; "Explosion de feu en pixel art animé.gif" → `FireExplosion.gif` (`git mv` if they were committed,
   else a plain move). `art-src/README.md` (three lines): what the folder holds (sources the editor imports from, never
   read by the game), and that `content/art/art-sources.json` lists what is imported from them.
3. Six lines in `content/art/art-sources.json`'s `files`, in the file's path order:
   `cinematics/ragnir_intro/1_happy_life.png` to `cinematics/ragnir_intro/6_i_will_get_him_back.png`, each with
   `"sourceId": "generated"` (decision 611, assumed). The GIFs get no line: they aren't under `content/art/`; the strips
   the monsters plan imports from them get theirs then.
4. `docs/plans/backlog.md`: the plan's out-of-scope lines (a pan toward a point, decision 618; audio in the editor's
   preview, decision 620); the `TextureLibrary` line names the Ragnir stills (decision 619).
5. One commit.

## Tests
None new: `ArtSourcesTests` (`Manifest_EveryImageAndFont_HasAnEntry`, `ArtFolder_EveryOtherFile_IsALicenseTextOrTheManifest`,
`Manifest_EveryEntry_NamesAFile`) fails before the slice and passes after.

## Verify by hand
`powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/verify.ps1` passes; `git status` is clean and shows
no `.gif` under `content/`; `art-src/gifs/` holds the six renamed GIFs.

- **Done:** as planned. The archer GIF's name holds a typographic apostrophe, not the plan's straight one; moved by pattern.
