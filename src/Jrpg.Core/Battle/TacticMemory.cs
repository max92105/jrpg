namespace Jrpg.Core.Battle;

/// <summary>
/// What an enemy's rules remember of this battle so far: for each reaction, the foes who did it since the enemy's last
/// turn (decision 844). Only <see cref="TacticMemoryNotes"/> writes it; choosing a rule only reads it, so deciding
/// eligibility draws nothing.
/// </summary>
internal sealed class TacticMemory
{
    private readonly Dictionary<TacticConditionKind, List<Combatant>> _reactions = [];

    /// <summary>Notes that <paramref name="subject"/> did <paramref name="kind"/>, once per kind.</summary>
    public void NoteReaction(TacticConditionKind kind, Combatant subject)
    {
        if (!_reactions.TryGetValue(kind, out var subjects))
        {
            subjects = [];
            _reactions[kind] = subjects;
        }

        if (!subjects.Contains(subject))
        {
            subjects.Add(subject);
        }
    }

    /// <returns>The foes who did <paramref name="kind"/>, in the order noted, fallen ones kept.</returns>
    public IReadOnlyList<Combatant> Noted(TacticConditionKind kind) =>
        _reactions.TryGetValue(kind, out var subjects) ? [.. subjects] : [];

    /// <summary>The enemy's turn answered what was noted, whether a rule used it or not (decision 844).</summary>
    public void ForgetReactions() => _reactions.Clear();
}
