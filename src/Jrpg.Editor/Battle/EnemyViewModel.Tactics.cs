namespace Jrpg.Editor.Battle;

/// <summary>
/// An enemy's rules, the list from the start and each phase's, as the Tactics tab edits them; <c>ToEnemy</c> writes
/// them rebuilt from their rows.
/// </summary>
internal sealed partial class EnemyViewModel
{
    /// <summary>Its rules as typed; a rule typed or a list changed is this enemy's change.</summary>
    public EnemyTacticsViewModel Tactics { get; }

    // The list reads the enemy, not its rules, so a rule's change reaches it as the enemy's own typed field.
    private void OnTacticsChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(Tactics));
        OnPropertyChanged(nameof(Problem));
    }
}
