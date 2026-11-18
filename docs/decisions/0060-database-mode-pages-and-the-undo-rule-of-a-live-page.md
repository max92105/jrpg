# 0060. Database mode pages and the undo rule of a live page

Status: Accepted (default taken while the developer was away; his to overturn)
Date: 2026-11-17

## Context
The editor is more than a map editor now: the developer asked for modes and for more screen space. The mode bar plan
(`docs/plans/done/2026-11-15-mode-bar/`) gives the main window two modes, World (today's map editor) and Database, where a
list of catalog pages sits on the left and the chosen page fills the rest. Until then every catalog editor was a
modal window, then a page inside the modal `CatalogDialog` (stages 1 and 2), and ADR 0045's rules leaned on that:
a window applies its whole session as one `CatalogDraft` step, so nothing can change the draft under it and it needs
no re-check (decision 591); a window removes only the records added in its session (decision 581); an id is typed
while the record is new and fixed once the draft holds it (decision 582). The one exception was ADR 0045's amendment B
(decision 602): the Enemies window's Edit... opened the Drop tables window over it, whose OK applied its own step,
and the Enemies window then read the draft again.

A page in Database mode is not modal: while it holds typed changes the developer can show another page, go back to
World, press Ctrl+Z, Ctrl+Y or Ctrl+S, or close the editor. Each of these could change the draft under the page or
leave it, so the page needs an undo rule of its own. Constraints: never lose typed work, never ask in flow (the
developer dislikes prompts while working, decision 497, and is asked once, on close); about twenty page view models by
the editor push's row 12, so a rule that rewrites each one costs per page; the rule must be testable headless through
`DatabaseWorkspaceViewModel` and `MainViewModel`; no Core, Data, content or save change. In Database mode the map is
off (decision 925: the World menus, the tool bar, the map keys and the history's reach into the shown map), so the
entry points that can change the draft are few and named. The developer delegated the plan's approval, so this choice
was taken while he was away.

## Options
The plan's design.md ("The undo rule") holds the entry-point table.

### A. Apply on leave: a page's session is one undo step, applied before anything that leaves it (chosen)
- Pros: ADR 0045's one step per session and its id and removal rules kept as they are; a page view model gains only
  the three `IEditedCatalog` members, so pages are written once for both hosts; no question asked; a problem refuses
  the move by name, as Save already refuses; one invariant to test, a test per entry point. About 720 lines over three
  slices (WS2-WS4) before a page can be typed into in Database mode.
- Cons: every entry point that could change the draft must call `TryApplyShown` first, or the restart drops the typing
  silently; Ctrl+Z on typed changes applies them and takes that step back at once, and Ctrl+Y with typed changes only
  applies them, dropping the redo; a page with a problem blocks leaving it until fixed or reverted; an added record's
  id is fixed at the first save, undo or leave rather than at an OK.

### B. Per-edit undo steps
Every typed change applied to the draft live, one undo step each.
- Pros: Ctrl+Z takes back one typed change, as on the map; no pending state, so no leave rule.
- Cons: every page view model rewritten to apply its boxes live; a typed id is invalid between keystrokes, so the draft
  would hold or refuse half-typed ids; about 8 more slices.

### C. A question on leaving a page with unapplied changes
- Pros: the developer chooses each time between applying and dropping; no surprise undo step.
- Cons: a prompt in flow on every page switch, undo, save or mode change with typed work (decision 497).

### D. Each page keeps its session across switches, as open maps do
- Pros: switching pages is free; typed work waits on its page.
- Cons: an undo, a redo or another page's apply changes the draft under a hidden session, which is then stale: drop
  its typing silently or ask, the two things this rule exists to avoid.

### E. Stay at stage 1's dialogs
- Pros: no new undo rule; ADR 0045 unchanged.
- Cons: the screen space without the mode bar the developer asked for; a window to open and close for every catalog.

## Decision
Option A (decisions 918-924, 928, 931 and 939; options B to E set aside by decision 931).
- **The page (918).** A page is a user control over its catalog view model (`IEditedCatalog`), hosted by
  `CatalogDialog` (stages 1 and 2, and outside Database mode until WS6) or the Database view. In Database mode it starts
  from the draft when shown; Apply records its whole session as one undo step (ADR 0045's one step per session kept)
  and the page starts again from the draft on the same record, so an added record's id is fixed at its first save,
  undo or leave (decision 582) and only records added since can be removed (decision 581).
- **Leaving applies (919).** Showing another page, World mode, Undo, Redo, Save, Save all, closing the editor, and any
  dialog opened over the Database mode that changes the draft apply the shown page first. A page whose problem stops
  Apply refuses the move, naming the page and its problem ("The Shops page has a problem: ... Fix it or press
  Revert."); Revert drops what was typed. No question is asked.
- **Undo and redo (920).** Ctrl+Z on a page with typed changes applies them and takes that step back at once; Ctrl+Y
  with typed changes only applies them, which records a new step and so drops the redo, as any edit does. Inside a
  focused text box Ctrl+Z stays the box's own. With nothing in the history Ctrl+Z is greyed and the typed changes stay.
- **The restart (921).** Any change of the draft while a page is shown (its own Apply, an undo, a redo, a save) starts
  its session again from the draft, the same record chosen (the first when it is gone); by decision 919 nothing typed
  is pending at that moment. This replaces, in Database mode, decision 591's "no re-check": the page never re-checks
  the draft under a session, it starts again.
- **One session at a time (922).** Only the shown page holds unapplied edits; World mode ends the session; Database
  mode reopens the last page shown on its last chosen record (the list's first page on the first visit). The editor
  starts in World mode; neither is remembered between runs.
- **Save (923).** Save and Save all apply the shown page first; a page that can't apply stops them with nothing
  written, the save line naming the page and why.
- **Close (924).** The close question and the unexpected-error box (ADR 0050) list a page that can't apply ("the Shops
  page, not applied: ..."); its Save all then refuses as in decision 923 and the editor stays open; Don't save closes
  without running Apply again. A page with valid typed changes is applied before the question, so it counts as the
  catalogs' unsaved change.
- **Edit... to another catalog (928).** From WS6 the Enemies page's Edit... beside the Drop table box shows the Drop
  tables page on the enemy's table, the Enemies page applied first (or the move refused); it is the only Edit...
  reaching another catalog, the Skills page's strip being a box (WS1). The nested window and its refresh go, since the Enemies page starts again from the draft when
  shown: **this retires ADR 0045's amendment B (decision 602)**, done in WS6.
- **A throwing Apply (939).** An Apply that throws (a `To...` on a state the problem line missed, Data's check) is
  turned into a refusal by `TryApplyShown`, "The Shops page can't apply: {message}", catching the two exceptions an
  apply raises (`InvalidOperationException`, `ContentValidationException`), so no move, save or close depends on it
  succeeding; the error is still a bug to fix.

## Consequences
- **Easier:** a catalog fills the main window with no window to open or close; a new catalog editor is a page written
  once ([page-recipe.md](../plans/done/2026-11-15-mode-bar/page-recipe.md)), its view model unchanged by the mode bar.
- **The invariant to keep:** in Database mode only the shown page holds unapplied edits, and nothing changes the draft
  while it holds them. Any new entry point that changes the draft in Database mode (a menu entry, a key, a dialog, a
  page command reaching outside its session) calls `TryApplyShown` first, with its test; one that skips it drops the
  page's typing without a word.
- **The developer will notice:** Ctrl+Z after typing takes back the whole page's session, not the last box; a page with
  a problem can't be left until fixed or reverted; the view is rebuilt on every restart, so a page loses its scroll
  after Apply, an undo or a save.
- **Watch for:** typed boxes bound with the default `LostFocus` trigger would not reach the view model before Ctrl+Z or
  a mode switch applies the page (the recipe requires `PropertyChanged`); code after `TryApplyShown` in a page's
  handler runs on a page no longer shown (decision 940).
- No save format, content or Core change; determinism unaffected.
- If the developer overturns the rule: a question on leave (option C, the alternative he is most likely to prefer)
  changes WS3 and WS4 only; per-edit steps (option B) rewrite every page view model.
- Amends ADR 0045: decision 591's "no re-check" becomes the restart in Database mode, and amendment B (decision 602)
  is retired (done in WS6).
