using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tests.Skills;

namespace Jrpg.Editor.Tests.Catalogs;

// The tiered effects are replaced whole as one undo step whose do and undo both point the skills at the records of the
// step's side (decision 975), since a skill holds its tier, not its id.
public sealed class CatalogDraftTieredEffectTests
{
    private static readonly TieredEffect _retunedFire1 = SkillFixtures.Fire1 with { PerPower = 5 };

    private readonly CatalogDraft _draft =
        new(SkillFixtures.CatalogWith([SkillFixtures.Bolt], [SkillFixtures.Fire1], [SkillFixtures.Poison1]));

    [Fact]
    public void SetTieredEffects_ChangedPerPower_SkillsHoldTheNewRecord()
    {
        _draft.SetTieredEffects([_retunedFire1]);

        Assert.Same(_retunedFire1, _draft.Current.Skills["bolt"].Effect.Tier);
    }

    [Fact]
    public void SetTieredEffects_Undo_SkillsHoldTheOldRecord()
    {
        _draft.SetTieredEffects([_retunedFire1]);

        _draft.Undo();

        Assert.Same(SkillFixtures.Fire1, _draft.Current.Skills["bolt"].Effect.Tier);
    }

    // Undone in order: the skills step first, back to the retuned record it saw, then the effects step, back to fire_1.
    [Fact]
    public void SetTieredEffects_ThenSetSkills_UndoBoth_SkillsHoldTheOldNameAndRecord()
    {
        _draft.SetTieredEffects([_retunedFire1]);
        _draft.SetSkills([_draft.Current.Skills["bolt"] with { Name = "Thunder" }]);

        _draft.Undo();
        _draft.Undo();

        var bolt = _draft.Current.Skills["bolt"];
        Assert.Equal("Bolt", bolt.Name);
        Assert.Same(SkillFixtures.Fire1, bolt.Effect.Tier);
    }

    // The review's C7: the skills step restores the skills it saw, which held the retuned record.
    [Fact]
    public void SetTieredEffects_ThenSetSkills_UndoBothRedoBoth_SkillsHoldTheNewRecord()
    {
        _draft.SetTieredEffects([_retunedFire1]);
        _draft.SetSkills([_draft.Current.Skills["bolt"] with { Name = "Thunder" }]);

        _draft.Undo();
        _draft.Undo();
        _draft.Redo();
        _draft.Redo();

        var bolt = _draft.Current.Skills["bolt"];
        Assert.Equal("Thunder", bolt.Name);
        Assert.Same(_retunedFire1, bolt.Effect.Tier);
    }

    [Fact]
    public void SetTieredEffects_EqualByContent_RecordsNothing()
    {
        _draft.SetTieredEffects([SkillFixtures.Fire1 with { }]);

        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void SetTieredEffects_AddedRecord_UndoRemovesIt()
    {
        var fire2 = new TieredEffect("fire_2", EffectKind.Damage, 6) { DamageType = DamageType.Fire };
        _draft.SetTieredEffects([SkillFixtures.Fire1, fire2]);

        _draft.Undo();

        Assert.False(_draft.Current.TieredEffects.ContainsKey("fire_2"));
    }
}
