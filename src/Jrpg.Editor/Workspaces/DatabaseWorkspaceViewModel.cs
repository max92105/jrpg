using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Workspaces;

/// <summary>
/// The Database mode's state (ADR 0060): its pages and the one shown, whose session alone holds unapplied edits
/// (decision 922). Apply records the session as one undo step (decision 918); any new catalog in the draft while a page
/// is shown, its own Apply included, starts the session again from the draft on the same record (decision 921), so an
/// added record's id is fixed once applied. Leaving the page applies it first, or is refused naming why (decision 919),
/// a refused page also named for the close question's list of unsaved work (decision 924).
/// </summary>
internal sealed partial class DatabaseWorkspaceViewModel : ObservableObject
{
    // Each page's chosen record when it was last left, so the mode reopens on it (decision 922).
    private readonly Dictionary<CatalogPage, string?> _chosenKeys = [];

    // The problem or the failure message the last refusal quotes; null whenever Refusal is.
    private string? _unappliedReason;

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

    /// <summary>
    /// Why the last move away from the shown page was refused; null once a move passes, and cleared when the page's
    /// problem or session changes, since it quotes them (decision 1008).
    /// </summary>
    [ObservableProperty]
    public partial string? Refusal { get; private set; }

    /// <summary>
    /// "the Shops page, not applied: {its problem or its apply's failure}", the last refusal as the close question and
    /// the unexpected-error box list it (decision 924), its last period left to the list's sentence; null when nothing
    /// was refused, cleared with <see cref="Refusal"/>.
    /// </summary>
    public string? UnappliedPageLine =>
        _unappliedReason is { } reason ? $"the {ShownPage!.Title} page, not applied: {reason.TrimEnd('.')}" : null;

    /// <summary>The page with that title; throws when the list has none.</summary>
    public CatalogPage PageTitled(string title) => Pages.Single(page => page.Title == title);

    public bool HasPage(string title) => Pages.Any(page => page.Title == title);

    /// <summary>Shows the last page shown on its last chosen record, the first page on the first visit.</summary>
    public void Open() => Open(ShownPage ?? Pages[0]);

    /// <summary>
    /// Shows that page on its record when last left, the Database mode being closed (nothing to apply).
    /// </summary>
    public void Open(CatalogPage page) => Show(page);

    /// <summary>
    /// Ends the shown page's session, applied first and its chosen record remembered; false when the page refused
    /// (<see cref="TryApplyShown"/>), its session kept. Nothing shown passes.
    /// </summary>
    public bool TryClose()
    {
        if (!TryLeaveShown())
        {
            return false;
        }

        Session = null;
        return true;
    }

    /// <summary>
    /// Applies the shown page before anything leaves it (decision 919): true once applied or with nothing shown; false
    /// with <see cref="Refusal"/> naming the page when its problem stops Apply or Apply throws (decision 939).
    /// </summary>
    public bool TryApplyShown()
    {
        Refusal = Session is { } session ? ApplyOrRefuse(session) : null;
        return Refusal is null;
    }

    /// <summary>
    /// Shows that page on the record with that key, its first record when none has it (an Edit... reaching another
    /// catalog, decision 928), the shown page applied first; refused, it stays.
    /// </summary>
    public void ShowPageOnRecord(CatalogPage page, string key)
    {
        if (!TryLeaveShown())
        {
            return;
        }

        _chosenKeys[page] = key;
        Show(page);
    }

    /// <summary>Shows the page on its record when last left, the shown page applied first; refused, it stays.</summary>
    [RelayCommand]
    private void ShowPage(CatalogPage page)
    {
        if (TryLeaveShown())
        {
            Show(page);
        }
    }

    // The draft's change starts the session again, so Apply needs no restart of its own.
    [RelayCommand(CanExecute = nameof(CanApply))]
    private void Apply() => Session!.Apply();

    private bool CanApply() => Session is { Edited.Problem: null };

    /// <summary>Drops what was typed: the session starts again from the draft on the same record.</summary>
    [RelayCommand]
    private void Revert() => StartAgain(Session!);

    // The refusal's words, or null once applied. An Apply that throws is a bug its problem line missed: refused rather
    // than thrown, so no move depends on it (decision 939).
    private string? ApplyOrRefuse(PageSession session)
    {
        var title = ShownPage!.Title;
        if (session.Edited.Problem is { } problem)
        {
            _unappliedReason = problem;
            return $"The {title} page has a problem: {problem} Fix it or press Revert.";
        }

        try
        {
            session.Apply();
            return null;
        }
        catch (Exception refused) when (refused is InvalidOperationException or ContentValidationException)
        {
            _unappliedReason = refused.Message;
            return $"The {title} page can't apply: {refused.Message}";
        }
    }

    partial void OnRefusalChanged(string? value)
    {
        if (value is null)
        {
            _unappliedReason = null;
        }
    }

    // The shown page applied, its chosen record remembered for its next showing; false when it refused.
    private bool TryLeaveShown()
    {
        if (!TryApplyShown())
        {
            return false;
        }

        if (Session is { } session)
        {
            _chosenKeys[ShownPage!] = session.Edited.ChosenKey;
        }

        return true;
    }

    private void Show(CatalogPage page)
    {
        ShownPage = page;
        Session = page.Start(_chosenKeys.GetValueOrDefault(page));
    }

    private void StartAgain(PageSession session) => Session = ShownPage!.Start(session.Edited.ChosenKey);

    partial void OnSessionChanged(PageSession? oldValue, PageSession? newValue)
    {
        Refusal = null;
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
            Refusal = null;
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
