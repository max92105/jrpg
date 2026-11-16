using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Statuses;
using Jrpg.Editor.Tests.Skills;

namespace Jrpg.Editor.Tests.Statuses;

// The Statuses page starts from the draft and its whole session is one undo step, the skills' grants following its
// records (decision 975) wherever they name them.
public sealed class StatusEditingViewModelTests
{
    // Every optional field set, so a box that rebuilt it differently would show as a change.
    private static readonly StatusDefinition _venom = new()
    {
        Id = "venom",
        Name = "Venom",
        Description = "Burns and blurs.",
        IsHarmful = true,
        ActsAt = StatusTiming.StartOfTurn,
        Family = "toxin",
        MarkLetter = "V",
        Parts =
        [
            new StatusPart(StatusPartKind.DamageEachTurn, 3, DamageType.Fire),
            new StatusPart(StatusPartKind.StatChange, -10, Stat: StatKind.Accuracy),
        ],
    };

    private readonly CatalogDraft _draft = new(SkillFixtures.CatalogWithoutUsers([SkillFixtures.Bolt], [SkillFixtures.Fire1]) with
    {
        Statuses = new[] { SkillFixtures.Poison1, _venom }.ToDictionary(status => status.Id, StringComparer.Ordinal),
    });

    private readonly StatusEditingViewModel _statusEditing;

    public StatusEditingViewModelTests() => _statusEditing = new StatusEditingViewModel(_draft);

    [Fact]
    public void Apply_EditedAmount_GrantsHoldTheNewRecord()
    {
        var edited = _statusEditing.Start();
        edited.Statuses.Single(status => status.Id == "poison_1").Parts[0].AmountText = "12";

        _statusEditing.Apply(edited);

        Assert.Equal(12, _draft.Current.Skills["bolt"].Effect.Statuses[0].Status.Parts[0].Amount);
        Assert.Same(_draft.Current.Statuses["poison_1"], _draft.Current.Skills["bolt"].Effect.Statuses[0].Status);
    }

    [Fact]
    public void Apply_EditedAmount_IsOneStep()
    {
        var steps = 0;
        _draft.StepRecorded += (_, _) => steps++;
        var edited = _statusEditing.Start();
        edited.Statuses.Single(status => status.Id == "venom").Parts[1].AmountText = "-20";

        _statusEditing.Apply(edited);

        Assert.Equal(1, steps);
    }

    // The records are rebuilt from their boxes, so only a comparison by content keeps the step out.
    [Fact]
    public void Apply_Untouched_RecordsNoStep()
    {
        var steps = 0;
        _draft.StepRecorded += (_, _) => steps++;
        var edited = _statusEditing.Start();

        _statusEditing.Apply(edited);

        Assert.Equal(0, steps);
    }
}
