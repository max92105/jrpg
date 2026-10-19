using JRPG.Core.Battles.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace JRPG.UI.Battle.Controls
{
    /// <summary>
    /// One party member's card on the victory screen. PlayAsync fills the EXP bar level by level:
    /// each time it reaches the end, the level ticks up, "Level Up!" pops, and the bar starts over.
    /// </summary>
    public partial class ExpGainRow : UserControl
    {
        private static readonly Duration AppearDuration = TimeSpan.FromMilliseconds(300);
        private const double FullBarMs = 1100; // time to fill an empty bar to the end
        private const double MinStepMs = 200;
        private const double PopScale = 1.5;

        private readonly ExpGain gain;
        private bool isSkipped;

        // Step being filled (the numbers follow the bar while it moves)
        private ExpStep currentStep;
        private TaskCompletionSource<bool> runningFill;

        public ExpGainRow(ExpGain gain)
        {
            InitializeComponent();
            this.gain = gain;

            NameText.Text = gain.Character.Name;

            // Start from where the character was before the battle's EXP
            var first = gain.Steps.FirstOrDefault();
            if (first != null)
                ShowBar(first.Level, first.FromEXP, first.EXPToNextLevel);
            else
                ShowBar(gain.Character.Level, gain.Character.EXP, gain.Character.EXPToNextLevel);
        }

        /// <summary>Fades and slides the card in, after a delay (cards come in one after another).</summary>
        public void Appear(TimeSpan delay)
        {
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, AppearDuration) { BeginTime = delay });
            AppearSlide.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(0, AppearDuration) { BeginTime = delay, EasingFunction = ease });
        }

        /// <summary>Plays the EXP gain, level by level. Returns early if Skip() is called.</summary>
        public async Task PlayAsync()
        {
            foreach (var step in gain.Steps)
            {
                if (isSkipped)
                    return;

                currentStep = step;
                await FillBarAsync(step);

                if (isSkipped)
                    return;

                if (step.LeveledUp)
                    ShowLevelUp(step.Level + 1);
            }

            // Settle on the real final state (after a level up, the bar starts the new level)
            currentStep = null;
            ShowBar(gain.Character.Level, gain.Character.EXP, gain.Character.EXPToNextLevel);
        }

        /// <summary>Jumps straight to the end: final level, final bar.</summary>
        public void Skip()
        {
            if (isSkipped)
                return;
            isSkipped = true;

            runningFill?.TrySetResult(true);
            currentStep = null;

            BeginAnimation(OpacityProperty, null);
            Opacity = 1;
            AppearSlide.BeginAnimation(TranslateTransform.XProperty, null);
            AppearSlide.X = 0;

            var character = gain.Character;
            ShowBar(character.Level, character.EXP, character.EXPToNextLevel);

            if (gain.Steps.Any(step => step.LeveledUp))
            {
                LevelUpText.BeginAnimation(OpacityProperty, null);
                LevelUpText.Opacity = 1;
            }
        }

        // ============ Bar ============

        private void ShowBar(int level, int exp, int expToNextLevel)
        {
            LevelText.Text = level.ToString();

            ExpBar.BeginAnimation(ProgressBar.ValueProperty, null);
            ExpBar.Maximum = Math.Max(1, expToNextLevel);
            ExpBar.Value = exp;
            ShowNumbers(exp, expToNextLevel);
        }

        private void ShowNumbers(int exp, int expToNextLevel)
        {
            ExpText.Text = exp.ToString("N0");
            MaxExpText.Text = $"/{expToNextLevel:N0}";
            NextLevelText.Text = Math.Max(0, expToNextLevel - exp).ToString("N0");
        }

        private Task FillBarAsync(ExpStep step)
        {
            ExpBar.BeginAnimation(ProgressBar.ValueProperty, null);
            ExpBar.Maximum = Math.Max(1, step.EXPToNextLevel);
            ExpBar.Value = step.FromEXP;

            // A small gain fills quickly, a whole level takes FullBarMs
            double fraction = (step.ToEXP - step.FromEXP) / ExpBar.Maximum;
            var duration = TimeSpan.FromMilliseconds(Math.Max(MinStepMs, FullBarMs * fraction));

            var fill = new DoubleAnimation(step.FromEXP, step.ToEXP, duration)
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            var done = new TaskCompletionSource<bool>();
            runningFill = done;
            fill.Completed += (s, e) => done.TrySetResult(true);

            ExpBar.BeginAnimation(ProgressBar.ValueProperty, fill);
            return done.Task;
        }

        /// <summary>The numbers count up with the bar.</summary>
        private void ExpBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (currentStep != null)
                ShowNumbers((int)Math.Round(ExpBar.Value), currentStep.EXPToNextLevel);
        }

        // ============ Level up ============

        private void ShowLevelUp(int newLevel)
        {
            LevelText.Text = newLevel.ToString();
            Pop(LevelPop);
            Pop(LevelUpPop);
            LevelUpText.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(200)));
        }

        private static void Pop(ScaleTransform scale)
        {
            var pop = new DoubleAnimation(PopScale, 1.0, TimeSpan.FromMilliseconds(350))
            {
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
            };
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, pop);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, pop);
        }
    }
}
