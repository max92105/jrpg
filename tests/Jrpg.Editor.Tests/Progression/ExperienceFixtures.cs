using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Editor.Progression;
using Jrpg.Editor.Tests.Party;

namespace Jrpg.Editor.Tests.Progression;

/// <summary>
/// The Experience page tests' own table, so no repository number is pinned: three steps of 10, 20 and 40, max level 4,
/// in the test world with the skilled archer as its only class: its expose at level 4, the max level, refuses the table
/// once a row is removed.
/// </summary>
internal static class ExperienceFixtures
{
    public static ExperienceTable TestTable { get; } = new([10, 20, 40]);

    public static ContentCatalog World { get; } = WithTable(TestTable);

    public static ContentCatalog WithTable(ExperienceTable table) =>
        ClassFixtures.World with
        {
            Classes = new Dictionary<string, ClassDefinition>
            {
                [ClassFixtures.SkilledArcher.Id] = ClassFixtures.SkilledArcher,
            },
            Rules = ClassFixtures.World.Rules with { Experience = table },
        };

    /// <summary>
    /// The page's table over <see cref="WithTable"/>, as if the table were saved: no max level note yet.
    /// </summary>
    public static ExperienceTableViewModel TableOf(ExperienceTable table) => new(WithTable(table), table.MaxLevel);
}
