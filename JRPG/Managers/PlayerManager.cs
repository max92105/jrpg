using JRPG.Core.Maps;
using JRPG.Core.Maps.Models;
using JRPG.Enums;
using System;

namespace JRPG.Managers
{
    public class PlayerManager
    {
        // Position and movement
        public double X { get; private set; } = 10.0;
        public double Y { get; private set; } = 3.0;
        public double Radius => 0.1;

        public double VelocityX { get; private set; }
        public double VelocityY { get; private set; }
        public double MoveSpeed { get; set; } = 4.0;

        public Direction Direction { get; private set; } = Direction.Down;

        // Animation
        public int AnimationFrame { get; private set; } = 0;
        public double AnimationSpeed { get; set; } = 10.0;
        private double animationTimer = 0;
        private const int TotalFrames = 8;

        public bool IsMoving => VelocityX != 0 || VelocityY != 0;

        public void SetPosition(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void UpdateMovement(bool up, bool down, bool left, bool right, double deltaTime, Map map)
        {
            VelocityX = 0;
            VelocityY = 0;

            if (left && !right)
            {
                VelocityX = -MoveSpeed;
                Direction = Direction.Left;
            }
            else if (right && !left)
            {
                VelocityX = MoveSpeed;
                Direction = Direction.Right;
            }

            if (up && !down)
            {
                VelocityY = -MoveSpeed;
                Direction = Direction.Up;
            }
            else if (down && !up)
            {
                VelocityY = MoveSpeed;
                Direction = Direction.Down;
            }

            if (!IsMoving)
            {
                AnimationFrame = 0;
                return;
            }

            animationTimer += deltaTime;
            if (animationTimer >= 1.0 / AnimationSpeed)
            {
                AnimationFrame = (AnimationFrame + 1) % TotalFrames;
                animationTimer = 0;
            }

            var newX = Clamp(X + VelocityX * deltaTime, Radius, map.Width - Radius);
            var newY = Clamp(Y + VelocityY * deltaTime, Radius, map.Height - Radius);

            if (CanMoveTo(newX, newY, map))
            {
                X = newX;
                Y = newY;
            }
        }

        private bool CanMoveTo(double x, double y, Map map)
        {
            double left = x - Radius;
            double right = x + Radius;
            double top = y - Radius;
            double bottom = y + Radius;

            for (int ty = (int)Math.Floor(top); ty <= (int)Math.Floor(bottom); ty++)
            {
                for (int tx = (int)Math.Floor(left); tx <= (int)Math.Floor(right); tx++)
                {
                    if (tx < 0 || ty < 0 || tx >= map.Width || ty >= map.Height)
                        return false;

                    int tileId = map.GetTileIdAt(tx, ty);
                    if (!map.TileDefinitions.TryGetValue(tileId, out var def) || !def.IsWalkable)
                        return false;
                }
            }

            foreach (var obj in map.Objects)
            {
                if (!map.ObjectDefinitions.TryGetValue(obj.Id, out var def)) continue;
                if (!def.HasCollision || def.CollisionBox == null) continue;

                var box = def.CollisionBox.Value;

                double worldLeft = obj.X - 0.5 + (box.X / MapConstant.TileSize);
                double worldTop = obj.Y - 0.5 + (box.Y / MapConstant.TileSize);
                double worldRight = worldLeft + (box.Width / MapConstant.TileSize);
                double worldBottom = worldTop + (box.Height / MapConstant.TileSize);

                if (right > worldLeft && left < worldRight &&
                    bottom > worldTop && top < worldBottom)
                {
                    return false;
                }
            }

            return true;
        }

        private double Clamp(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}
