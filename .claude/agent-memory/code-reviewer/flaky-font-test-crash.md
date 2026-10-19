---
name: flaky-font-test-crash
description: Game test host can crash (AccessViolation), hang, or fail an unrelated test (e.g. ExplorationSceneTests.Start) after FontLibraryTests.Load_FileThatIsNotAFont; rerun before blaming a slice
metadata:
  type: project
---

`tests/Jrpg.Game.Tests/Rendering/FontLibraryTests.Load_FileThatIsNotAFont_ThrowsNamingTheFile` hands StbTrueTypeSharp a 23-byte non-font; `stbtt__find_table` reads past the buffer, so the whole Game test host intermittently dies with AccessViolationException (seen 2026-10-19, M3b review: 1 crash + 1 hang in ~14 runs). M3c review (2026-10-19): one host crash ("error: 1", total drops) and one failure of `ExplorationSceneTests.Update_DuringWarpFadeIn_DoesNotWalk` inside `Start` (font/screen text construction) in 2 full runs; 5 Game-only reruns clean.

**Why:** a native out-of-bounds read depends on heap layout, so it looks like "1 failed" in an unrelated slice, and the failing test varies.
**How to apply:** when `dotnet test` reports a Game.Tests host failure or an odd failure in text/font construction, rerun `--project tests/Jrpg.Game.Tests` a few times; report it as a follow-up (guard the header size before AddFont) unless the slice touched FontLibrary.
