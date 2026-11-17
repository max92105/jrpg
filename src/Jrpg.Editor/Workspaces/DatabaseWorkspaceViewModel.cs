using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Workspaces;

/// <summary>
/// The Database mode's state (ADR 0060): its pages and the one shown, whose session alone holds unapplied edits
/// (decision 922). Apply records the session as one undo step (decision 918); any new catalog in the draft while a page
/// is shown, its own Apply included, starts the session again from the draft on the same record (decision 921), so an
/// added record's id is fixed once applied.
/// </summary>
internal sealed partial class DatabaseWorkspaceViewModel : ObservableObject
{
    // Each page's chosen record when it was last left, so the mode reopens on it (decision 922).
    private readonly Dictionary<CatalogPage, string?> _chosenKeys = [];

    /// <param name="draft">The catalogs the pages edit, listened to for the restart.</param>
    /// <param name="pages">The list's pages in order; the first is shown on the first visit.</param>
    public DatabaseWorkspaceViewModel(CatalogDraft draft, IReadOnlyList<CatalogPage> pages)
    {
        Pages = pages;
        draft.PropertyChanged += OnDraftChanged;
    }

    public IReadOnlyList<CatalogPage> Pages { get; }

    /// <summary>The page the Database mode shows, kept while World is shown so the mode reopens on it.</summary>
    [ObservableProperty]
    public partial CatalogPage? ShownPage { get; private set; }

    /// <summary>The shown page's edited catalog; null while the Database mode is closed.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    public partial PageSession? Session { get; private set; }

    /// <summary>Shows the last page shown on its last chosen record, the first page on the first visit.</summary>
    public void Open()
    {
        ShownPage ??= Pages[0];
        Session = ShownPage.Start(_chosenKeys.GetValueOrDefault(ShownPage));
    }

    /// <summary>
    /// Ends the shown page's session, remembering its chosen record; what was typed and not applied goes.
    /// </summary>
    public void Close()
    {
        _chosenKeys[ShownPage!] = Session!.Edited.ChosenKey;
        Session = null;
    }

    // The draft's change starts the session again, so Apply needs no restart of its own.
    [RelayCommand(CanExecute = nameof(CanApply))]
    private void Apply() => Session!.Apply();

    private bool CanApply() => Session is { Edited.Problem: null };

    /// <summary>Drops what was typed: the session starts again from the draft on the same record.</summary>
    [RelayCommand]
    private void Revert() => StartAgain(Session!);

    private void StartAgain(PageSession session) => Session = ShownPage!.Start(session.Edited.ChosenKey);

    partial void OnSessionChanged(PageSession? oldValue, PageSession? newValue)
    {
        if (oldValue is not null)
        {
            oldValue.Edited.PropertyChanged -= OnEditedChanged;
        }

        if (newValue is not null)
        {
            newValue.Edited.PropertyChanged += OnEditedChanged;
        }
    }

    private void OnEditedChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IEditedCatalog.Problem))
        {
            ApplyCommand.NotifyCanExecuteChanged();
        }
    }

    // Only a new catalog starts the session again; the saved mark (IsDirty) changes nothing the page shows.
    private void OnDraftChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CatalogDraft.Current) && Session is { } session)
        {
            StartAgain(session);
        }
    }
}
