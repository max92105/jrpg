using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Party;
using Jrpg.Data.Art;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Party;

/// <summary>
/// One party member as the Party members page types it (decision 953): its id (typed while the member is new, fixed
/// once the draft holds it, decision 582), name, class by id, summary and the three art file names. Every field of the
/// record is a box, so <see cref="ToMember"/> builds the record from the boxes alone. <see cref="Problem"/> is only the
/// typed id's and the class's; the rest is Data's rules, which the list shows.
/// </summary>
internal sealed partial class PartyMemberViewModel : ObservableObject
{
    private PartyMemberViewModel(PartyMemberDefinition member)
    {
        Id = member.Id;
        Name = member.Name;
        ClassId = member.ClassId;
        Summary = member.Summary;
        PortraitFile = member.PortraitFile;
        BattleSpriteFile = member.BattleSpriteFile;
        AttackEffectFile = member.AttackEffectFile;
    }

    /// <summary>
    /// A typed field changed: what the list's problem line listens to, never the label's or the problem's own notices,
    /// so the list runs Data's check once per change.
    /// </summary>
    public event EventHandler? Changed;

    public static PartyMemberViewModel From(PartyMemberDefinition member) => new(member);

    /// <summary>A member added this session (design section 1): every text empty and no class.</summary>
    public static PartyMemberViewModel New() =>
        new(new PartyMemberDefinition
        {
            Id = "",
            Name = "",
            ClassId = "",
            Summary = "",
            PortraitFile = "",
            BattleSpriteFile = "",
            AttackEffectFile = "",
        })
        {
            IsNew = true,
        };

    /// <summary>
    /// True while the member was added this session: the only time its id can be typed (decision 582).
    /// </summary>
    public bool IsNew { get; private init; }

    /// <summary>The id as typed; the page lets it be typed only while <see cref="IsNew"/>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string Id { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public partial string Name { get; set; }

    /// <summary>The class's id, the class box's chosen value; empty until one is chosen.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string ClassId { get; set; }

    [ObservableProperty]
    public partial string Summary { get; set; }

    [ObservableProperty]
    public partial string PortraitFile { get; set; }

    [ObservableProperty]
    public partial string BattleSpriteFile { get; set; }

    [ObservableProperty]
    public partial string AttackEffectFile { get; set; }

    /// <summary>The list's line: "omer: Omer"; "(new member)" while it has no id.</summary>
    public string Label => Id.Length > 0 ? $"{Id}: {Name}" : "(new member)";

    /// <summary>
    /// Why the member can't be built: the typed id while new, then a class not chosen (the box's value is null while
    /// nothing is chosen); null when it can be built.
    /// </summary>
    public string? Problem =>
        (IsNew ? ObjectIds.ProblemOf(Id, []) : null) ?? (string.IsNullOrEmpty(ClassId) ? "Class must be chosen." : null);

    /// <summary>Puts a file just imported into an art folder in that folder's box.</summary>
    /// <param name="artFolder">The battlers, portraits or effects folder (<see cref="ArtFolders"/>).</param>
    /// <param name="file">The file's name in that folder.</param>
    /// <exception cref="ArgumentException">No member picture comes from the folder.</exception>
    public void Pick(string artFolder, string file)
    {
        switch (artFolder)
        {
            case ArtFolders.Battlers:
                BattleSpriteFile = file;
                break;
            case ArtFolders.Portraits:
                PortraitFile = file;
                break;
            case ArtFolders.Effects:
                AttackEffectFile = file;
                break;
            default:
                throw new ArgumentException($"No member picture comes from the '{artFolder}' folder.", nameof(artFolder));
        }
    }

    /// <summary>The record the boxes make, the name, summary and art file names trimmed.</summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public PartyMemberDefinition ToMember() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : new PartyMemberDefinition
            {
                Id = Id,
                Name = Name.Trim(),
                ClassId = ClassId,
                Summary = Summary.Trim(),
                PortraitFile = PortraitFile.Trim(),
                BattleSpriteFile = BattleSpriteFile.Trim(),
                AttackEffectFile = AttackEffectFile.Trim(),
            };

    partial void OnIdChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnNameChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnClassIdChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnSummaryChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnPortraitFileChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnBattleSpriteFileChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnAttackEffectFileChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);
}
