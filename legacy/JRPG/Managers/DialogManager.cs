using JRPG.Core.Utils;
using JRPG.Managers;
using JRPG.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace JRPG
{
    public class DialogueManager
    {
        private TextBlock textBlock;
        private Border container;
        private Image portrait;
        private TextBlock speakerName;
        private TextBlock continueIcon;
        private List<string> pages = new List<string>();
        private int currentPage = 0;
        private string fullText;
        private int index = 0;
        private DispatcherTimer timer;
        private Action onComplete;
        private bool awaitingKeyPress = false;

        private DispatcherTimer blinkTimer;
        private bool isBlinkVisible = true;

        public bool IsActive => container.Visibility == System.Windows.Visibility.Visible;

        public DialogueManager(TextBlock outputBlock, Border boxContainer, Image portraitImage, TextBlock speakerBlock, TextBlock continueIndicator)
        {
            textBlock = outputBlock;
            container = boxContainer;
            portrait = portraitImage;
            speakerName = speakerBlock;
            continueIcon = continueIndicator;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += (s, e) => TypeNextCharacter();

            blinkTimer = new DispatcherTimer();
            blinkTimer.Interval = TimeSpan.FromMilliseconds(500);
            blinkTimer.Tick += (s, e) => BlinkIndicator();
        }

        public void ShowWithFlow(List<string> pages, string portraitPath = null, string speaker = null, Action callback = null)
        {
            GameFlowManager.StartDialogue();

            Show(pages, portraitPath, speaker, () =>
            {
                GameFlowManager.EndDialogue();
                callback?.Invoke();
            });
        }

        public void Show(List<string> dialoguePages, string portraitPath = null, string speaker = null, Action callback = null)
        {
            pages = dialoguePages;
            currentPage = 0;
            fullText = pages[currentPage];
            index = 0;
            onComplete = callback;
            awaitingKeyPress = false;

            textBlock.Text = "";
            container.Visibility = Visibility.Visible;

            if (!string.IsNullOrEmpty(portraitPath))
            {
                portrait.Source = ImageLoader.Load(AssetPathResolver.GetPath(Path.Combine("Portraits/", portraitPath)), 256);
                portrait.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                portrait.Source = null;
                portrait.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (!string.IsNullOrEmpty(speaker))
            {
                speakerName.Text = speaker;
                speakerName.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                speakerName.Text = "";
                speakerName.Visibility = System.Windows.Visibility.Collapsed;
            }

            timer.Start();
        }

        private void TypeNextCharacter()
        {
            if (index >= fullText.Length)
            {
                timer.Stop();
                awaitingKeyPress = true;
                return;
            }

            textBlock.Text += fullText[index];
            index++;

            if (currentPage < pages.Count - 1)
                continueIcon.Text = "▶";
            else
                continueIcon.Text = "✓";

            continueIcon.Visibility = Visibility.Visible;
            blinkTimer.Start();
        }

        public void Skip()
        {
            if (!IsActive) return;

            if (!awaitingKeyPress)
            {
                timer.Stop();
                textBlock.Text = fullText;
                awaitingKeyPress = true;
                continueIcon.Visibility = Visibility.Visible; // show indicator after skipping
            }
            else
            {
                continueIcon.Visibility = Visibility.Collapsed;
                blinkTimer.Stop();
                continueIcon.Opacity = 1.0;

                currentPage++;
                if (currentPage < pages.Count)
                {
                    fullText = pages[currentPage];
                    textBlock.Text = "";
                    index = 0;
                    awaitingKeyPress = false;
                    continueIcon.Visibility = Visibility.Collapsed; // hide again when new page starts
                    timer.Start();
                }
                else
                {
                    Hide();
                    onComplete?.Invoke();
                }
            }
        }

        public void Hide()
        {
            timer.Stop();
            container.Visibility = Visibility.Collapsed;
            continueIcon.Visibility = Visibility.Collapsed;
            continueIcon.Visibility = Visibility.Collapsed;
            blinkTimer.Stop();
            continueIcon.Opacity = 1.0;

            awaitingKeyPress = false;
        }

        private void BlinkIndicator()
        {
            if (!continueIcon.IsVisible)
                return;

            isBlinkVisible = !isBlinkVisible;
            continueIcon.Opacity = isBlinkVisible ? 1.0 : 0.2;
        }
    }
}