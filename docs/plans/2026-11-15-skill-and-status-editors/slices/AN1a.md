# AN1a. The preview's playback rules

Editor, about 140 lines (about 80 of them tests). Decision 986. Design: design.md, section 2.6 ("The shared preview").
Needs nothing of SK or FX.

## Goal
The rules of the shared preview, headless, before the control that draws them (AN1b): which frame shows for a
playing preview at a frame time, Pause holding it, Step forward and Step back moving one frame and pausing, a new
frame time keeping the shown frame, a frame time that isn't usable holding the frame, and the frame line ("frame 3 of
8", ", the hit"). Core's `TileAnimation.FrameAt` is the frame rule, as for every editor animation.

## New types
- `Editor/Animation/FramePlayback` (internal sealed partial, `ObservableObject`): which frame a preview shows.
  - `Show(int frameCount)`: frame 0, playing; `FrameCount`.
  - `FrameMilliseconds` (`int?`, set by the host's typed box through the control; null or below 1 holds the shown
    frame); `HitFrame` (`int?`). A new `FrameMilliseconds` sets the elapsed time to the start of the shown frame.
  - `Advance(TimeSpan delta)`: throws `ArgumentOutOfRangeException` on a negative delta; while playing, with frames and
    a frame time, moves its own elapsed time (kept within one loop) and shows `TileAnimation.FrameAt` of it.
  - `ShownFrame`, `IsPlaying`, `FrameLine` ("frame 3 of 8", ", the hit" on `HitFrame`; empty with no frames), each
    raised when it changes.
  - `TogglePlayingCommand`, `StepForwardCommand`, `StepBackCommand` (pause, then one frame on or back, wrapping; greyed
    with no frames).

## Tests
- `Editor.Tests/Animation/FramePlaybackTests` (new):
  - `Advance_PlayingAtHundredMs_ShowsTheFrameOfTheElapsedTime`.
  - `Advance_PastTheLastFrame_Wraps`.
  - `Advance_NegativeDelta_Throws`.
  - `Advance_Paused_HoldsTheFrame`.
  - `Advance_NoFrameTime_HoldsTheFrame`.
  - `StepForward_LastFrame_WrapsToFirstAndPauses`; `StepBack_FirstFrame_WrapsToLast`.
  - `FrameMilliseconds_ChangedWhilePlaying_KeepsTheShownFrame`.
  - `Show_NewFrames_StartsAtZeroPlaying`.
  - `FrameLine_OnTheHitFrame_SaysTheHit`; `FrameLine_NoFrames_IsEmpty`.

## Verify by hand
None: the control comes in AN1b.
