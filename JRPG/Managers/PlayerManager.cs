using JRPG.Core.Maps.IO;
using JRPG.Core.Maps.Models;
using JRPG.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JRPG.Managers
{
    public class PlayerManager
    {
        // Position and movement
        public double X { get; private set; } = 10.0;
        public double Y { get; private set; } = 3.0;
        public double PreviousX { get; private set; }
        public double PreviousY { get; private set; }

        public double Radius => 0.1;

        public double VelocityX { get; private set; }
        public double VelocityY { get; private set; }
        public double MoveSpeed { get; set; } = 3.0;

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

            PreviousX = X;
            PreviousY = Y;

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
                    if (!TileDatabase.Definitions.TryGetValue(tileId, out var def) || !def.IsWalkable)
                        return false;
                }
            }

            foreach (var obj in map.Objects)
            {
                if (!ObjectDatabase.Definitions.TryGetValue(obj.Id, out var def)) continue;
                if (def.CollisionPolygon == null || def.CollisionPolygon.Count == 0) continue;

                int minX = def.Tiles.Min(t => t.X);
                int minY = def.Tiles.Min(t => t.Y);
                int maxX = def.Tiles.Max(t => t.X);
                int maxY = def.Tiles.Max(t => t.Y);

                int width = (maxX - minX) + 1;
                int height = (maxY - minY) + 1;

                // Reconstruct full polygon in world space
                var polygon = def.CollisionPolygon.Select(p =>
                {
                    double worldX = obj.X + (p.X * width);
                    double worldY = obj.Y + (p.Y * height);
                    return (worldX, worldY);
                }).ToList();

                // Check if player's center is inside polygon
                if (IsPointInsidePolygon(x, y, polygon))
                    return false;
            }

            return true;
        }

        private bool IsPointInsidePolygon(double x, double y, List<(double x, double y)> polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                var xi = polygon[i].x;
                var yi = polygon[i].y;
                var xj = polygon[j].x;
                var yj = polygon[j].y;

                bool intersect = ((yi > y) != (yj > y)) &&
                                 (x < (xj - xi) * (y - yi) / (yj - yi + 0.00001) + xi);
                if (intersect)
                    inside = !inside;
            }
            return inside;
        }

        private double Clamp(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}
