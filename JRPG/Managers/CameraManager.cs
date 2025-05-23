using JRPG.Core.Maps.Models;

namespace JRPG.Managers
{
    public class CameraManager
    {
        public double X { get; private set; }
        public double Y { get; private set; }

        public double PreviousX { get; private set; }
        public double PreviousY { get; private set; }

        public double ViewWidthTiles { get; }
        public double ViewHeightTiles { get; }

        private double easing;

        public CameraManager(double viewWidthTiles, double viewHeightTiles, double easingFactor = 0.35)
        {
            ViewWidthTiles = viewWidthTiles;
            ViewHeightTiles = viewHeightTiles;
            easing = easingFactor;
        }

        public void SetEasing(double value)
        {
            easing = value;
        }

        public void SnapTo(double centerX, double centerY, Map map)
        {
            var clamped = ClampToBounds(centerX, centerY, map);
            X = clamped.x;
            Y = clamped.y;
        }

        public void Update(double centerX, double centerY, Map map)
        {
            PreviousX = X;

            var target = ClampToBounds(centerX, centerY, map);
            X += (target.x - X) * easing;

            PreviousY = Y;
            Y += (target.y - Y) * easing;
        }

        private (double x, double y) ClampToBounds(double centerX, double centerY, Map map)
        {
            double minX = 0;
            double minY = 0;
            double maxX = map.Width - ViewWidthTiles;
            double maxY = map.Height - ViewHeightTiles;

            double targetX = centerX - ViewWidthTiles / 2.0;
            double targetY = centerY - ViewHeightTiles / 2.0;

            // Center if map is smaller than view
            if (map.Width <= ViewWidthTiles)
                targetX = (map.Width - ViewWidthTiles) / 2.0;
            else
                targetX = Clamp(targetX, minX, maxX);

            if (map.Height <= ViewHeightTiles)
                targetY = (map.Height - ViewHeightTiles) / 2.0;
            else
                targetY = Clamp(targetY, minY, maxY);

            return (targetX, targetY);
        }

        private double Clamp(double val, double min, double max)
        {
            return (val < min) ? min : (val > max) ? max : val;
        }
    }
}