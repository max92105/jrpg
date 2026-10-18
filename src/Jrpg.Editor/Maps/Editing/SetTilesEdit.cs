namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// Tiles changed on one layer as a single undo step: a whole brush stroke or fill undoes at once, as in the legacy editor.
/// </summary>
/// <param name="LayerZ">The layer's Z.</param>
/// <param name="Changes">Each cell at most once, with the tile it had before.</param>
internal sealed record SetTilesEdit(int LayerZ, IReadOnlyList<TileChange> Changes) : IMapEdit
{
    public void Apply(MapDraft draft)
    {
        foreach (var change in Changes)
        {
            draft.SetTileId(LayerZ, change.Cell, change.After);
        }
    }

    public void Revert(MapDraft draft)
    {
        foreach (var change in Changes)
        {
            draft.SetTileId(LayerZ, change.Cell, change.Before);
        }
    }
}
