using Jrpg.Core.Battle;
using Jrpg.Core.Maps;

namespace Jrpg.Game.Battle;

/// <summary>
/// Which frame of its strip each combatant's sprite shows (ADR 0047): an animated enemy loops always, at the battle's
/// time, each enemy one frame ahead of the enemy before it so a trio of one kind doesn't bob as one (decision 635); a
/// fallen one holds the frame it fell on while it whitens and fades (decision 634). A still battler shows frame 0.
/// </summary>
internal sealed class BattlerFrames
{
    private readonly IReadOnlyList<Combatant> _combatants;
    private readonly TileAnimation?[] _loops;
    private readonly int[] _enemiesBefore;
    private readonly int?[] _heldFrames;
    private TimeSpan _elapsed;

    /// <param name="combatants">
    /// The battle's combatants, in its order: an enemy's place counts the enemies before it.
    /// </param>
    /// <param name="arts">Their art in the same order, whose loops play.</param>
    public BattlerFrames(IReadOnlyList<Combatant> combatants, IReadOnlyList<BattlerArt> arts)
    {
        _combatants = combatants;
        _loops = [.. arts.Select(art => art.IdleAnimation)];
        _enemiesBefore = [.. combatants.Select((_, index) => EnemiesBefore(combatants, index))];
        _heldFrames = new int?[combatants.Count];
    }

    /// <summary>
    /// Moves the battle's time on; a combatant seen fallen for the first time keeps the frame it was showing, until it
    /// is revived and loops again.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">A negative delta: the time only moves on.</exception>
    public void Advance(TimeSpan delta)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(delta, TimeSpan.Zero);
        for (var index = 0; index < _combatants.Count; index++)
        {
            if (_combatants[index].IsAlive)
            {
                _heldFrames[index] = null;
            }
            else if (_heldFrames[index] is null && _loops[index] is not null)
            {
                _heldFrames[index] = FrameOf(index);
            }
        }

        _elapsed += delta;
    }

    /// <param name="index">The combatant's place in the battle.</param>
    /// <returns>The frame from 0; 0 for a still battler.</returns>
    public int FrameOf(int index) =>
        _heldFrames[index] ?? (_loops[index] is { } loop ? LoopFrameOf(index, loop) : 0);

    private int LoopFrameOf(int index, TileAnimation loop) =>
        loop.FrameAt(_elapsed + (_enemiesBefore[index] * loop.FrameDuration));

    private static int EnemiesBefore(IReadOnlyList<Combatant> combatants, int index) =>
        combatants.Take(index).Count(before => before.Side == BattleSide.Enemies);
}
