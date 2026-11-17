using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Party;

/// <summary>
/// Every party member as the Party members page edits it (decision 953): the draft's members in id order, the ones
/// added this session after them; Add starts from the blank; only an added member can be removed, since maps and the
/// new game name a saved one (decision 581); no Duplicate, a member being one character. <see cref="Problem"/> is the
/// chosen member's own first, then another's, then Data's message for the whole list; <see cref="MissingArtNote"/> the
/// chosen member's pictures the art folders lack. Its host chooses a member by its id (<see cref="IEditedCatalog"/>).
/// </summary>
internal sealed partial class PartyMemberCatalogViewModel : ObservableObject, IEditedCatalog
{
    private readonly ContentCatalog _content;

    /// <param name="content">The draft as it is: the members edited and the classes they name.</param>
    /// <param name="artImagesIn">The PNG names of an art folder by name, built by the shell over the art root.</param>
    public PartyMemberCatalogViewModel(ContentCatalog content, Func<string, IReadOnlyList<string>> artImagesIn)
    {
        _content = content;
        ArtChoices = new BattleArtChoices(artImagesIn);
        Members =
        [
            .. content.PartyMembers.Values
                .OrderBy(member => member.Id, StringComparer.Ordinal)
                .Select(PartyMemberViewModel.From),
        ];
        foreach (var listed in Members)
        {
            listed.Changed += OnMemberChanged;
        }

        Classes = ClassChoicesOf(content);
        SelectedMember = Members.FirstOrDefault();
    }

    public ObservableCollection<PartyMemberViewModel> Members { get; }

    /// <summary>
    /// The class box's choices: the draft's classes by name, then a missing one for each class a member names that the
    /// draft lacks, so the box still shows it (decision 967).
    /// </summary>
    public IReadOnlyList<RecordChoice> Classes { get; }

    /// <summary>The files the three art boxes offer: the battlers, portraits and effects folders.</summary>
    public BattleArtChoices ArtChoices { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(MissingArtNote))]
    [NotifyCanExecuteChangedFor(nameof(RemoveMemberCommand))]
    public partial PartyMemberViewModel? SelectedMember { get; set; }

    /// <summary>
    /// Why the game would refuse the members as typed: the chosen member's own first, as the page shows it, then
    /// another's named by id (by position while it has none), then Data's; null when the game would take them.
    /// </summary>
    public string? Problem =>
        SelectedMember?.Problem
        ?? RecordListProblem.FirstOf(Members, listed => listed.Id, listed => listed.Problem, "Member")
        ?? CatalogProblems.OfPartyMembers([.. Members.Select(listed => listed.ToMember())], _content);

    /// <summary>
    /// "Not in the art folders: portrait Omer2.png.", the chosen member's pictures its folders lack: kept and saved
    /// (decision 442), refused by the game's start; empty when every one is there or no member is chosen.
    /// </summary>
    public string MissingArtNote =>
        SelectedMember is { } chosen
        && ArtChoices.MissingOf(chosen.BattleSpriteFile, chosen.PortraitFile, chosen.AttackEffectFile) is [_, ..] missing
            ? $"Not in the art folders: {string.Join(", ", missing)}."
            : "";

    /// <summary>The chosen member's id; null with none chosen or while a new one has no id yet.</summary>
    public string? ChosenKey => SelectedMember is { Id.Length: > 0 } chosen ? chosen.Id : null;

    public void Choose(string key) => SelectedMember = Members.FirstOrDefault(listed => listed.Id == key) ?? SelectedMember;

    /// <exception cref="InvalidOperationException">
    /// There is a <see cref="Problem"/>; the page's host applies only without one.
    /// </exception>
    public IReadOnlyList<PartyMemberDefinition> ToMembers() =>
        Problem is { } problem ? throw new InvalidOperationException(problem) : [.. Members.Select(listed => listed.ToMember())];

    private static List<RecordChoice> ClassChoicesOf(ContentCatalog content) =>
    [
        .. content.Classes.Values
            .OrderBy(classDefinition => classDefinition.Name, StringComparer.Ordinal)
            .ThenBy(classDefinition => classDefinition.Id, StringComparer.Ordinal)
            .Select(classDefinition => new RecordChoice(classDefinition.Id, classDefinition.Name)),
        .. content.PartyMembers.Values
            .Select(member => member.ClassId)
            .Where(classId => !content.Classes.ContainsKey(classId))
            .Distinct()
            .Order(StringComparer.Ordinal)
            .Select(RecordChoice.Missing),
    ];

    /// <summary>Adds a new member last and chooses it, so its id is the next thing typed.</summary>
    [RelayCommand]
    private void AddMember()
    {
        var added = PartyMemberViewModel.New();
        added.Changed += OnMemberChanged;
        Members.Add(added);
        SelectedMember = added;
    }

    [RelayCommand(CanExecute = nameof(CanRemoveMember))]
    private void RemoveMember()
    {
        var removed = SelectedMember!;
        removed.Changed -= OnMemberChanged;
        SelectedMember = ListOrder.Remove(Members, removed);
    }

    private bool CanRemoveMember() => SelectedMember is { IsNew: true };

    // Only typed fields reach here, never a member's label or problem notices, so Data's check runs once per change.
    // The chosen member's art names are among them, so its note is read again.
    private void OnMemberChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(Problem));
        OnPropertyChanged(nameof(MissingArtNote));
    }
}
