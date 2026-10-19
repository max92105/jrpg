using JRPG.Core.Systems;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace JRPG.UI.GameMenu.Controls
{
    /// <summary>Top bar of the game menu: page title on the left, location and play time on the right.</summary>
    public partial class MenuHeader : UserControl
    {
        private readonly DispatcherTimer clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

        public MenuHeader()
        {
            InitializeComponent();
            clock.Tick += (s, e) => UpdatePlayTime();
        }

        public string Title
        {
            get => TitleText.Text;
            set => TitleText.Text = value;
        }

        /// <summary>Refreshes the info and keeps the play time ticking. Call when the menu opens.</summary>
        public void Start()
        {
            UpdateLocation();
            UpdatePlayTime();
            clock.Start();
        }

        public void Stop() => clock.Stop();

        private void UpdatePlayTime()
        {
            TimeSpan time = GameSystems.System.GetPlayTime();
            PlayTimeText.Text = $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}";
        }

        private void UpdateLocation()
        {
            string mapFile = GameSystems.System.GetCurrentMapFile();

            LocationGroup.Visibility = string.IsNullOrEmpty(mapFile) ? Visibility.Collapsed : Visibility.Visible;
            LocationText.Text = Path.GetFileNameWithoutExtension(mapFile);
        }
    }
}
