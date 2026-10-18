namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// Tiles changed on one layer as a single undo step: a whole brush stroke or fill undoes at once, as in the legacy editor.
/// </summary>
/// <param name="LayerIndex">The layer's index in the draft, bottom first.</param>
/// <param name="Changes">Each cell at most once, with the tile it had before.</param>
internal sealed record SetTilesEdit(int LayerIndex, IReadOnlyList<TileChange> Changes) : IMapEdit
{
    public void Apply(MapDraft draft)
    {
        foreach (var change in Changes)
        {
            draft.SetTileId(LayerIndex, change.Cell, change.After);
        }
    }

    public void Revert(MapDraft draft)
    {
        foreach (var change in Changes)
        {
            draft.SetTileId(LayerIndex, change.Cell, change.Before);
        }
    }
}
