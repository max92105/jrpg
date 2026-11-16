---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice SK1 (writer takes over skills.json, effects.json, statuses.json;
relay commit 1296b8da; code uncommitted). 2026-11-16. Round 1. Verdict: Ready to commit. No Critical.

- Warning, open: ContentWriter.Catalogs.cs CheckCatalogs: CheckTieredEffects, CheckStatuses and SkillCatalogValidator
  (inside CheckSkills) have no writer test; only the classes.json and enemies.json refusals are tested. Proposed one
  refusal test per file (statuses.json, effects.json, skills.json), each asserting the file name and no file changed.
- Suggestion, open: delete ContentWriterRepositoryTests.Effects_/Statuses_RepositoryFile_WritesBackUnchanged
  (identical-bytes test now covers both through the writer's own sort; "row 7's window" comments stale).
- Suggestion, open: CatalogProblems.CheckSkills doc's "the enemies' check runs with the catalog's own" is opaque.
- Checked, no finding: Effect, StatusDefinition override Equals (list members by content), so the reload tests'
  Assert.Equal is meaningful; test-world skills.json is the live content's; stale tier/status records after an
  effects/statuses edit are decision 975's (rebind), not SK1's.
