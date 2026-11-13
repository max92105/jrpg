using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// Editing the draft's sprite records through the Character sprites window (decision 569): the window starts from the
/// draft as it is and its whole session is applied as one undo step, as <see cref="Quests.QuestEditingViewModel"/>.
/// </summary>
/// <param name="catalog">The records edited.</param>
/// <param name="listFiles">
/// The readable images in the field-sprite folder, read again each time the window opens.
/// </param>
/// <param name="sizeOf">An image's size by file name; null when the folder lacks it.</param>
internal sealed class FieldSpriteEditingViewModel(
    CatalogDraft catalog, Func<IReadOnlyList<string>> listFiles, Func<string, (int Width, int Height)?> sizeOf)
{
    /// <summary>
    /// The window's state from the draft as it is now, so a record undone or saved since is read right.
    /// </summary>
    public FieldSpriteCatalogViewModel Start() => new(catalog, listFiles, sizeOf);

    /// <summary>Gives the draft the window's changed records; nothing when none changed.</summary>
    public void Apply(FieldSpriteCatalogViewModel edited) => catalog.SetFieldSprites(edited.ToSheets());
}
