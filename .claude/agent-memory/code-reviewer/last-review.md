---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 10 (Content cutover)
Round: 1 (verdict: Ready to commit; caller said this is the single review round)

- open (Warning): .gitattributes gives eol=crlf to layouts and catalogs but not content/maps/** (settings, editor-written per ADR 0011 A); with core.autocrlf=false they check out LF and 11a's identical-bytes save test / editor saves diverge
- open (Warning): ContentCatalogLoaderTests Load_RepositoryContent_LoadsEveryGeneratedTile / LoadsGeneratedObjectsWithTileGroupSizes still say "Generated" after content/generated/ is gone
- open (Suggestion): FontLibrary.FontsFolder = "art/fonts" duplicates the "art" now owned by ArtFolders.RootIn; add ArtFolders.Fonts and build the path from RootIn
- open (Suggestion): tick slice 10 in the plan and note the deviations (ArtFolders.RootIn instead of path constants in ContentRootLocator, tools/Directory.Build.props deleted)
- follow-up: ADR 0009 "Until the cutover (slice 10)" bullets now describe the past; mark them done or point to ADR 0011
