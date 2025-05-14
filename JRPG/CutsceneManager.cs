using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using JRPG.Core.Cutscenes.Models;
using JRPG.Core.Utils;
using JRPG.UI.Cutscene;

namespace JRPG
{
    public class CutsceneManager
    {
        private Queue<CutsceneStep> steps = new Queue<CutsceneStep>();
        private CutscenePlayer CutscenePlayer;
        private Dictionary<string, Image> activeSprites = new Dictionary<string, Image>();

        public Action OnCutsceneEnd;

        public CutsceneManager(CutscenePlayer cutscenePlayer)
        {
            this.CutscenePlayer = cutscenePlayer;
        }

        public void PlayCutscene(List<CutsceneStep> cutscene)
        {
            CutscenePlayer.Visibility = Visibility.Visible;
            CutscenePlayer.CutsceneCanvas.Visibility = Visibility.Visible;

            steps = new Queue<CutsceneStep>(cutscene);
            NextStep();
        }

        private async void NextStep()
        {
            if (steps.Count == 0) return;

            var step = steps.Dequeue();
            switch (step)
            {
                case CutsceneStepFade fade:
                    if (fade.FadeToBlack)
                        await FadeHelper.FadeToBlackAsync(((MainWindow)Application.Current.MainWindow).FadeRect);
                    else
                        await FadeHelper.FadeFromBlackAsync(((MainWindow)Application.Current.MainWindow).FadeRect);
                    NextStep();
                    break;

                case CutsceneStepBackground bg:
                    var bgImage = new Image
                    {
                        Source = new BitmapImage(new Uri(AssetPathResolver.GetBackgroundsPath(bg.BackgroundPath), UriKind.Absolute)),
                        Width = 1920,
                        Height = 1080
                    };
                    Canvas.SetLeft(bgImage, 0);
                    Canvas.SetTop(bgImage, 0);
                    CutscenePlayer.CutsceneCanvas.Children.Add(bgImage);
                    NextStep();
                    break;

                case CutsceneStepSprite sprite:
                    var img = new Image
                    {
                        Source = new BitmapImage(new Uri(AssetPathResolver.GetSpritesPath(sprite.SpritePath), UriKind.Absolute)),
                        Width = 96,
                        Height = 96
                    };
                    if (sprite.Flip)
                    {
                        img.LayoutTransform = new ScaleTransform(-1, 1);
                    }
                    Canvas.SetLeft(img, sprite.X * 96);
                    Canvas.SetTop(img, sprite.Y * 96);
                    CutscenePlayer.CutsceneCanvas.Children.Add(img);
                    activeSprites[sprite.Id] = img;
                    NextStep();
                    break;

                case CutsceneStepDialogue dialogue:
                    ((MainWindow)Application.Current.MainWindow).ShowDialogue(dialogue.Lines, dialogue.PortraitPath, dialogue.SpeakerName, NextStep);
                    break;

                case CutsceneStepMove move:
                    if (activeSprites.TryGetValue(move.Id, out var movingImg))
                    {
                        var startX = Canvas.GetLeft(movingImg);
                        var startY = Canvas.GetTop(movingImg);
                        var endX = move.ToX * 96;
                        var endY = move.ToY * 96;
                        var dx = (endX - startX);
                        var dy = (endY - startY);

                        var frames = (int)(move.Duration * 60);
                        int current = 0;

                        DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000.0 / 60) };
                        timer.Tick += (s, e) =>
                        {
                            current++;
                            double t = (double)current / frames;
                            Canvas.SetLeft(movingImg, startX + dx * t);
                            Canvas.SetTop(movingImg, startY + dy * t);
                            if (current >= frames)
                            {
                                timer.Stop();
                                NextStep();
                            }
                        };
                        timer.Start();
                    }
                    else
                    {
                        NextStep();
                    }
                    break;

                case CutsceneStepEnd _:
                    CutscenePlayer.CutsceneCanvas.Children.Clear();
                    activeSprites.Clear();
                    CutscenePlayer.Visibility = Visibility.Collapsed;
                    CutscenePlayer.CutsceneCanvas.Visibility = Visibility.Collapsed;

                    await FadeHelper.FadeFromBlackAsync(((MainWindow)Application.Current.MainWindow).FadeRect);
                    break;

                default:
                    NextStep();
                    break;
            }
        }

        public void StartIntroCutscene()
        {
            var steps = new List<CutsceneStep>
            {
                    new CutsceneStepFade(fadeToBlack: true, 0.5), // Fade to black
                    new CutsceneStepBackground("Forest.png"),
                    new CutsceneStepSprite("Hero", "PlaceholderCharacter.png", 4.0, 6.0, flip: false),
                    new CutsceneStepSprite("Villain", "Goblin.png", 10.0, 6.0, flip: true),
                    new CutsceneStepFade(fadeToBlack: false, 0.5), // Fade in
          
                    new CutsceneStepDialogue(new List<string> { "Where are we?", "This forest gives me chills..." }, "Hero", "Alaric.png"),
                    new CutsceneStepDialogue(new List<string> { "You're too late, hero.", "The ritual is already underway!" }, "Villain", "Goblin.png"),

                    new CutsceneStepMove("Villain", 6.0, 6.0, 1.5),

                    new CutsceneStepDialogue(new List<string> { "No!" }, "Hero", "Alaric.png"),

                    new CutsceneStepFade(fadeToBlack: true, 0.5), // Fade to black
                    new CutsceneStepEnd() ,// Clean up
            };

            PlayCutscene(steps);
        }
    }
}
