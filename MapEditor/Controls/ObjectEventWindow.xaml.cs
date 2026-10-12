using JRPG.Core.Events.Models;
using JRPG.Core.Maps;
using JRPG.Core.Maps.IO;
using JRPG.Core.Maps.Models;
using JRPG.Core.Systems.Inventories;
using JRPG.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace MapEditor.Controls
{
    /// <summary>
    /// Edits the event of a placed object: when it starts, its prompt, "only once", and its
    /// list of actions. Works on a copy; Result is the edited event (null = remove it).
    /// </summary>
    public partial class ObjectEventWindow : Window
    {
        private readonly ObjectDefinition definition;
        private readonly string currentMapFile;
        private readonly MapEvent working;

        public MapEvent Result { get; private set; }

        public ObjectEventWindow(ObjectDefinition definition, MapEvent existing, string currentMapFile)
        {
            InitializeComponent();

            this.definition = definition;
            this.currentMapFile = currentMapFile;
            working = existing?.Clone() ?? new MapEvent();

            ObjectText.Text = $"Object: {definition?.Name ?? "?"}";
            LoadEventFields();
            RefreshActionList(working.Actions.Count > 0 ? 0 : -1);
        }

        // === Event settings ===

        private void LoadEventFields()
        {
            ActivationBox.SelectedItem = ActivationBox.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(i => (string)i.Tag == working.Activation.ToString());
            PromptBox.Text = working.Prompt ?? "";
            OnceBox.IsChecked = working.Once;
        }

        private void ReadEventFields()
        {
            if (ActivationBox.SelectedItem is ComboBoxItem item &&
                Enum.TryParse((string)item.Tag, out EventActivation activation))
                working.Activation = activation;

            working.Prompt = PromptBox.Text.Trim();
            working.Once = OnceBox.IsChecked == true;
        }

        // === Action list ===

        private string ObjectName(int id) =>
            ObjectDatabase.Definitions.TryGetValue(id, out var def) ? def.Name : $"#{id} (missing)";

        private void RefreshActionList(int select)
        {
            ActionList.Items.Clear();
            for (int i = 0; i < working.Actions.Count; i++)
                ActionList.Items.Add(new ListBoxItem { Content = $"{i + 1}. {working.Actions[i].Describe(ObjectName)}" });

            ActionList.SelectedIndex = Math.Min(select, working.Actions.Count - 1);
            if (ActionList.SelectedIndex < 0)
                ShowActionEditor(null);
        }

        /// <summary>Updates the label of the selected action after one of its fields changed.</summary>
        private void RefreshSelectedLabel()
        {
            int index = ActionList.SelectedIndex;
            if (index >= 0 && ActionList.Items[index] is ListBoxItem item)
                item.Content = $"{index + 1}. {working.Actions[index].Describe(ObjectName)}";
        }

        private EventAction SelectedAction =>
            ActionList.SelectedIndex >= 0 && ActionList.SelectedIndex < working.Actions.Count
                ? working.Actions[ActionList.SelectedIndex]
                : null;

        private void ActionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowActionEditor(SelectedAction);
        }

        private void AddAction_Click(object sender, RoutedEventArgs e)
        {
            if (!(NewActionTypeBox.SelectedItem is ComboBoxItem item) ||
                !Enum.TryParse((string)item.Tag, out EventActionType type))
                return;

            working.Actions.Add(CreateAction(type));
            RefreshActionList(working.Actions.Count - 1);
        }

        private EventAction CreateAction(EventActionType type)
        {
            var action = new EventAction { Type = type };
            switch (type)
            {
                case EventActionType.Dialogue:
                    action.Lines = new List<string> { "..." };
                    break;
                case EventActionType.GiveItem:
                    action.ItemId = InventoryManager.KnownItemIds.FirstOrDefault();
                    action.Quantity = 1;
                    break;
                case EventActionType.SwapObject:
                    action.ObjectId = GuessOpenVersion();
                    break;
                case EventActionType.Warp:
                    action.TargetMap = MapFiles().FirstOrDefault(f => !SameFile(f, currentMapFile)) ?? MapFiles().FirstOrDefault();
                    break;
            }
            return action;
        }

        private void RemoveAction_Click(object sender, RoutedEventArgs e)
        {
            int index = ActionList.SelectedIndex;
            if (index < 0) return;

            working.Actions.RemoveAt(index);
            RefreshActionList(index);
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e) => MoveSelected(-1);
        private void MoveDown_Click(object sender, RoutedEventArgs e) => MoveSelected(+1);

        private void MoveSelected(int direction)
        {
            int index = ActionList.SelectedIndex;
            int target = index + direction;
            if (index < 0 || target < 0 || target >= working.Actions.Count) return;

            var action = working.Actions[index];
            working.Actions.RemoveAt(index);
            working.Actions.Insert(target, action);
            RefreshActionList(target);
        }

        // === Selected action editor ===

        private void ShowActionEditor(EventAction action)
        {
            ActionEditorPanel.Children.Clear();

            if (action == null)
            {
                AddHint("Add an action, or start from a preset (Chest, Door, Sign).");
                return;
            }

            switch (action.Type)
            {
                case EventActionType.Dialogue:
                    AddHeader("Dialogue");
                    AddTextField("Speaker (optional)", action.Speaker, v => action.Speaker = v);
                    AddComboField("Portrait (optional)", PortraitFiles(), action.Portrait, true, v => action.Portrait = string.IsNullOrWhiteSpace(v) ? null : v);
                    AddLabel("Text: one line = one page");
                    var linesBox = new TextBox
                    {
                        Text = string.Join(Environment.NewLine, action.Lines ?? new List<string>()),
                        AcceptsReturn = true,
                        TextWrapping = TextWrapping.Wrap,
                        Height = 180,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                    };
                    linesBox.TextChanged += (s, e) =>
                    {
                        action.Lines = linesBox.Text
                            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .ToList();
                        RefreshSelectedLabel();
                    };
                    ActionEditorPanel.Children.Add(linesBox);
                    break;

                case EventActionType.GiveItem:
                    AddHeader("Give item");
                    AddComboField("Item", InventoryManager.KnownItemIds, action.ItemId, true, v => action.ItemId = v);
                    AddTextField("Quantity", action.Quantity.ToString(), v => { if (int.TryParse(v, out var q) && q > 0) action.Quantity = q; });
                    AddHint("The game shows \"Obtained ...!\" automatically.");
                    break;

                case EventActionType.SwapObject:
                    AddHeader("Swap object");
                    AddHint("The object turns into another object, e.g. a closed chest into an open chest. It stays that way (remembered in the save).");
                    var objectBox = new ComboBox { Margin = new Thickness(0, 4, 0, 8) };
                    foreach (var def in ObjectDatabase.Definitions.Values.OrderBy(d => d.Category).ThenBy(d => d.Name))
                        objectBox.Items.Add(new ComboBoxItem { Content = $"{def.Name}  ({def.Category})", Tag = def.Id });
                    objectBox.SelectedItem = objectBox.Items.OfType<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == action.ObjectId);
                    objectBox.SelectionChanged += (s, e) =>
                    {
                        if (objectBox.SelectedItem is ComboBoxItem selected)
                        {
                            action.ObjectId = (int)selected.Tag;
                            RefreshSelectedLabel();
                        }
                    };
                    AddLabel("Becomes");
                    ActionEditorPanel.Children.Add(objectBox);
                    break;

                case EventActionType.Warp:
                    AddHeader("Warp to map");
                    AddComboField("Map", MapFiles(), action.TargetMap, true, v => action.TargetMap = v);
                    AddTextField("Tile X", action.TargetX.ToString(), v => { if (int.TryParse(v, out var x)) action.TargetX = x; });
                    AddTextField("Tile Y", action.TargetY.ToString(), v => { if (int.TryParse(v, out var y)) action.TargetY = y; });
                    AddHint("Where the player arrives. Open that map in the editor and read the tile under the mouse in the status bar. Pick a spot next to the door, not on it.");
                    break;

                case EventActionType.Battle:
                    AddHeader("Start battle");
                    AddHint("Starts a battle (goblins for now). Actions after it are skipped.");
                    break;
            }
        }

        private void AddHeader(string text)
        {
            ActionEditorPanel.Children.Add(new TextBlock { Text = text, FontWeight = FontWeights.Bold, FontSize = 14, Margin = new Thickness(0, 0, 0, 8) });
        }

        private void AddLabel(string text)
        {
            ActionEditorPanel.Children.Add(new TextBlock { Text = text, Margin = new Thickness(0, 4, 0, 2) });
        }

        private void AddHint(string text)
        {
            ActionEditorPanel.Children.Add(new TextBlock { Text = text, Foreground = Brushes.Gray, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 8) });
        }

        private void AddTextField(string label, string value, Action<string> update)
        {
            AddLabel(label);
            var box = new TextBox { Text = value ?? "", Margin = new Thickness(0, 0, 0, 8) };
            box.TextChanged += (s, e) => { update(box.Text.Trim()); RefreshSelectedLabel(); };
            ActionEditorPanel.Children.Add(box);
        }

        private void AddComboField(string label, IEnumerable<string> options, string value, bool editable, Action<string> update)
        {
            AddLabel(label);
            var box = new ComboBox { IsEditable = editable, Margin = new Thickness(0, 0, 0, 8) };
            foreach (var option in options)
                box.Items.Add(option);
            box.Text = value ?? "";

            // Editable combo: typing and picking both end up in Text
            box.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler((s, e) => { update(box.Text.Trim()); RefreshSelectedLabel(); }));
            box.SelectionChanged += (s, e) =>
            {
                if (box.SelectedItem is string selected)
                {
                    update(selected);
                    RefreshSelectedLabel();
                }
            };
            ActionEditorPanel.Children.Add(box);
        }

        // === Presets ===

        private void PresetChest_Click(object sender, RoutedEventArgs e)
        {
            working.Activation = EventActivation.Interact;
            working.Prompt = "Open";
            working.Once = true;
            working.Actions = new List<EventAction>
            {
                CreateAction(EventActionType.GiveItem),
                CreateAction(EventActionType.SwapObject)
            };
            LoadEventFields();
            RefreshActionList(1); // show the swap: check it points to the open chest
        }

        private void PresetDoor_Click(object sender, RoutedEventArgs e)
        {
            working.Activation = EventActivation.Touch;
            working.Prompt = "Enter";
            working.Once = false;
            working.Actions = new List<EventAction> { CreateAction(EventActionType.Warp) };
            LoadEventFields();
            RefreshActionList(0);
        }

        private void PresetSign_Click(object sender, RoutedEventArgs e)
        {
            working.Activation = EventActivation.Interact;
            working.Prompt = "Read";
            working.Once = false;
            working.Actions = new List<EventAction> { CreateAction(EventActionType.Dialogue) };
            LoadEventFields();
            RefreshActionList(0);
        }

        /// <summary>Best guess for the "open" version of this object (e.g. "Chest" -> "Chest Open").</summary>
        private int GuessOpenVersion()
        {
            var candidates = ObjectDatabase.Definitions.Values
                .Where(d => definition == null || d.Id != definition.Id)
                .Where(d => d.Name != null && d.Name.IndexOf("open", StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            var sameCategory = candidates.FirstOrDefault(d => definition != null && d.Category == definition.Category);
            return (sameCategory ?? candidates.FirstOrDefault())?.Id ?? definition?.Id ?? 0;
        }

        // === Files ===

        private static IEnumerable<string> MapFiles()
        {
            string folder = AssetPathResolver.GetPath(MapConstant.MapFolderPath);
            return Directory.Exists(folder)
                ? Directory.GetFiles(folder, "*.json").Select(Path.GetFileName).OrderBy(f => f)
                : Enumerable.Empty<string>();
        }

        private static IEnumerable<string> PortraitFiles()
        {
            string folder = AssetPathResolver.GetPath("Portraits");
            var files = Directory.Exists(folder)
                ? Directory.GetFiles(folder, "*.png").Select(Path.GetFileName).OrderBy(f => f)
                : Enumerable.Empty<string>();
            return new[] { "" }.Concat(files);
        }

        private static bool SameFile(string a, string b) =>
            string.Equals(Path.GetFileName(a ?? ""), Path.GetFileName(b ?? ""), StringComparison.OrdinalIgnoreCase);

        // === OK / remove ===

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            ReadEventFields();

            if (working.Actions.Count == 0)
            {
                var answer = MessageBox.Show(this, "This event has no actions, so it will do nothing. Keep it anyway?",
                    "Object Event", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (answer != MessageBoxResult.Yes) return;
            }

            Result = working;
            DialogResult = true;
        }

        private void RemoveEvent_Click(object sender, RoutedEventArgs e)
        {
            Result = null;
            DialogResult = true;
        }
    }
}
