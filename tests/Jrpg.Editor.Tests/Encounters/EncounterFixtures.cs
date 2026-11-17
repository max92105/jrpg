using Jrpg.Core.Battle;
using Jrpg.Editor.Encounters;

namespace Jrpg.Editor.Tests.Encounters;

/// <summary>
/// The Encounters page tests' own enemies, folders and encounter, so no repository record or file is pinned.
/// </summary>
internal static class EncounterFixtures
{
    public static EnemyChoice Goblin { get; } = new("goblin", "Goblin");

    public static EnemyChoice Orc { get; } = new("orc", "Orc");

    /// <summary>Two backgrounds and one track in the folders.</summary>
    public static EncounterChoices Choices { get; } = new([Goblin, Orc], ["Cave.png", "Forest.png"], ["boss/loop.wav"]);

    /// <summary>Two goblins: two slots whose rows are alike but for their reference.</summary>
    public static EncounterDefinition GoblinPair { get; } = new() { Id = "goblin_pair", EnemyIds = ["goblin", "goblin"] };
}
