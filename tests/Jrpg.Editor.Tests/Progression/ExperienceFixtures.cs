using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Editor.Tests.Party;

namespace Jrpg.Editor.Tests.Progression;

/// <summary>
/// The Experience page tests' own table, so no repository number is pinned: three steps of 10, 20 and 40, max level 4,
/// in the test world with the archer as its only class (its one skill at level 1), so no repository class learning a
/// skill late refuses the table.
/// </summary>
internal static class ExperienceFixtures
{
    public static ExperienceTable TestTable { get; } = new([10, 20, 40]);

    public static ContentCatalog World { get; } = WithTable(TestTable);

    public static ContentCatalog WithTable(ExperienceTable table) =>
        ClassFixtures.World with
        {
            Classes = new Dictionary<string, ClassDefinition> { [ClassFixtures.Archer.Id] = ClassFixtures.Archer },
            Rules = ClassFixtures.World.Rules with { Experience = table },
        };
}
