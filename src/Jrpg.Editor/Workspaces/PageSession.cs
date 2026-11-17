namespace Jrpg.Editor.Workspaces;

/// <summary>
/// One shown page's edited catalog with its apply (ADR 0060): what the Database mode holds unapplied, its whole session
/// one undo step (decision 918).
/// </summary>
internal sealed class PageSession(IEditedCatalog edited, Action apply)
{
    public IEditedCatalog Edited { get; } = edited;

    /// <summary>Gives the draft the edited catalog as one undo step; nothing when it didn't change.</summary>
    public void Apply() => apply();
}
