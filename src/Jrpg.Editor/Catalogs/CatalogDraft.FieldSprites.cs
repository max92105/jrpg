using Jrpg.Core.Sprites;

namespace Jrpg.Editor.Catalogs;

/// <summary>The field sprite records (ADR 0043), edited by the Character sprites window (decision 569).</summary>
internal sealed partial class CatalogDraft
{
    /// <summary>
    /// Adds or replaces the given records, by file, as one undo step; the records not given stay (a saved record is
    /// never removed, decision 569); nothing when every given record equals the current one. No undo refusal: only
    /// objects name a record, and they are catalog steps undone first.
    /// </summary>
    public void SetFieldSprites(IReadOnlyList<FieldSpriteSheet> changed)
    {
        var before = Current.FieldSprites;
        if (changed.All(sheet => before.TryGetValue(sheet.File, out var current) && current == sheet))
        {
            return;
        }

        var after = new Dictionary<string, FieldSpriteSheet>(before, StringComparer.Ordinal);
        foreach (var sheet in changed)
        {
            after[sheet.File] = sheet;
        }

        Do(new CatalogEdit(catalog => catalog with { FieldSprites = after }, catalog => catalog with { FieldSprites = before }));
    }
}
