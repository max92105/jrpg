using System.ComponentModel;

namespace Jrpg.Editor.Workspaces;

/// <summary>
/// A page's edited catalog as the Database mode sees it (design section 3 of the mode bar plan): Apply greys and a move
/// is refused on its problem; a page shown again, or on a record, starts on its key.
/// </summary>
internal interface IEditedCatalog : INotifyPropertyChanged
{
    /// <summary>Why the game would refuse the catalog as typed; null when it would take it.</summary>
    string? Problem { get; }

    /// <summary>The chosen record's id (its file for an image catalog); null with none chosen.</summary>
    string? ChosenKey { get; }

    /// <summary>Chooses the record with that key; an unknown key keeps the choice.</summary>
    void Choose(string key);
}
