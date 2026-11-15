# 0050. Unexpected errors in the editor

Status: accepted
Date: 2026-11-15

## Context
The editor had no handler for an exception nothing caught: any throw on its UI thread closed it and lost every unsaved
map, catalog change and undo step. A hand check lost a draft that way (the Cinematics window threw as it opened). The
developer works for hours in one session, and the editor is about to carry all content work (plan
`2026-11-15-editor-push`, decisions 669-674, 690-692). Some throws come back every frame: the map drawing and five
windows run on `CompositionTarget.Rendering`.

## Options
### A. Keep the editor open and ask (chosen)
The error is written to a report file; a box names the error, the report and what is unsaved; the developer keeps
working or closes through the usual close question.
- Pros: the draft, the open windows and the undo history survive; Save all stays one keystroke away; nothing is
  written to `content/` that the developer didn't ask for.
- Cons: the editor runs on after a command stopped halfway, so its state may be off; the box says to save and
  restart if something looks wrong.

### B. Offer Save all, then close
- Pros: never runs on a doubtful state.
- Cons: a save the validator refuses loses the draft anyway; an error in a preview closes hours of work for nothing.

### C. Keep open, with an autosave of the draft beside the content
- Pros: survives a crash the handler can't catch (a stack overflow, a power cut).
- Cons: a second copy of the content to write, find and reconcile at startup; a draft the validator refuses can't be
  written in the game's format (ADR 0011), so it would need its own.

## Decision
A: an exception nothing caught on the UI thread, once the main window has loaded, is written and asked about, and the
editor stays open (the developer: "yes always try to save progress"). The same error again (same exception type and
throwing method) is counted, never asked about and never written twice, so a throw in a per-frame tick costs one box
and Save all stays reachable. While the box is open, a different error is written without a second box.

## Consequences
- Reports go to `%LocalAppData%\Jrpg\Editor\Errors\` and to the error output; on exit a report gains how many times
  its error came back.
- An error before the main window has loaded is written and still ends the editor: there is no draft to keep yet.
- Closing from the box goes through Save all / Don't save / Cancel; there is no emergency write and no autosave. If
  crashes the handler can't catch ever cost work, option C is the next step.
- Errors on other threads aren't handled: the editor has none. The first background thread brings its own handling.
- Help > Raise a test error throws on purpose, in every build, for the hand check and for
  `.claude/scripts/open-editor-windows.ps1`, which now reports a window whose opening raised the box.
- The rules that said "the editor has no unhandled-exception handler" now say what a throw does: it stops the command
  halfway and opens the box.
