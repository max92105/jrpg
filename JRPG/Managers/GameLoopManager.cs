using System;
using System.Diagnostics;
using System.Windows.Media;
using JRPG.Core.Utils;

namespace JRPG
{
    public class GameLoopManager
    {
        private readonly Action update;
        private readonly Action<double> render;

        // DateTime.Now only ticks every ~15ms, which made movement uneven; Stopwatch is precise
        private readonly Stopwatch clock = Stopwatch.StartNew();
        private double lastTime;
        private double accumulator;

        public const double TargetDelta = 1.0 / 60.0; // 60 FPS fixed update

        public GameLoopManager(Action update, Action<double> render)
        {
            this.update = update;
            this.render = render;
            lastTime = clock.Elapsed.TotalSeconds;
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
            double now = clock.Elapsed.TotalSeconds;
            double frameTime = now - lastTime;
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