using JRPG.Core.Systems;
using JRPG.Core.Systems.SaveGames.Models;
using JRPG.UI.GameMenu.Controls;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JRPG.UI.GameMenu
{
    /// <summary>
    /// Save / Load page: one SaveSlotPanel per file.
    /// Saving happens here; loading is handed to onConfirmedSlotClicked (the caller knows how to switch scenes).
    /// </summary>
    public partial class SaveLoadView : MenuPage
    {
        public enum SaveLoadMode { Save, Load }

        private readonly SaveLoadMode mode;
        private readonly Func<int, Task> onConfirmedSlotClicked;

        // onBack is kept for the existing callers; the page has no back button (Esc / the nav handle that)
        public SaveLoadView(SaveLoadMode mode, Func<int, Task> onConfirmedSlotClicked, Action onBack)
        {
            InitializeComponent();
            this.mode = mode;
            this.onConfirmedSlotClicked = onConfirmedSlotClicked;

            HeaderText.Text = mode == SaveLoadMode.Save ? "Save Game" : "Load Game";
            PopulateSlots();
        }

        private void PopulateSlots()
        {
            SlotListPanel.Children.Clear();

            foreach (var preview in GameSystems.Save.GetAllSavePreviews())
            {
                var slot = new SaveSlotPanel(preview);
                slot.MouseEnter += (s, e) => ShowHint(HintFor(preview));
                slot.MouseLeave += (s, e) => ShowHint(null);
                slot.MouseLeftButtonUp += OnSlotClicked;
                SlotListPanel.Children.Add(slot);
            }
        }

        private string HintFor(SaveDataPreview preview)
        {
            string file = FileName(preview);

            if (mode == SaveLoadMode.Save)
                return preview.IsEmpty ? $"Save to {file}." : $"Overwrite {file}.";

            return preview.IsEmpty || preview.IsCorrupt ? $"{file} can't be loaded." : $"Load {file}.";
        }

        private async void OnSlotClicked(object sender, MouseButtonEventArgs e)
        {
            var preview = ((SaveSlotPanel)sender).Preview;

            if (mode == SaveLoadMode.Save)
                await SaveAsync(preview);
            else if (!preview.IsEmpty && !preview.IsCorrupt)
                await onConfirmedSlotClicked(preview.SlotIndex); // the caller switches scenes
        }

        private async Task SaveAsync(SaveDataPreview preview)
        {
            string file = FileName(preview);

            if (!preview.IsEmpty && !await Dialog.AskAsync($"Overwrite {file}?", "Overwrite"))
                return;

            await GameSystems.System.SaveToSlotAsync(preview.SlotIndex);
            PopulateSlots();
            ShowHint($"Saved to {file}.");
        }

        private static string FileName(SaveDataPreview preview) => $"File {preview.SlotIndex + 1}";
    }
}
