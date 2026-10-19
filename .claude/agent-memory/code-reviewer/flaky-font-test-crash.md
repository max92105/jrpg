---
name: flaky-font-test-crash
description: Game test host can crash (AccessViolation) or hang in FontLibraryTests.Load_FileThatIsNotAFont; rerun before blaming a slice
metadata:
  type: project
---

`tests/Jrpg.Game.Tests/Rendering/FontLibraryTests.Load_FileThatIsNotAFont_ThrowsNamingTheFile` hands StbTrueTypeSharp a 23-byte non-font; `stbtt__find_table` reads past the buffer, so the whole Game test host intermittently dies with AccessViolationException (seen 2026-10-19, M3b review: 1 crash + 1 hang in ~14 runs).

**Why:** a native out-of-bounds read depends on heap layout, so it looks like "1 failed" in an unrelated slice.
**How to apply:** when `dotnet test` reports a Game.Tests host failure, check the stack for this test and rerun `--project tests/Jrpg.Game.Tests --no-build`; report it as a follow-up (guard the header size before AddFont) unless the slice touched FontLibrary.
