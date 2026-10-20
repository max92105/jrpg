using Jrpg.Core.Battle;
using Jrpg.Core.Effects;

namespace Jrpg.Game.Battle;

/// <summary>
/// The targets being picked from Core's legal choice: the pointer only visits candidates, in formation order, wrapping
/// at either end (developer decision 56). A choice that takes everyone selects every candidate and doesn't move.
/// </summary>
internal sealed class BattleTargetCursor
{
    private readonly bool _takesAll;
    private int _index;

    /// <param name="choice">Who can be targeted, from Core; at least one candidate.</param>
    /// <param name="actor">Who aims: the cursor starts on them when they are a candidate, else on the last enemy.</param>
    /// <exception cref="ArgumentException">The choice has no candidate.</exception>
    public BattleTargetCursor(TargetChoice<Combatant> choice, Combatant actor)
    {
        if (choice.Candidates.Count == 0)
        {
            throw new ArgumentException("A target cursor needs at least one candidate.", nameof(choice));
        }

        Candidates = choice.Candidates;
        _takesAll = choice.TakesAll;
        _index = StartIndex(choice.Candidates, actor);
        Selected = _takesAll ? Candidates : [Candidates[_index]];
    }

    /// <summary>Everyone the pointer can visit, in formation order.</summary>
    public IReadOnlyList<Combatant> Candidates { get; }

    /// <summary>The chosen targets: the one under the pointer, or every candidate for a choice that takes all.</summary>
    public IReadOnlyList<Combatant> Selected { get; private set; }

    public void MovePrevious() => Step(-1);

    public void MoveNext() => Step(1);

    private static int StartIndex(IReadOnlyList<Combatant> candidates, Combatant actor)
    {
        for (var index = candidates.Count - 1; index >= 0; index--)
        {
            if (candidates[index] == actor)
            {
                return index;
            }
        }

        var lastEnemy = candidates.Count - 1;
        while (lastEnemy > 0 && candidates[lastEnemy].Side != BattleSide.Enemies)
        {
            lastEnemy--;
        }

        return lastEnemy;
    }

    private void Step(int direction)
    {
        if (_takesAll)
        {
            return;
        }

        _index = (_index + direction + Candidates.Count) % Candidates.Count;
        Selected = [Candidates[_index]];
    }
}
