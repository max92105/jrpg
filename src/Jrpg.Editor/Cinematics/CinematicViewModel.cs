using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Cinematics;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Cinematics;

/// <summary>
/// One cinematic as the Cinematics page edits it (ADR 0046): its id (typed while the cinematic is new, fixed once the
/// draft holds it), its track and its still rows in the order they show, numbered from 1, added after the chosen one
/// from the picture box, removed and moved. <see cref="Problem"/> is the typed id's, a missing still or a row's box
/// that can't be read; the rest is Data's, shown by the list. <see cref="StillsChanged"/> tells the list of each change
/// to the stills, typed or listed.
/// </summary>
internal sealed partial class CinematicViewModel : ObservableObject
{
    private CinematicViewModel(string id, IEnumerable<CinematicStillRow> stills)
    {
        Id = id;
        Stills = [.. stills];
        foreach (var row in Stills)
        {
            WatchRow(row);
        }

        Stills.CollectionChanged += OnStillsListChanged;
        Renumber();
        SelectedStill = Stills.FirstOrDefault();
    }

    /// <summary>
    /// A row's box typed, or a row added, removed or moved; never a row's own label, number or problem notice, so the
    /// list re-checks once per change (<c>.claude/rules/editor.md</c>).
    /// </summary>
    public event EventHandler? StillsChanged;

    /// <param name="cinematic">The cinematic as the draft holds it.</param>
    public static CinematicViewModel From(CinematicDefinition cinematic) =>
        new(cinematic.Id, cinematic.Stills.Select(CinematicStillRow.From)) { MusicFile = cinematic.MusicFile ?? "" };

    /// <summary>A cinematic with nothing typed yet.</summary>
    public static CinematicViewModel New() => new("", []) { IsNew = true };

    /// <summary>True while the cinematic was added this session: the only time its id can be typed.</summary>
    public bool IsNew { get; private init; }

    public ObservableCollection<CinematicStillRow> Stills { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string Id { get; set; }

    /// <summary>The track under the whole cinematic (decision 614); empty keeps the map's.</summary>
    [ObservableProperty]
    public partial string MusicFile { get; set; } = "";

    /// <summary>
    /// A voice line's playing length by its name (decision 620), handed to every row for its timing line; the list sets
    /// it from the page's choices.
    /// </summary>
    [ObservableProperty]
    public partial Func<string, TimeSpan?> VoiceLengthOf { get; set; } = _ => null;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveStillCommand), nameof(MoveStillUpCommand), nameof(MoveStillDownCommand))]
    public partial CinematicStillRow? SelectedStill { get; set; }

    /// <summary>The picture box's choice, added by Add; null while nothing is picked.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddStillCommand))]
    public partial string? ImageToAdd { get; set; }

    /// <summary>The list's line: the id, or a placeholder while a new cinematic has none.</summary>
    public string Label => Id.Length > 0 ? Id : "(new cinematic)";

    /// <summary>
    /// Why the cinematic can't be built: the typed id while new, no still, then the first row's problem; null when
    /// nothing keeps it from being built.
    /// </summary>
    public string? Problem =>
        (IsNew ? ObjectIds.ProblemOf(Id, []) : null)
        ?? (Stills.Count == 0 ? "Add at least one still." : null)
        ?? Stills.Select(row => row.Problem).FirstOrDefault(problem => problem is not null);

    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public CinematicDefinition ToCinematic() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : new CinematicDefinition
            {
                Id = Id,
                MusicFile = MusicFile.Length > 0 ? MusicFile : null,
                Stills = [.. Stills.Select(row => row.ToStill())],
            };

    // After the chosen still, or last when none is chosen; a picture may show twice.
    [RelayCommand(CanExecute = nameof(HasImageToAdd))]
    private void AddStill()
    {
        var row = CinematicStillRow.New(ImageToAdd!);
        Stills.Insert(SelectedStill is null ? Stills.Count : Stills.IndexOf(SelectedStill) + 1, row);
        SelectedStill = row;
    }

    private bool HasImageToAdd() => ImageToAdd is not null;

    [RelayCommand(CanExecute = nameof(HasSelectedStill))]
    private void RemoveStill() => SelectedStill = ListOrder.Remove(Stills, SelectedStill!);

    [RelayCommand(CanExecute = nameof(HasSelectedStill))]
    private void MoveStillUp() => ListOrder.MoveBy(Stills, SelectedStill!, -1);

    [RelayCommand(CanExecute = nameof(HasSelectedStill))]
    private void MoveStillDown() => ListOrder.MoveBy(Stills, SelectedStill!, 1);

    private bool HasSelectedStill() => SelectedStill is not null;

    // Each row shows its place, so any change to the list renumbers.
    private void OnStillsListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ListItemWatch.Follow<CinematicStillRow>(e, WatchRow, row => row.Changed -= OnRowChanged);
        Renumber();
        OnPropertyChanged(nameof(Problem));
        StillsChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnVoiceLengthOfChanged(Func<string, TimeSpan?> value)
    {
        foreach (var row in Stills)
        {
            row.VoiceLengthOf = value;
        }
    }

    private void WatchRow(CinematicStillRow row)
    {
        row.Changed += OnRowChanged;
        row.VoiceLengthOf = VoiceLengthOf;
    }

    private void OnRowChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(Problem));
        StillsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Renumber()
    {
        for (var index = 0; index < Stills.Count; index++)
        {
            Stills[index].Number = index + 1;
        }
    }
}
