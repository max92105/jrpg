using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Skills;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Skills;

/// <summary>
/// Every skill as the Skills page edits it (ADR 0045): the draft's skills in id order, the ones added this session
/// after them; Add starts from the first damage tier, Duplicate from the chosen one (decision 977); only an added one
/// can be removed, since classes and enemies name a saved one (decision 581). The chosen skill's users and its strip's
/// pace are read-only lines (decisions 978, 982). <see cref="Problem"/> is a skill's own problem first, then Data's
/// message for the whole list, the classes and enemies naming the skills included (decision 974). A host chooses a
/// skill by its id.
/// </summary>
internal sealed partial class SkillCatalogViewModel : ObservableObject, IEditedCatalog
{
    private readonly ContentCatalog _content;

    // Saved skills are never removed or renamed in a session (decision 581), so their ids are read once.
    private readonly IReadOnlyCollection<string> _savedIds;

    private readonly TieredEffect? _firstDamageTier;

    /// <param name="content">
    /// The draft as it is: the skills edited, the tiers Add starts from and who uses them.
    /// </param>
    /// <param name="choices">What the page's boxes offer, read when it opened.</param>
    public SkillCatalogViewModel(ContentCatalog content, SkillChoices choices)
    {
        _content = content;
        Choices = choices;
        _savedIds = [.. content.Skills.Keys];
        _firstDamageTier = content.TieredEffects.Values
            .Where(tier => tier.Kind == EffectKind.Damage)
            .MinBy(tier => tier.Id, StringComparer.Ordinal);
        Skills = [.. content.Skills.Values.OrderBy(skill => skill.Id, StringComparer.Ordinal).Select(SkillViewModel.From)];
        foreach (var skill in Skills)
        {
            skill.Changed += OnSkillChanged;
        }

        SelectedSkill = Skills.FirstOrDefault();
    }

    public ObservableCollection<SkillViewModel> Skills { get; }

    public SkillChoices Choices { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LearnedByText), nameof(UsedByText), nameof(StripPaceText))]
    [NotifyCanExecuteChangedFor(nameof(DuplicateSkillCommand), nameof(RemoveSkillCommand))]
    public partial SkillViewModel? SelectedSkill { get; set; }

    /// <summary>The classes listing the chosen skill; empty for a new one, which no class can list yet.</summary>
    public string LearnedByText => SelectedSkill is { IsNew: false } skill ? SkillUsers.LearnedBy(skill.Id, _content) : "";

    /// <summary>The enemies naming the chosen skill; empty for a new one, or when none does.</summary>
    public string UsedByText => SelectedSkill is { IsNew: false } skill ? SkillUsers.UsedBy(skill.Id, _content) : "";

    /// <summary>
    /// The chosen skill's strip as typed, with its pace from the draft's records (decision 982): "80 ms, hit on frame 3
    /// (Database > Effect strips)", "unlisted" added for a strip with no record; empty when it plays its user's strip.
    /// </summary>
    public string StripPaceText
    {
        get
        {
            if (SelectedSkill?.EffectFile.Trim() is not { Length: > 0 } strip)
            {
                return "";
            }

            var pace = EffectStripDefinition.Of(strip, Choices.StripPaces);
            var unlisted = Choices.StripPaces.ContainsKey(strip) ? "" : ", unlisted";

            // The page counts frames from 1, as the Effect strips preview does; the record counts them from 0.
            return $"{pace.FrameMilliseconds} ms, hit on frame {pace.HitFrame + 1}{unlisted} (Database > Effect strips)";
        }
    }

    /// <summary>
    /// Why the game would refuse the skills as typed, naming the skill by id (by position while it has none); null when
    /// the game would take them.
    /// </summary>
    public string? Problem =>
        RecordListProblem.FirstOf(Skills, skill => skill.Id, skill => skill.Problem, "Skill")
        ?? CatalogProblems.OfSkills([.. Skills.Select(skill => skill.ToSkill())], _content);

    public string? ChosenKey => SelectedSkill?.Id;

    public void Choose(string key) => SelectedSkill = Skills.FirstOrDefault(skill => skill.Id == key) ?? SelectedSkill;

    /// <exception cref="InvalidOperationException">
    /// There is a <see cref="Problem"/>; the page's host applies only without one.
    /// </exception>
    public IReadOnlyList<SkillDefinition> ToSkills() =>
        Problem is { } problem ? throw new InvalidOperationException(problem) : [.. Skills.Select(skill => skill.ToSkill())];

    /// <summary>Adds a skill last and chooses it, so its id is the next thing typed.</summary>
    [RelayCommand]
    private void AddSkill() => Insert(SkillViewModel.New(_firstDamageTier, _savedIds), Skills.Count);

    /// <summary>Adds a copy of the chosen skill after it, its id left to type (decision 585).</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedSkill))]
    private void DuplicateSkill() =>
        Insert(SkillViewModel.DuplicateOf(SelectedSkill!, _savedIds), Skills.IndexOf(SelectedSkill!) + 1);

    [RelayCommand(CanExecute = nameof(CanRemoveSkill))]
    private void RemoveSkill()
    {
        var removed = SelectedSkill!;
        removed.Changed -= OnSkillChanged;
        SelectedSkill = ListOrder.Remove(Skills, removed);
        OnPropertyChanged(nameof(Problem));
    }

    private bool HasSelectedSkill() => SelectedSkill is not null;

    private bool CanRemoveSkill() => SelectedSkill is { IsNew: true };

    private void Insert(SkillViewModel skill, int index)
    {
        skill.Changed += OnSkillChanged;
        Skills.Insert(index, skill);
        SelectedSkill = skill;
        OnPropertyChanged(nameof(Problem));
    }

    // Only the typed fields reach here, so Data's check runs once per change. Only the chosen skill's boxes are shown,
    // and the pace line is a lookup, so it follows every change rather than telling the strip box apart.
    private void OnSkillChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(Problem));
        OnPropertyChanged(nameof(StripPaceText));
    }
}
