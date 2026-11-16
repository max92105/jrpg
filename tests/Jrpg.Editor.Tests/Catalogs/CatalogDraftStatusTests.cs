using Jrpg.Core.Statuses;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tests.Skills;

namespace Jrpg.Editor.Tests.Catalogs;

// The statuses are replaced whole as one undo step whose do and undo both point the skills' grants at the records of
// the step's side (decision 975), since a grant holds its status, not its id.
public sealed class CatalogDraftStatusTests
{
    private static readonly StatusDefinition _strongerPoison1 =
        SkillFixtures.Poison1 with { Parts = [new StatusPart(StatusPartKind.DamageEachTurn, 12)] };

    private readonly CatalogDraft _draft =
        new(SkillFixtures.CatalogWith([SkillFixtures.Bolt], [SkillFixtures.Fire1], [SkillFixtures.Poison1]));

    [Fact]
    public void SetStatuses_ChangedAmount_GrantsHoldTheNewRecord()
    {
        _draft.SetStatuses([_strongerPoison1]);

        Assert.Same(_strongerPoison1, _draft.Current.Skills["bolt"].Effect.Statuses[0].Status);
    }

    [Fact]
    public void SetStatuses_Undo_GrantsHoldTheOldRecord()
    {
        _draft.SetStatuses([_strongerPoison1]);

        _draft.Undo();

        Assert.Same(SkillFixtures.Poison1, _draft.Current.Skills["bolt"].Effect.Statuses[0].Status);
    }

    // Built apart from the same parts, so only a comparison of the parts in order finds them equal.
    [Fact]
    public void SetStatuses_EqualByContent_RecordsNothing()
    {
        _draft.SetStatuses([SkillFixtures.Poison1 with { Parts = [new StatusPart(StatusPartKind.DamageEachTurn, 4)] }]);

        Assert.False(_draft.CanUndo);
    }
}
