using System.Windows;

namespace Jrpg.Editor.Workspaces;

/// <summary>
/// How the Database mode builds the view of its shown page, by the type of the page's edited catalog: the pages know
/// nothing of their host, so the main window registers each one's builder with the callbacks it needs.
/// </summary>
internal sealed class DatabasePageViews
{
    private readonly Dictionary<Type, Func<IEditedCatalog, FrameworkElement>> _builders = [];

    public void Register<TEdited>(Func<TEdited, FrameworkElement> build)
        where TEdited : IEditedCatalog =>
        _builders.Add(typeof(TEdited), edited => build((TEdited)edited));

    /// <summary>A new view over the edited catalog; throws when no builder is registered for its type.</summary>
    public FrameworkElement ViewOf(IEditedCatalog edited) =>
        _builders.TryGetValue(edited.GetType(), out var build)
            ? build(edited)
            : throw new InvalidOperationException($"No Database page view is registered for {edited.GetType().Name}.");
}
