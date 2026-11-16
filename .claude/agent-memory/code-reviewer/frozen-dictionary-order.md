---
name: frozen-dictionary-order
description: ContentCatalog maps are FrozenDictionary; their Values order is not file order; tests with plain Dictionary hide it
metadata:
  type: project
---

ContentCatalog's maps (Classes, Enemies, ...) are built with `ToFrozenDictionary` in `ContentCatalogLoader`. Probed
2026-11-16 (SK5, dotnet fsi): small sets of mixed-length ids (today's 9 classes) come back in insertion order
(LengthBucketsFrozenDictionary), but six 4-letter ids `monk,bard,sage,mage,seer,hero` enumerate as
`monk,hero,bard,mage,seer,sage` (OrdinalStringFrozenDictionary_LeftJustifiedSubstring).

Seen twice: `Skills/ResourceChoice.AllIn` ("in the order the classes list them") and `Skills/SkillUsers.LearnedBy`.

**Why:** a test that builds the catalog `with { Classes = x.ToDictionary(...) }` keeps insertion order and passes,
while the loaded catalog may not.

**How to apply:** when code or a doc comment promises "file order" or "the order the classes list them" from a
ContentCatalog map's `.Values`, flag it (Warning) with the probe input above; suggest id order or an ordered source.
