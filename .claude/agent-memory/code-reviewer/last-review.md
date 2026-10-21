---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/E3c.md, E3c (the catalog writer, ContentWriter.WriteMapCatalogs)
Round: 1 (verdict: Ready to commit; verify all pass, 1338 tests)

- open (Warning): ContentWriter.Aside<T> writes nothing, it pairs a path with a write action; name reads as "write aside" at call sites. Rename (PendingWrite/FileToWrite) or small private record.
- open (Suggestion): no test that an art problem (not a validator) also writes nothing; TileOutsideItsImage only checks the message.
- open (Suggestion): (string Path, Action<string> WriteTo) tuple spelled twice; a private record FileToWrite would name it.
- open (Follow-up): WriteAll's move loop isn't atomic across files (a failed 2nd/3rd move leaves earlier files replaced and .saving leftovers); its comment only covers the write phase. Predates (WriteMap).
- open (Follow-up): WriteAll's catch File.Delete throws DirectoryNotFoundException for a later file whose folder wasn't created yet, masking the original error (WriteMap layout/settings folders). Predates.
