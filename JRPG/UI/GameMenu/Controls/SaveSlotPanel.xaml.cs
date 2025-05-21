using JRPG.Core.Battles.Models;
using JRPG.Core.Systems.SaveGames.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JRPG.UI.GameMenu.Controls
{
    /// <summary>
    /// Interaction logic for SaveSlotPanel.xaml
    /// </summary>
    public partial class SaveSlotPanel : UserControl
    {
        private int slotIndex;
        private Action<int> clickCallback;

        public SaveSlotPanel()
        {
            InitializeComponent();
            MouseLeftButtonUp += OnClick;
            Cursor = Cursors.Hand;
        }

        public void SetData(SaveDataPreview data, bool isSaveMode, int index, Action<int> onClick)
        {
            slotIndex = index;
            clickCallback = onClick;

            SlotNameText.Text = $"Slot {index + 1}";
            TimestampText.Text = data.Timestamp.HasValue ? data.Timestamp.Value.ToString("g") : null; // e.g., 5/19/2025 2:45 PM
            MapNameText.Text = data.MapName ?? String.Empty;

            PartyPortraitPanel.Children.Clear();
            foreach (Character c in data.Party)
            {
                var portrait = new CharacterPortrait
                {
                    Character = c,
                    Width = 48,
                    Height = 48,
                    IsSelected = false,
                    ShowStats = false,
                    Margin = new Thickness(4)
                };

                PartyPortraitPanel.Children.Add(portrait);
            }
        }

        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            clickCallback?.Invoke(slotIndex);
        }
    }
}
