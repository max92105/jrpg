using JRPG.Core.Maps;
using JRPG.Core.Triggers.Models;
using JRPG.Core.Utils;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Trigger = JRPG.Core.Triggers.Models.Trigger;

namespace MapEditor
{
    public partial class TriggerEditWindow : Window
    {
        public Trigger Result { get; private set; }

        // The map being edited, to catch warps that point back at it by mistake
        private readonly string currentMapFile;

        public TriggerEditWindow(Trigger existing = null, string currentMapFile = null)
        {
            InitializeComponent();
            this.currentMapFile = currentMapFile;

            if (existing is TriggerWarp warp)
            {
                SelectType("Warp");
                WarpMapBox.Text = warp.TargetMap;
                WarpXBox.Text = warp.TargetX.ToString();
                WarpYBox.Text = warp.TargetY.ToString();
            }
            else if (existing is TriggerDialogue dialogue)
            {
                SelectType("Dialogue");
                DialogueSpeakerBox.Text = dialogue.SpeakerName ?? "";
                DialoguePortraitBox.Text = dialogue.PortraitPath ?? "";

                DialogueLinesBox.Items.Clear();
                foreach (var line in dialogue.Lines ?? Enumerable.Empty<string>())
                    DialogueLinesBox.Items.Add(line);
            }
            else if (existing is TriggerItem item)
            {
                SelectType("Item");
                ItemBox.Text = item.ItemId;
            }
            else
            {
                SelectType("Warp");
            }
        }

        // Select by tag rather than index so the list order can change safely
        private void SelectType(string tag)
        {
            TriggerTypeBox.SelectedItem = TriggerTypeBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(i => (string)i.Tag == tag);
        }

        private void TriggerTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WarpPanel.Visibility = Visibility.Collapsed;
            DialoguePanel.Visibility = Visibility.Collapsed;
            ItemPanel.Visibility = Visibility.Collapsed;

            string selected = (TriggerTypeBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();

            if (selected == "Warp") WarpPanel.Visibility = Visibility.Visible;
            else if (selected == "Dialogue") DialoguePanel.Visibility = Visibility.Visible;
            else if (selected == "Item") ItemPanel.Visibility = Visibility.Visible;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            string selected = (TriggerTypeBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();

            try
            {
                switch (selected)
                {
                    case "Warp":
                        if (string.IsNullOrWhiteSpace(WarpMapBox.Text)) throw new Exception("Target map required");
                        if (!int.TryParse(WarpXBox.Text, out int x)) throw new Exception("Invalid X");
                        if (!int.TryParse(WarpYBox.Text, out int y)) throw new Exception("Invalid Y");

                        string targetMap = WarpMapBox.Text.Trim();
                        if (!File.Exists(AssetPathResolver.GetPath(MapConstant.MapFolderPath, targetMap)))
                            throw new Exception($"Map \"{targetMap}\" doesn't exist in the Maps folder");

                        // Warping inside the same map is allowed (teleport), but it's usually a wrong pick
                        bool sameMap = currentMapFile != null &&
                                       string.Equals(Path.GetFileName(currentMapFile), targetMap, StringComparison.OrdinalIgnoreCase);
                        if (sameMap && MessageBox.Show(
                                $"This warp leads to the map it is on ({targetMap}), so the player only teleports inside it.\n\nIs that intended?",
                                "Warp to the same map", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                            return;

                        Result = new TriggerWarp
                        {
                            TargetMap = WarpMapBox.Text.Trim(),
                            TargetX = x,
                            TargetY = y
                        };
                        break;

                    case "Dialogue":
                        var dialogue = new TriggerDialogue
                        {
                            SpeakerName = DialogueSpeakerBox.Text.Trim(),
                            PortraitPath = DialoguePortraitBox.Text.Trim(),
                            Lines = DialogueLinesBox.Items.Cast<string>().ToList()
                        };
                        Result = dialogue;
                        break;

                    case "Item":
                        if (string.IsNullOrWhiteSpace(ItemBox.Text)) throw new Exception("Item name required");

                        Result = new TriggerItem
                        {
                            ItemId = ItemBox.Text.Trim()
                        };
                        break;

                    default:
                        throw new Exception("Choose a trigger type");
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SelectMapFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Map Files (*.json)|*.json",
                Title = "Select Target Map",
                InitialDirectory = Path.Combine(AssetPathResolver.GetPath(MapConstant.MapFolderPath)),
            };

            if (dialog.ShowDialog() == true)
            {
                string selectedPath = dialog.SafeFileName;
                string baseDir = Directory.GetCurrentDirectory();

                // Store a relative path if possible
                if (selectedPath.StartsWith(baseDir))
                {
                    selectedPath = selectedPath.Substring(baseDir.Length).TrimStart('\\', '/');
                }

                WarpMapBox.Text = selectedPath.Replace("\\", "/"); // Normalize for consistency
            }
        }

        private void SelectPortrait_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "PNG Files (*.png)|*.png",
                Title = "Select Portrait",
                InitialDirectory = AssetPathResolver.GetAssetsRootPath()
            };

            if (dialog.ShowDialog() == true)
            {
                string selectedPath = dialog.SafeFileName;
                string baseDir = Directory.GetCurrentDirectory();

                if (selectedPath.StartsWith(baseDir))
                {
                    selectedPath = selectedPath.Substring(baseDir.Length).TrimStart('\\', '/');
                }

                DialoguePortraitBox.Text = selectedPath.Replace("\\", "/");
            }
        }

        private void AddDialogueLine_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NewDialogueLineBox.Text))
            {
                DialogueLinesBox.Items.Add(NewDialogueLineBox.Text.Trim());
                NewDialogueLineBox.Text = "";
            }
        }

        private void RemoveDialogueLine_Click(object sender, RoutedEventArgs e)
        {
            if (DialogueLinesBox.SelectedItem != null)
            {
                DialogueLinesBox.Items.Remove(DialogueLinesBox.SelectedItem);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}