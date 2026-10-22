using Jrpg.Core.Content;
using Jrpg.Core.Geometry;

namespace Jrpg.Core.Exploration;

/// <summary>The player on the map: where they stand, which way they face, and how they move.</summary>
internal sealed class PlayerWalker(Point2D position, Facing facing, ExplorationRules rules)
{
    public Point2D Position { get; private set; } = position;

    public Facing Facing { get; private set; } = facing;

    /// <summary>True while the player pushes in a direction, even against a wall.</summary>
    public bool IsMoving { get; private set; }

    public GridCell Cell => GridCell.Containing(Position);

    /// <summary>Puts the player somewhere without walking there, as a warp does.</summary>
    public void PlaceAt(Point2D newPosition)
    {
        Position = newPosition;
        IsMoving = false;
    }

    /// <summary>
    /// Moves <c>WalkSpeed × tilt × delta</c> along the intent. The move is split into steps no longer than the
    /// player's radius, so a long frame can't carry the body over a wall.
    /// </summary>
    public void Walk(MoveIntent intent, TimeSpan delta, MapCollision collision)
    {
        IsMoving = intent.Magnitude > 0;
        if (!IsMoving)
        {
            return;
        }

        Facing = FacingOf(intent);
        var distance = rules.WalkSpeed * intent.Magnitude * delta.TotalSeconds;
        var stepCount = Math.Max(1, (int)Math.Ceiling(distance / rules.PlayerRadius));
        var stepScale = rules.WalkSpeed * delta.TotalSeconds / stepCount;
        var step = new Point2D(intent.X * stepScale, intent.Y * stepScale);
        for (var index = 0; index < stepCount; index++)
        {
            if (!TryStep(step, collision))
            {
                return;
            }
        }
    }

    // The larger axis wins; on a tie the player faces up or down, so two keys held give a stable facing.
    private static Facing FacingOf(MoveIntent intent)
    {
        if (Math.Abs(intent.Y) >= Math.Abs(intent.X))
        {
            return intent.Y < 0 ? Facing.Up : Facing.Down;
        }

        return intent.X < 0 ? Facing.Left : Facing.Right;
    }

    // A blocked diagonal tries its horizontal part alone, then its vertical part, so it slides along a wall instead of
    // stopping dead (developer decision 45).
    private bool TryStep(Point2D step, MapCollision collision)
    {
        if (TryMoveBy(step, collision))
        {
            return true;
        }

        var isDiagonal = step.X != 0 && step.Y != 0;
        return isDiagonal && (TryMoveBy(step with { Y = 0 }, collision) || TryMoveBy(step with { X = 0 }, collision));
    }

    private bool TryMoveBy(Point2D step, MapCollision collision)
    {
        var target = collision.KeepOnMap(new Point2D(Position.X + step.X, Position.Y + step.Y), rules.PlayerRadius);
        if (!collision.CanStandAt(target, rules.PlayerRadius))
        {
            return false;
        }

        Position = target;
        return true;
    }
}
