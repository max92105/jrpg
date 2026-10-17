---
name: namespace-cycles
description: Core namespace dependency direction - Content is the hub that references feature namespaces; a feature type that takes ContentCatalog while Content references that feature creates a cycle
metadata:
  type: feedback
---

The developer cares about avoiding namespace cycles in Core (slice 8 placed FieldItemUse in Party/ to avoid Items <-> Party). Yet the same slice created Content <-> Session: `ContentCatalog.NewGame` uses `Session.NewGameDefinition` while `Session.GameSession.StartNew` takes `ContentCatalog`.

**Why:** Cycles defeat the feature-folder layering; the build doesn't catch them.

**How to apply:** For every new `using Jrpg.Core.X` in Core, grep whether X already uses the current namespace. Content definitions that the catalog holds and that a consumer of the catalog also defines belong in Content/ (like GameRules). Related: [[validator-null-ids]].
