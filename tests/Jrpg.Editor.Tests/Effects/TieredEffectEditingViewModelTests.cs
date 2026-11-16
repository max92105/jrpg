using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Effects;
using Jrpg.Editor.Tests.Skills;

namespace Jrpg.Editor.Tests.Effects;

// The Effects page starts from the draft and its whole session is one undo step, the skills following its records
// (decision 975) wherever they name them.
public sealed class TieredEffectEditingViewModelTests
{
    private readonly CatalogDraft _draft = new(SkillFixtures.CatalogWithoutUsers([SkillFixtures.Bolt], [SkillFixtures.Fire1]));
    private readonly TieredEffectEditingViewModel _effectEditing;

    public TieredEffectEditingViewModelTests() => _effectEditing = new TieredEffectEditingViewModel(_draft);

    [Fact]
    public void Apply_EditedPerPower_SkillsHoldTheNewRecord()
    {
        var edited = _effectEditing.Start();
        edited.Effects.Single(effect => effect.Id == "fire_1").PerPowerText = "5";

        _effectEditing.Apply(edited);

        Assert.Equal(5, _draft.Current.Skills["bolt"].Effect.Tier!.PerPower);
        Assert.Same(_draft.Current.TieredEffects["fire_1"], _draft.Current.Skills["bolt"].Effect.Tier);
    }

    [Fact]
    public void Apply_EditedPerPower_IsOneStep()
    {
        var steps = 0;
        _draft.StepRecorded += (_, _) => steps++;
        var edited = _effectEditing.Start();
        edited.Effects[0].PerPowerText = "5";

        _effectEditing.Apply(edited);

        Assert.Equal(1, steps);
    }

    // The records are rebuilt from their boxes, so only a comparison by content keeps the step out.
    [Fact]
    public void Apply_Untouched_RecordsNoStep()
    {
        var steps = 0;
        _draft.StepRecorded += (_, _) => steps++;
        var edited = _effectEditing.Start();

        _effectEditing.Apply(edited);

        Assert.Equal(0, steps);
    }
}
