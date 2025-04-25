using System;
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
        private string fullText;
        private int index = 0;
        private DispatcherTimer timer;
        private Action onComplete;
        private bool awaitingKeyPress = false;

        public bool IsActive => container.Visibility == System.Windows.Visibility.Visible;

        public DialogueManager(TextBlock outputBlock, Border boxContainer, Image portraitImage)
        {
            textBlock = outputBlock;
            container = boxContainer;
            portrait = portraitImage;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += (s, e) => TypeNextCharacter();
        }

        public void Show(string text, string portraitPath = null, Action callback = null)
        {
            fullText = text;
            index = 0;
            onComplete = callback;
            awaitingKeyPress = false;

            textBlock.Text = "";
            container.Visibility = System.Windows.Visibility.Visible;

            if (!string.IsNullOrEmpty(portraitPath))
            {
                portrait.Source = new BitmapImage(new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, portraitPath)));
                portrait.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                portrait.Source = null;
                portrait.Visibility = System.Windows.Visibility.Collapsed;
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
        }

        public void Skip()
        {
            if (!IsActive) return;

            if (!awaitingKeyPress)
            {
                timer.Stop();
                textBlock.Text = fullText;
                awaitingKeyPress = true;
            }
            else
            {
                Hide();
                onComplete?.Invoke();
            }
        }

        public void Hide()
        {
            timer.Stop();
            container.Visibility = System.Windows.Visibility.Collapsed;
            awaitingKeyPress = false;
        }
    }
}