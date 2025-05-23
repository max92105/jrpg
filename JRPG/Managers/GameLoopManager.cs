using System;
using System.Windows.Threading;
using JRPG.Core.Utils;

namespace JRPG
{
    public class GameLoopManager
    {
        private readonly Action update;
        private readonly Action<double> render;

        private readonly DispatcherTimer timer;
        private DateTime lastTime;
        private double accumulator = 0.0;

        private const double TargetDelta = 1.0 / 60.0; // 60 FPS fixed timestep

        public GameLoopManager(Action update, Action<double> render)
        {
            this.update = update;
            this.render = render;

            lastTime = DateTime.Now;
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1) // Frequent enough to simulate real loop
            };
            timer.Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            double frameTime = (now - lastTime).TotalSeconds;
            lastTime = now;

            // Avoid spiral of death
            frameTime = frameTime.Clamp(0, 0.25);

            accumulator += frameTime;

            while (accumulator >= TargetDelta)
            {
                update?.Invoke(); // Call game.Update()
                accumulator -= TargetDelta;
            }

            double interpolationFactor = (accumulator / TargetDelta).Clamp(0, 1);
            render?.Invoke(interpolationFactor); // Call game.Render(interpolation)
        }

        public void Start() => timer.Start();
        public void Stop() => timer.Stop();
    }
}