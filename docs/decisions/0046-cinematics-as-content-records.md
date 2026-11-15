# 0046. Cinematics as content records

Status: accepted
Date: 2026-11-14

## Context
Each leader's intro is a cell event of `intro.json` (ADR 0016): a `voice` step, three `still` steps of four seconds
with a caption each, then a warp. Ragnir's intro now has its six real stills, and the developer wants to set a
cinematic up in the editor: the stills in order, how long each shows, a voice line fitting each still, music under
the whole of it and a slow zoom per still, with a preview. Today a still is edited as one step among others in a map's
cell event, found by opening the intro map in Events mode, and the event dialog's step switches are at their
complexity limit. Plan `2026-11-14-cinematics`, decisions 609-630.

## Options
### A. A record played by one event step (chosen)
`content/cinematics.json` holds each cinematic (id, an optional track, its stills in order: picture, seconds, an
optional subtitle, voice line and zoom), edited in a Database window (ADR 0045); an event plays one with
`{ "type": "cinematic", "cinematicId": "..." }` and keeps the scripting around it (the leader gate, the warp after).
- Pros: a window independent of any map; a record that can be reused and previewed as a whole; the voice and the zoom
  sit on the still they belong to; the event stays two steps.
- Cons: a new content file with its loader, validator, writer, undo refusal and startup checks (about 21 slices with
  the game, the preview and the retirement of the `still` step); two ways to show stills until that retirement.

### B. Today's steps, improved (about 6 slices)
The `still` step gains a zoom (a start and an end percent) and an optional voice, a new `music` step sets the track
for the rest of the event, and the event dialog's step editor gets a picture box, Up and Down, and a still's preview.
- Pros: no new file or catalog plumbing; everything stays in the intro map's event; about a third of the work.
- Cons: a cinematic is edited inside one map's cell event; it can't be reused or previewed as a whole outside that
  event; the event dialog's switches, already at their complexity limit, grow with each field.

### C. A record played by its own scene, outside events (ADR 0016's option A)
- Pros: the smallest player.
- Cons: loses the event's gate and warp; a second way to script scenes, rejected once already.

## Decision
A: a cinematic is a content record played by one `cinematic` event step, chosen for a window independent of any map
and a reusable, previewable record. A still names its picture with the cinematic's folder under
`content/art/cinematics/` (`ragnir_intro/1_happy_life.png`), as music names `town/loop.wav` (decision 610).

## Consequences
- Amends ADR 0016's decision B for the presentation part: the event still scripts the intro; the stills, their
  timing, voices, zoom and track move into the record.
- Amends ADR 0009's "file name only" for stills: a still's picture carries its cinematic's folder.
- Time and look rules (which still shows at a time, its opacity, its zoom, whose voice speaks) live in Core, read by
  the game and the editor's preview alike; the black around a cinematic, the subtitle band and the hold to skip stay
  in the game.
- The `still` step is retired once every intro is a cinematic (decision 621); the `voice` step stays as the voice-over
  for any story scene.
- An undo that would remove a cinematic a map's event plays is refused (decision 630), as for other referenced records.
