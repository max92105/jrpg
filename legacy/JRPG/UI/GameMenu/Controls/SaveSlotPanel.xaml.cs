using JRPG.Core.Systems.SaveGames.Models;
using JRPG.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace JRPG.UI.GameMenu.Controls
{
    /// <summary>One save file in the Save / Load list. Clicks are handled by SaveLoadView.</summary>
    public partial class SaveSlotPanel : UserControl
    {
        private const double PortraitSize = 52;

        public SaveSlotPanel(SaveDataPreview preview)
        {
            InitializeComponent();
            Preview = preview;
            Cursor = Cursors.Hand;

            SlotNameText.Text = $"File {preview.SlotIndex + 1}";

            if (preview.IsEmpty || preview.IsCorrupt)
            {
                LocationText.Text = preview.IsCorrupt ? "Unreadable file" : "Empty";
                LocationText.Foreground = (Brush)FindResource("Brush.Text.Disabled");
                TimestampText.Visibility = Visibility.Collapsed;
                return;
            }

            LocationText.Text = preview.MapName ?? string.Empty;
            TimestampText.Text = preview.Timestamp?.ToString("g"); // e.g. 5/19/2025 2:45 PM

            foreach (var character in preview.Party)
                PartyPortraitPanel.Children.Add(CreatePortrait(character.PortraitFile));
        }

        public SaveDataPreview Preview { get; }

        private Border CreatePortrait(string portraitFile)
        {
            return new Border
            {
                Width = PortraitSize,
                Height = PortraitSize,
                Margin = new Thickness(6, 0, 0, 0),
                BorderBrush = (Brush)FindResource("Brush.Stroke"),
                BorderThickness = new Thickness(1),
                Child = new Image
                {
                    Source = ImageLoader.LoadPortrait(portraitFile),
                    Stretch = Stretch.UniformToFill
                }
            };
        }
    }
}
