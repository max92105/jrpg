using JRPG.Core.Systems;
using JRPG.Core.Systems.SaveGames.Models;
using JRPG.UI.GameMenu.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace JRPG.UI.GameMenu
{
    /// <summary>
    /// Interaction logic for SaveLoadView.xaml
    /// </summary>
    public partial class SaveLoadView : UserControl
    {
        public enum SaveLoadMode { Save, Load }

        private SaveLoadMode currentMode;
        private Action onBack;
        private Func<int, Task> onConfirmedSlotClicked;

        public SaveLoadView(
            SaveLoadMode mode,
            Func<int, Task> onConfirmedSlotClicked,
            Action onBack)
        {
            InitializeComponent();
            currentMode = mode;
            this.onConfirmedSlotClicked = onConfirmedSlotClicked; // ✅ missing assignment
            this.onBack = onBack;

            HeaderText.Text = currentMode == SaveLoadMode.Save ? "Save Game" : "Load Game";
            PopulateSlots();
        }

        private void PopulateSlots()
        {
            SlotListPanel.Children.Clear();

            List<SaveDataPreview> previews = GameSystems.Save.GetAllSavePreviews();

            foreach (var preview in previews)
            {
                var slot = new SaveSlotPanel();
                slot.SetData(preview, currentMode == SaveLoadMode.Save, preview.SlotIndex, OnSlotClicked);
                SlotListPanel.Children.Add(slot);
            }
        }

        private async void OnSlotClicked(int slotIndex)
        {
            if (currentMode == SaveLoadMode.Save)
            {
                var confirm = MessageBox.Show(
                    $"Are you sure you want to overwrite Slot {slotIndex + 1}?",
                    "Confirm Save",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (confirm != MessageBoxResult.Yes)
                    return;

                await GameSystems.System.SaveToSlotAsync(slotIndex);
                MessageBox.Show("Game saved!", "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                PopulateSlots();
            }
            else
            {
                await onConfirmedSlotClicked(slotIndex); // Delegate does transition
            }
        }
    }
}
