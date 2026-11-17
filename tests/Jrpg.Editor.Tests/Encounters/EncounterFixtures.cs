using Jrpg.Core.Battle;
using Jrpg.Editor.Encounters;

namespace Jrpg.Editor.Tests.Encounters;

/// <summary>The Encounters page tests' own enemies and encounter, so no repository record is pinned.</summary>
internal static class EncounterFixtures
{
    public static EnemyChoice Goblin { get; } = new("goblin", "Goblin");

    public static EnemyChoice Orc { get; } = new("orc", "Orc");

    public static EncounterChoices Choices { get; } = new([Goblin, Orc]);

    /// <summary>Two goblins: two slots whose rows are alike but for their reference.</summary>
    public static EncounterDefinition GoblinPair { get; } = new() { Id = "goblin_pair", EnemyIds = ["goblin", "goblin"] };
}
