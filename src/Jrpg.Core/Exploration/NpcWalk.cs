using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Core.Exploration;

/// <summary>
/// One NPC walking a <c>moveNpc</c> step's path (ADR 0044): one cell per step from where it stands, at the speed the
/// Game advances it, facing each step's way, through walls and the player (decision 575). A step that would leave the
/// map is dropped when the walk starts (decision 578). Once arrived it stays at the end, facing its last step, until
/// the visit ends; nothing allocates per update.
/// </summary>
internal sealed class NpcWalk
{
    private readonly List<Facing> _steps;
    private readonly Point2D _start;
    private int _index;
    private double _progress;

    /// <param name="start">Where the NPC stands as the walk begins, its top-left in map cells.</param>
    /// <param name="facing">What it faces then, kept when no step is left to walk.</param>
    /// <param name="path">The step's path; a step past the map's edge is dropped and the next one walks on.</param>
    /// <param name="layout">The map walked on.</param>
    public NpcWalk(Point2D start, Facing facing, IReadOnlyList<Facing> path, MapLayout layout)
    {
        _start = start;
        Position = start;
        _steps = new List<Facing>(path.Count);
        var cell = GridCell.Containing(start);
        foreach (var step in path)
        {
            var next = Facings.Step(cell, step);
            if (!layout.Contains(next))
            {
                continue;
            }

            _steps.Add(step);
            cell = next;
        }

        End = (Position: WalkedTo(_steps.Count, 0), Facing: _steps.Count > 0 ? _steps[^1] : facing);
        Facing = _steps.Count > 0 ? _steps[0] : facing;
    }

    /// <summary>The NPC's top-left now, between cells while a step is under way.</summary>
    public Point2D Position { get; private set; }

    /// <summary>The current step's way; the last step's once arrived.</summary>
    public Facing Facing { get; private set; }

    /// <summary>How long it has walked, for the walk frame.</summary>
    public TimeSpan Walked { get; private set; }

    public bool IsFinished => _index >= _steps.Count;

    /// <summary>Where the walk ends and what the NPC faces there.</summary>
    public (Point2D Position, Facing Facing) End { get; }

    /// <summary>
    /// Moves on by what <paramref name="delta"/> covers at <paramref name="cellsPerSecond"/>, never past the end.
    /// </summary>
    public void Advance(TimeSpan delta, double cellsPerSecond)
    {
        if (IsFinished)
        {
            return;
        }

        Walked += delta;
        var distance = cellsPerSecond * delta.TotalSeconds;
        while (distance > 0 && !IsFinished)
        {
            var remaining = 1 - _progress;
            if (distance >= remaining)
            {
                distance -= remaining;
                _index++;
                _progress = 0;
            }
            else
            {
                _progress += distance;
                distance = 0;
            }
        }

        Position = WalkedTo(_index, _progress);
        Facing = IsFinished ? End.Facing : _steps[_index];
    }

    /// <summary>
    /// Jumps to the end: a skipped cutscene places the NPC where the walk would have left it (decision 576).
    /// </summary>
    public void Finish()
    {
        _index = _steps.Count;
        _progress = 0;
        Position = End.Position;
        Facing = End.Facing;
    }

    // The start plus the whole steps before the current one, plus the current step's progress along its way.
    private Point2D WalkedTo(int wholeSteps, double progress)
    {
        var (x, y) = (_start.X, _start.Y);
        for (var index = 0; index < wholeSteps; index++)
        {
            (x, y) = Moved(x, y, _steps[index], 1);
        }

        return wholeSteps < _steps.Count ? Pointed(Moved(x, y, _steps[wholeSteps], progress)) : new Point2D(x, y);
    }

    private static Point2D Pointed((double X, double Y) point) => new(point.X, point.Y);

    private static (double X, double Y) Moved(double x, double y, Facing facing, double distance) => facing switch
    {
        Facing.Up => (x, y - distance),
        Facing.Down => (x, y + distance),
        Facing.Left => (x - distance, y),
        _ => (x + distance, y),
    };
}
