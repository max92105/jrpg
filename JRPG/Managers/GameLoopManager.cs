using System;
using System.Windows.Media;
using JRPG.Core.Utils;

namespace JRPG
{
    public class GameLoopManager
    {
        private readonly Action update;
        private readonly Action<double> render;

        private DateTime lastTime;
        private double accumulator;

        private const double TargetDelta = 1.0 / 60.0; // 60 FPS fixed update

        public GameLoopManager(Action update, Action<double> render)
        {
            this.update = update;
            this.render = render;
            lastTime = DateTime.Now;
        }

        public void Start()
        {
            CompositionTarget.Rendering += OnRendering;
        }

        public void Stop()
        {
            CompositionTarget.Rendering -= OnRendering;
        }

        private void OnRendering(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            double frameTime = (now - lastTime).TotalSeconds;
            lastTime = now;

            frameTime = frameTime.Clamp(0.001, 0.25); // avoid big spikes
            accumulator += frameTime;

            while (accumulator >= TargetDelta)
            {
                update?.Invoke();
                accumulator -= TargetDelta;
            }

            double interpolation = accumulator / TargetDelta;
            render?.Invoke(interpolation.Clamp(0, 1));
        }
    }
}