---
name: lax-test-json-readers
description: Test-only manifest readers (sound-sources, art-sources) build their own JsonSerializerOptions without required/nullable checks
metadata:
  type: feedback
---

Test-only readers of hand-written JSON (SoundSourcesTests, ArtSourcesManifest in AS1) build their own
`JsonSerializerOptions` with only `UnmappedMemberHandling.Disallow`: a missing field silently becomes null/false
(a missing `isPlaceholder` reads as shipping art), duplicate keys keep the last value.

**Why:** `Jrpg.Data.Content.ContentJsonOptions.Strict` (internal, visible to Data.Tests) already sets
RespectRequiredConstructorParameters, RespectNullableAnnotations, AllowDuplicateProperties=false and camelCase.

**How to apply:** when a new test reads a content-side JSON file, check it uses `ContentJsonOptions.Strict`; a bool
or string field that defaults silently is a Warning. SoundSourcesTests' copy predates AS1: Follow-up only.
