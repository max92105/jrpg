namespace Jrpg.Core.Battle;

/// <summary>
/// What an enemy's rules remember of this battle so far: for each reaction, the foes who did it since the enemy's last
/// turn (decision 844), the skills it used this battle (decision 842) and the turns it acted (decision 847). Only
/// <see cref="TacticMemoryNotes"/> writes it; choosing a rule only reads it, so deciding eligibility draws nothing.
/// </summary>
internal sealed class TacticMemory
{
    private readonly Dictionary<TacticConditionKind, List<Combatant>> _reactions = [];
    private readonly HashSet<string> _usedSkills = [];

    /// <summary>The turns the enemy acted this battle; a turn a stun skipped has no action, so it isn't one.</summary>
    public int TurnsActed { get; private set; }

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

    /// <summary>
    /// Whether the enemy used this skill this battle, by skill id, whichever rule chose it (decision 842).
    /// </summary>
    public bool HasUsed(string skillId) => _usedSkills.Contains(skillId);

    /// <summary>
    /// The enemy used this skill, so its once-per-battle rules are spent for the battle (decision 842).
    /// </summary>
    public void NoteUsed(string skillId) => _usedSkills.Add(skillId);

    /// <summary>The enemy acted a turn, which its every-Nth-turn rules count (decision 847).</summary>
    public void NoteTurnActed() => TurnsActed++;

    /// <summary>The enemy's turn answered what was noted, whether a rule used it or not (decision 844).</summary>
    public void ForgetReactions() => _reactions.Clear();
}
