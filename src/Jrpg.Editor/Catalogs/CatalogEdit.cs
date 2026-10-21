using Jrpg.Core.Content;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// One catalog change that can be undone and redone (ADR 0014). Functions rather than snapshots, so undoing it changes
/// only its own records, never a map saved or a tile painted since.
/// </summary>
/// <param name="Apply">Makes the change on a catalog.</param>
/// <param name="Revert">Takes it back.</param>
internal sealed record CatalogEdit(Func<ContentCatalog, ContentCatalog> Apply, Func<ContentCatalog, ContentCatalog> Revert);
