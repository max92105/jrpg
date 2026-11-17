using CommunityToolkit.Mvvm.ComponentModel;

namespace Jrpg.Editor.Encounters;

/// <summary>
/// One place in an encounter's line of enemies, its place on screen its position in the list. A class, not a record:
/// two goblins are two rows told apart by reference, so moving or removing the chosen one never takes the other.
/// </summary>
internal sealed partial class EnemySlotRow(string enemyId, string name) : ObservableObject
{
    public string EnemyId { get; } = enemyId;

    public string Name { get; } = name;

    /// <summary>The row's place from 1, as the page numbers its slots ("1. Goblin"); the encounter sets it.</summary>
    [ObservableProperty]
    public partial int Number { get; set; }
}
