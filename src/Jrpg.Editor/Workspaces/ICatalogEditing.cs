namespace Jrpg.Editor.Workspaces;

/// <summary>
/// Edits one catalog of the draft through its page (ADR 0060): a session starts from the draft as it is and is applied
/// as one undo step (decision 918). Each catalog's editing view model declares it, so the Database mode holds every
/// page in one list.
/// </summary>
/// <typeparam name="TEdited">The page's edited catalog.</typeparam>
internal interface ICatalogEditing<TEdited>
    where TEdited : IEditedCatalog
{
    /// <summary>The page's edited catalog from the draft as it is now.</summary>
    TEdited Start();

    /// <summary>Gives the draft the edited catalog as one undo step; nothing when it didn't change.</summary>
    void Apply(TEdited edited);
}
