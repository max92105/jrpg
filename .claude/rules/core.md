---
paths:
  - "src/Jrpg.Core/**/*.cs"
---

# Rules for Jrpg.Core

- No `using Microsoft.Xna.*`, `using System.Windows.*`, or any rendering/input/audio namespace. If you need one, the design is wrong: expose state or an event instead.
- No file or network I/O here except through interfaces (`ISaveStore`, `IContentSource`) implemented elsewhere.
- No `async` in the simulation step. The game advances via `Update(TimeSpan delta)` or discrete commands.
- Battle and world logic must be runnable headless from a unit test with a fixed seed.
- Prefer returning results/events (`BattleEvent`, `DamageResult`) over side effects, so the renderer can animate them and tests can assert them.
- Collections exposed publicly are `IReadOnlyList<T>` / `IReadOnlyDictionary<K,V>`.
