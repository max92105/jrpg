namespace Jrpg.Editor.Workspaces;

/// <summary>
/// One entry of the Database mode's page list (decision 917): its title, and how its session starts from the draft, so
/// the list holds pages of every catalog type alike.
/// </summary>
internal sealed class CatalogPage
{
    private readonly Func<string?, PageSession> _start;

    private CatalogPage(string title, Func<string?, PageSession> start)
    {
        Title = title;
        _start = start;
    }

    /// <summary>The page's name in the list and the Database menu, as the menu entry's <c>Tag</c> spells it.</summary>
    public string Title { get; }

    /// <summary>
    /// A page whose session is the editing view model's own: started from the draft, applied as one step.
    /// </summary>
    public static CatalogPage Of<TEdited>(string title, ICatalogEditing<TEdited> editing)
        where TEdited : IEditedCatalog =>
        new(title, chosenKey =>
        {
            var edited = editing.Start();
            if (chosenKey is not null)
            {
                edited.Choose(chosenKey);
            }

            return new PageSession(edited, () => editing.Apply(edited));
        });

    /// <summary>
    /// A session from the draft as it is now, on the record with that key when given (the first one when the draft no
    /// longer holds it, as <see cref="IEditedCatalog.Choose"/> keeps the choice).
    /// </summary>
    public PageSession Start(string? chosenKey) => _start(chosenKey);
}
