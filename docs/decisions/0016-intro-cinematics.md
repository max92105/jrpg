# 0016. Intro cinematics are map cutscenes

Status: accepted
Date: 2026-10-23

## Context
Choosing a leader plays that character's intro: a voice-over over still images that appear and disappear (decision 159). The developer has no budget for animation or video, wants placeholders now, and will drop in real stills and audio later without code changes (decision 160). Captions double as subtitles (decision 172), and each still's time is set in content so it can match the voice (decision 173).

The roadmap also expects map cutscenes (scripted story scenes). Is the intro the start of that system, or a separate slideshow? Constraints: content only in `content/` (ADR 0011), scenes return requests and the update path never touches a device (ADR 0008), runnable headless in tests.

## Options
### A. A slideshow content type, played by its own scene
`cinematics.json` lists stills (an image, seconds, a caption) and one voice file; a `CinematicScene` plays one over the map.
- Pros: exactly what the intro needs; small; deterministic.
- Cons: a second way to script a scene once map cutscenes exist; the intro can't move the player, give items or warp.

### B. Map cutscenes on `EventRun`
The intro is a map event: new event steps (show a still for some seconds with a caption, play a voice) run by `EventRun` (ADR 0006), with a new activation that runs an event when the player arrives on its map. The intro's event ends by warping to where play starts.
- Pros: one scripting system for the intro and every later story scene; intros can do anything an event can (warp, give items, recruit); the editor authors them once it knows the new steps.
- Cons: bigger first step (a new activation, new steps, the editor's support for them, a way to give each leader their own intro); skipping must jump past the presentation steps without skipping state changes such as the final warp.

### C. Video files
- Pros: any look.
- Cons: DesktopGL has no video playback, so it needs a package; real videos cost what the developer doesn't have.

## Decision
B (developer, 2026-10-23): the intro is a map cutscene. A new event activation runs an event when the player arrives on its map; new steps show a still for a set time with an optional caption and play a voice-over; the intro event ends with a warp to where play starts.

## Consequences
- Every later story scene builds on the same steps; new step kinds (camera, moves, waits) extend `EventStep` and `EventRun` as ADR 0006 does.
- Timing stays deterministic: a still's seconds count on the update delta, and `EventRun` waits on the step until the Game reports it shown, as it waits on a warp's fade.
- Skipping an intro runs on past the presentation steps (stills, voice) but still applies the steps that change state (warp, items, recruits).
- The editor must open and save maps that use the new activation and steps, so it learns them in the same plan.
- Voice and stills run on two clocks (device and game); drift over a short intro is accepted.
