using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Cinematics;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;
using Jrpg.Editor.Objects;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Cinematics;

/// <summary>
/// Every cinematic as the Cinematics page edits it (ADR 0046): the draft's cinematics in id order, the ones added this
/// session after them, each added under the id typed in the box; only an added one can be removed (as decision 581
/// keeps a shop). <see cref="Problem"/> is a cinematic's own problem first, then Data's message for the whole list.
/// </summary>
internal sealed partial class CinematicCatalogViewModel : ObservableObject, IEditedCatalog
{
    /// <param name="cinematics">The draft's cinematics as they are.</param>
    /// <param name="choices">The pictures, voices and tracks the boxes offer.</param>
    public CinematicCatalogViewModel(IEnumerable<CinematicDefinition> cinematics, CinematicMediaChoices choices)
    {
        Choices = choices;
        Cinematics = [.. cinematics.OrderBy(cinematic => cinematic.Id, StringComparer.Ordinal).Select(CinematicViewModel.From)];
        foreach (var cinematic in Cinematics)
        {
            Watch(cinematic);
        }

        SelectedCinematic = Cinematics.FirstOrDefault();
    }

    public ObservableCollection<CinematicViewModel> Cinematics { get; }

    public CinematicMediaChoices Choices { get; }

    /// <summary>
    /// Play's preview of the chosen cinematic (decision 620); stopped by any typed change or another choice.
    /// </summary>
    public CinematicPreview Preview { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveCinematicCommand), nameof(PlayCommand))]
    [NotifyPropertyChangedFor(nameof(MissingNote))]
    public partial CinematicViewModel? SelectedCinematic { get; set; }

    /// <summary>The id a cinematic is added under, as typed.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewIdProblem))]
    [NotifyCanExecuteChangedFor(nameof(AddCinematicCommand))]
    public partial string NewId { get; set; } = "";

    /// <summary>Why <see cref="NewId"/> can't be added: empty, badly formed or taken; null when it can.</summary>
    public string? NewIdProblem => ObjectIds.ProblemOf(NewId.Trim(), [.. Cinematics.Select(cinematic => cinematic.Id)]);

    /// <summary>
    /// Why the game would refuse the cinematics as typed, naming the cinematic by id (by position while it has none);
    /// null when the game would take them.
    /// </summary>
    public string? Problem =>
        RecordListProblem.FirstOf(Cinematics, cinematic => cinematic.Id, cinematic => cinematic.Problem, "Cinematic")
        ?? CatalogProblems.OfCinematics([.. Cinematics.Select(cinematic => cinematic.ToCinematic())]);

    /// <summary>
    /// "Not in the folders: intro/7.png, gone.wav.", the pictures, voices and track the chosen cinematic names that the
    /// folders lack: kept and saved (decision 442), refused by the game's start; empty when none is missing.
    /// </summary>
    public string MissingNote =>
        SelectedCinematic is { } cinematic && Choices.MissingOf(cinematic) is { Count: > 0 } missing
            ? $"Not in the folders: {string.Join(", ", missing)}."
            : "";

    public string? ChosenKey => SelectedCinematic?.Id;

    public void Choose(string key) =>
        SelectedCinematic = Cinematics.FirstOrDefault(cinematic => cinematic.Id == key) ?? SelectedCinematic;

    /// <exception cref="InvalidOperationException">
    /// There is a <see cref="Problem"/>; the page's host applies only without one.
    /// </exception>
    public IReadOnlyList<CinematicDefinition> ToCinematics() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : [.. Cinematics.Select(cinematic => cinematic.ToCinematic())];

    /// <summary>
    /// Adds a cinematic under the typed id, last, and chooses it, so its first still is the next thing added.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddCinematic))]
    private void AddCinematic()
    {
        var cinematic = CinematicViewModel.New();
        cinematic.Id = NewId.Trim();
        Watch(cinematic);
        Cinematics.Add(cinematic);
        SelectedCinematic = cinematic;
        NewId = "";
        RaiseProblemChanged();
    }

    private bool CanAddCinematic() => NewIdProblem is null;

    [RelayCommand(CanExecute = nameof(CanRemoveCinematic))]
    private void RemoveCinematic()
    {
        var removed = SelectedCinematic!;
        removed.PropertyChanged -= OnCinematicChanged;
        removed.StillsChanged -= OnCinematicStillsChanged;
        SelectedCinematic = ListOrder.Remove(Cinematics, removed);
        RaiseProblemChanged();
    }

    private bool CanRemoveCinematic() => SelectedCinematic is { IsNew: true };

    /// <summary>
    /// Plays the chosen cinematic as typed from the chosen still (the first when none is), greyed while a box of it
    /// can't be read or the game would refuse it.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanPlay))]
    private void Play()
    {
        var cinematic = SelectedCinematic!;
        var fromStill = cinematic.SelectedStill is { } still ? cinematic.Stills.IndexOf(still) : 0;
        Preview.Play(cinematic.ToCinematic(), fromStill);
    }

    // Data's rules too: a still's seconds past the game's clock would overflow the preview's.
    private bool CanPlay() =>
        SelectedCinematic is { Problem: null } cinematic && CatalogProblems.OfCinematics([cinematic.ToCinematic()]) is null;

    [RelayCommand]
    private void Stop() => Preview.Stop();

    partial void OnSelectedCinematicChanged(CinematicViewModel? value) => Preview.Stop();

    // A cinematic's typed id, its track and its stills feed the problem line. Its Label and Problem notices arrive here
    // too and are dropped: the line rereads every cinematic.
    private void Watch(CinematicViewModel cinematic)
    {
        cinematic.VoiceLengthOf = Choices.VoiceLengthOf;
        cinematic.PropertyChanged += OnCinematicChanged;
        cinematic.StillsChanged += OnCinematicStillsChanged;
    }

    private void OnCinematicChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CinematicViewModel.Id) or nameof(CinematicViewModel.MusicFile))
        {
            RaiseProblemChanged();
        }
    }

    private void OnCinematicStillsChanged(object? sender, EventArgs e) => RaiseProblemChanged();

    // The taken ids change with the list and its typed ids, so the new id's problem is read again with the line's. A
    // preview playing what was typed before stops.
    private void RaiseProblemChanged()
    {
        Preview.Stop();
        PlayCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(Problem));
        OnPropertyChanged(nameof(NewIdProblem));
        OnPropertyChanged(nameof(MissingNote));
        AddCinematicCommand.NotifyCanExecuteChanged();
    }
}
