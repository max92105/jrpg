using JRPG.Core.Battles.Enums;
using JRPG.Core.Battles.Models;
using JRPG.Core.Systems.Inventories.Models;
using JRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace JRPG.UI.Battle.Controls
{
    /// <summary>
    /// The battle command menu. It only shows choices and raises events;
    /// BattleUI decides what each choice does.
    /// </summary>
    public partial class CommandPanel : UserControl
    {
        private const double UnavailableOpacity = 0.4;

        /// <summary>"Attack", "Skill", "Item", "Defend" or "Run".</summary>
        public event Action<string> OnCommandSelected;
        public event Action<Skill> OnSkillSelected;
        public event Action<InventoryItem> OnItemSelected;

        private string actorName;

        public CommandPanel()
        {
            InitializeComponent();
        }

        // ============ Lists ============

        /// <summary>The main list, for whoever is acting.</summary>
        public void ShowCommands(string actingName)
        {
            actorName = actingName;
            ShowCommands();
        }

        /// <summary>Back to the main list (e.g. from Skills, or on right-click).</summary>
        public void ShowCommands()
        {
            HeaderText.Text = actorName;
            ClearRows();

            AddRow("Attack", "Attack.png", null, "Strike one enemy with your weapon.", () => OnCommandSelected?.Invoke("Attack"));
            AddRow("Skills", "Skill.png", null, "Use a skill. Skills cost MP.", () => OnCommandSelected?.Invoke("Skill"));
            AddRow("Items", "Item.png", null, "Use an item from the bag.", () => OnCommandSelected?.Invoke("Item"));
            AddRow("Defend", "Defend.png", null, "Brace yourself and end your turn.", () => OnCommandSelected?.Invoke("Defend"));
            AddRow("Run", "Run.png", null, "Escape from the battle.", () => OnCommandSelected?.Invoke("Run"));
        }

        public void ShowSkills(BattleActor actor)
        {
            ClearRows();

            foreach (var skill in actor.Skills)
            {
                string notEnoughMp = actor.MP < skill.MPCost ? "Not enough MP." : null;

                AddRow(skill.Name, skill.IconFile, $"{skill.MPCost} MP", DescribeSkill(skill),
                    () => OnSkillSelected?.Invoke(skill), notEnoughMp);
            }

            AddBackRow();
        }

        public void ShowItems(IEnumerable<InventoryItem> inventory)
        {
            ClearRows();

            foreach (var entry in inventory.Where(e => e.Item is Consumable && e.Quantity > 0))
            {
                AddRow(entry.Item.Name, entry.Item.IconFile, $"x{entry.Quantity}", entry.Item.Description,
                    () => OnItemSelected?.Invoke(entry));
            }

            AddBackRow();
        }

        /// <summary>Shows a line of help under the list, e.g. "Choose a target". Null hides it.</summary>
        public void ShowMessage(string text)
        {
            DescriptionText.Text = text;
            DescriptionBox.Visibility = string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible;
        }

        // ============ Rows ============

        private void ClearRows()
        {
            RowsPanel.Children.Clear();
            ShowMessage(null);
        }

        private void AddBackRow()
        {
            AddRow("Back", null, null, "Return to the commands.", ShowCommands);
        }

        /// <summary>
        /// Adds one row:  [icon] Text ........ value
        /// </summary>
        /// <param name="unavailableReason">If set, the row is greyed out and clicking it explains why instead.</param>
        private void AddRow(string text, string iconFile, string value, string description, Action onClick, string unavailableReason = null)
        {
            var content = new DockPanel();

            var valueText = new TextBlock { Text = value, Style = (Style)FindResource("Text.Number"), FontSize = 16 };
            DockPanel.SetDock(valueText, Dock.Right);
            content.Children.Add(valueText);

            var icon = new Image { Source = ImageLoader.LoadIcon(iconFile), Width = 24, Height = 24, Margin = new Thickness(0, 0, 12, 0) };
            DockPanel.SetDock(icon, Dock.Left);
            content.Children.Add(icon);

            content.Children.Add(new TextBlock { Text = text, Style = (Style)FindResource("Text.Body"), FontSize = 20 });

            var row = new Button
            {
                Content = content,
                Style = (Style)FindResource("BattleCommandStyle"),
                Opacity = unavailableReason == null ? 1.0 : UnavailableOpacity
            };

            row.MouseEnter += (s, e) => ShowMessage(description);
            row.Click += (s, e) =>
            {
                if (unavailableReason != null)
                    ShowMessage(unavailableReason);
                else
                    onClick();
            };

            RowsPanel.Children.Add(row);
        }

        // ============ Descriptions ============

        /// <summary>Skills have no description yet, so one is built from what they do, e.g. "Deals 15 damage to one enemy."</summary>
        private static string DescribeSkill(Skill skill)
        {
            string effect;
            switch (skill.EffectType)
            {
                case EffectType.Damage: effect = $"Deals {skill.EffectValue} damage"; break;
                case EffectType.ManaRecovery: effect = $"Restores {skill.EffectValue} MP"; break;
                default: effect = $"Restores {skill.EffectValue} HP"; break;
            }

            return $"{effect} to {DescribeTarget(skill.TargetType)}.";
        }

        private static string DescribeTarget(TargetType target)
        {
            switch (target)
            {
                case TargetType.Enemy: return "one enemy";
                case TargetType.AllEnemies: return "all enemies";
                case TargetType.Ally: return "one ally";
                case TargetType.Self: return "yourself";
                case TargetType.AllAllies: return "the whole party";
                case TargetType.SingleFaintedAlly: return "a fallen ally";
                case TargetType.AllFaintedAllies: return "all fallen allies";
                default: return "the target";
            }
        }
    }
}
