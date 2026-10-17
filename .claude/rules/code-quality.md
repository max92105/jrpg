---
paths:
  - "**/*.cs"
---

# Code quality standard

The bar: a professional reviewer reads the diff and understands the intent from names and structure alone, without asking. Code that only "works" is not done.

## Naming

- A type's name says its single responsibility: `TurnOrder`, `DamageCalculator`, `InventorySlot`. If you can't name it without "and", split it.
- Banned as a type-name suffix unless nothing more precise exists: `Manager`, `Helper`, `Utils`, `Util`, `Handler`, `Processor`, `Data`, `Info`, `Base`, `Common`. `BattleManager` should be `Battle` (what it is) or `TurnOrder` + `BattleRewards` (what it does).
- Methods are verb phrases that say what happens: `ApplyDamage`, `TryConsume`, `CalculateHitChance`. Queries read as values: `AliveEnemies`, `IsDefeated`.
- Booleans start with `Is`, `Has`, `Can`, `Should`. No negatives (`IsNotDead` → `IsAlive`).
- No abbreviations except domain terms everyone in the genre knows (`Hp`, `Mp`, `Xp`, `Atk`). Write them as words in PascalCase: `MaxHp`, not `MaxHP` or `mHP`.
- Same concept, same word, everywhere. Pick `Party` or `Team`, `Enemy` or `Monster`, once.
- `Try...` methods return `bool` and never throw for the expected failure.

## Structure

- Organize by feature, not by kind: `Battle/TurnOrder.cs`, `Battle/DamageCalculator.cs`. No top-level `Models/`, `Enums/`, `Managers/`, `Utils/` folders.
- One top-level type per file; file name = type name; namespace = folder path (enforced by IDE0130).
- Types are `internal sealed` by default. Make them `public` only when another project needs them; unseal only for a real subtype.
- Constructor injection for dependencies. A constructor with more than 4 dependencies means the class does too much.
- Prefer composition over inheritance. Inheritance only for true "is-a" with shared behavior, max one level deep in Core.
- An interface needs a reason: two implementations, or a seam for I/O, time or randomness. Not "in case".

## Methods

- Aim for under ~20 lines and one level of abstraction. Cyclomatic complexity above 10 fails the build (CA1502).
- Guard clauses and early returns instead of nested `if`. Max 2 levels of nesting.
- Max 4 parameters. More → introduce a parameter object that is a real concept (`AttackContext`), not a bag.
- No boolean flag parameters that switch behavior (`Move(true)`). Make two methods.
- Command or query, not both: a method either changes state or returns information.
- No magic numbers: name the constant, or move it to content JSON if it's a design value.

## Comments

- Comments explain *why* (a non-obvious rule, a trade-off, a source for a formula). Never *what*; rename or extract instead.
- No commented-out code, no narration (`// Loop through enemies`), no section banners, no "Step 1/Step 2".
- No `TODO` without a plan reference: `// TODO(plan 2026-10-20-status-effects, slice 3): ...`.
- XML docs only where the signature can't carry the meaning: units, ranges, invariants, formula sources. Never restate the name (`/// Gets the HP.`).

## Errors

- Validate at boundaries (content loading, save loading, editor input) and fail with a specific message naming the bad ID or field.
- Inside Core, trust non-nullable types. Don't add null checks the compiler already guarantees.
- Never swallow exceptions. A `catch` handles, translates with context, or isn't there.
- Throw specific exceptions (`ArgumentOutOfRangeException`, `InvalidOperationException`), never bare `Exception`.

## What AI-generated code tends to add — don't

- Wrapper methods that only forward a call.
- Defensive code for states that can't happen.
- Interfaces, factories, or generics with one user.
- Unused parameters, fields, usings, or "for later" members (build fails on these).
- Logging or `try/catch` added "to be safe".
- Re-implementing something that already exists in the codebase. Search first, reuse it.
- Renaming or reformatting code outside the slice.

When existing code breaks these rules, follow the rules in new code and note the old code as a follow-up; don't refactor it in the same slice.
