using JRPG.Core.Battles.Enum;
using JRPG.Core.Utils;
using JRPG.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JRPG
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class BattleUI : UserControl
    {
        private BattleManager battle;
        private List<Character> characters;
        private List<Enemy> enemies = new List<Enemy>();
        private List<InventoryItem> inventory;

        private int selectedEnemyIndex = -1;

        public event System.Action OnBattleEnd;

        public BattleUI(List<Character> characters, List<Enemy> enemies, List<InventoryItem> inventory)
        {
            InitializeComponent();
            this.characters = characters;
            this.enemies = enemies;
            this.inventory = inventory;
            battle = new BattleManager(characters, enemies);

            UpdateUI();
        }

        private void UpdateUI()
        {
            AttackButton.IsEnabled = (battle.State == BattleState.PlayerTurn);
            DefendButton.IsEnabled = (battle.State == BattleState.PlayerTurn);
            RunButton.IsEnabled = (battle.State == BattleState.PlayerTurn);
            UseItemButton.IsEnabled = (battle.State == BattleState.PlayerTurn);
            SkillButton.IsEnabled = (battle.State == BattleState.PlayerTurn);

            UpdateCharacterPanel();
            UpdateEnemyPanel();
        }

        private void AttackButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedEnemyIndex == -1)
            {
                MessageBox.Show("Select an enemy to attack!");
                return;
            }

            var aliveEnemies = battle.GetAliveEnemies();
            var target = aliveEnemies[selectedEnemyIndex];

            var result = battle.PlayerAttack(target);

            ShowFloatingText(result.ToString(), FloatingTextType.Damage, EnemyPanelContainer.Children[selectedEnemyIndex]);

            if (battle.State != BattleState.Won)
                battle.NextPlayerTurn();

            selectedEnemyIndex = -1;

            CheckNextState();
            UpdateUI();
        }

        private void DefendButton_Click(object sender, RoutedEventArgs e)
        {
            var result = battle.PlayerDefend();
            MessageBox.Show(result);

            if (battle.State != BattleState.Won)
                battle.NextPlayerTurn();

            CheckNextState();
            UpdateUI();
        }

        private void UseItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (inventory == null || !inventory.Any(i => i.Quantity > 0))
            {
                MessageBox.Show("No items available!");
                return;
            }

            var window = CreateSelectionWindow("Select Item", inventory.Select(i => $"{i.Name} (x{i.Quantity})"));

            var list = (ListBox)window.Content;
            list.SelectionChanged += (s, ev) =>
            {
                if (list.SelectedItem == null) return;

                string selected = list.SelectedItem.ToString();
                string itemName = selected.Split('(')[0].Trim();

                string result = battle.PlayerUseItem(itemName);
                ShowFloatingText(result, FloatingTextType.Heal, CharacterPanelContainer.Children[0]);

                var item = inventory.FirstOrDefault(i => i.Name == itemName);
                if (item != null)
                {
                    item.Quantity--;
                    if (item.Quantity <= 0)
                        inventory.Remove(item);
                }

                battle.NextPlayerTurn();
                CheckNextState();
                UpdateUI();
                window.Close();
            };

            window.ShowDialog();
        }

        private void SkillButton_Click(object sender, RoutedEventArgs e)
        {
            var currentChar = characters[battle.CurrentPlayerIndex];
            if (currentChar.Skills == null || currentChar.Skills.Count == 0)
            {
                MessageBox.Show($"{currentChar.Name} has no skills!");
                return;
            }

            var window = CreateSelectionWindow("Select Skill", currentChar.Skills.Select(s => s));
            var list = (ListBox)window.Content;

            list.SelectionChanged += (s, ev) =>
            {
                if (list.SelectedItem == null) return;

                string selectedSkill = list.SelectedItem.ToString();
                window.Close();
                SelectSkillTarget(selectedSkill);
            };

            window.ShowDialog();
        }

        private void SelectSkillTarget(string skillName)
        {
            bool isHeal = skillName.ToLower().Contains("heal");
            var targets = isHeal
                ? characters.Where(c => c.HP > 0).Select(c => c.Name)
                : battle.GetAliveEnemies().Select(e => e.Name);

            var window = CreateSelectionWindow("Select Target", targets);
            var list = (ListBox)window.Content;

            list.SelectionChanged += (s, ev) =>
            {
                if (list.SelectedItem == null) return;

                string selected = list.SelectedItem.ToString();
                string result = "";

                if (isHeal)
                {
                    var target = characters.First(c => c.Name == selected);
                    result = battle.PlayerUseSkillOnAlly(skillName, target);

                    var index = characters.IndexOf(target);
                    ShowFloatingText(result, FloatingTextType.Heal, CharacterPanelContainer.Children[index]);
                }
                else
                {
                    var target = battle.GetAliveEnemies().First(e => e.Name == selected);
                    int index = battle.GetAliveEnemies().IndexOf(target);
                    result = battle.PlayerUseSkillOnEnemy(skillName, target);

                    ShowFloatingText(result, FloatingTextType.Damage, EnemyPanelContainer.Children[index]);
                }

                battle.NextPlayerTurn();
                CheckNextState();
                UpdateUI();
                window.Close();
            };

            window.ShowDialog();
        }

        private Window CreateSelectionWindow(string title, IEnumerable<string> items)
        {
            var list = new ListBox
            {
                Background = Brushes.Black,
                Foreground = Brushes.White,
                ItemsSource = items.ToList()
            };

            return new Window
            {
                Title = title,
                Width = 300,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = Brushes.Black,
                Owner = Application.Current.MainWindow,
                Content = list
            };
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            EndBattle("You ran away!");
        }

        private void CheckNextState()
        {
            if (battle.State == BattleState.EnemyTurn)
                EnemyTurn();
            else if (battle.State == BattleState.Won)
                EndBattle("You won!");
        }

        private void EnemyTurn()
        {
            var result = battle.EnemyAttack();

            //TODO :: Need to change but to test
            ShowFloatingText("-10", FloatingTextType.Damage, CharacterPanelContainer.Children[0]);

            UpdateUI();

            if (battle.State == BattleState.Lost)
                EndBattle("You were defeated.");
        }

        private void EndBattle(string message)
        {
            MessageBox.Show(message);

            if (battle.State == BattleState.Won)
            {
                GrantBattleRewards(120, 50); // EXP and Gold reward
            }

            OnBattleEnd?.Invoke();
        }

        private void GrantBattleRewards(int expReward, int goldReward)
        {
            List<string> leveledUpNames = new List<string>();

            foreach (var character in characters)
            {
                int oldLevel = character.Level;
                character.GainEXP(expReward);
                if (character.Level > oldLevel)
                    leveledUpNames.Add(character.Name);
            }

            string message = $"Victory!\nGained {expReward} EXP and {goldReward} Gold.";

            if (leveledUpNames.Count > 0)
            {
                message += "\n\nLevel Up!\n";
                foreach (var name in leveledUpNames)
                    message += $"- {name}\n";
            }

            MessageBox.Show(message);
        }

        private void UpdateCharacterPanel()
        {
            CharacterPanelContainer.Children.Clear();

            for (int i = 0; i < characters.Count; i++)
            {
                var character = characters[i];

                var panel = new CharacterPanel
                {
                    Portrait = character.PortraitFile,
                    CharacterName = character.Name,
                    Level = character.Level,
                    CurrentHP = character.HP,
                    MaxHP = character.MaxHP,
                    CurrentMP = character.MP,
                    MaxMP = character.MaxMP,
                    IsActive = (i == battle.CurrentPlayerIndex),
                };

                CharacterPanelContainer.Children.Add(panel);
            }
        }

        private void UpdateEnemyPanel()
        {
            EnemyPanelContainer.Children.Clear();

            var aliveEnemies = battle.GetAliveEnemies();

            for (int i = 0; i < aliveEnemies.Count; i++)
            {
                var enemy = aliveEnemies[i];

                var panel = new EnemyPanel();
                panel.SetEnemy(enemy, i == selectedEnemyIndex);

                int capturedIndex = i;
                panel.MouseLeftButtonDown += (s, e) =>
                {
                    selectedEnemyIndex = capturedIndex;
                    UpdateEnemyPanel(); // Refresh
                };

                EnemyPanelContainer.Children.Add(panel);
            }
        }

        private List<FloatingText> floatingTexts = new List<FloatingText>();

        private void ShowFloatingText(string text, FloatingTextType type, UIElement anchor)
        {
            var floating = new FloatingText(text, type);

            Point position = anchor.TranslatePoint(new Point(0, 0), EffectCanvas);

            Canvas.SetLeft(floating, position.X + (anchor.RenderSize.Width / 2) - 20);
            Canvas.SetTop(floating, position.Y - 10);

            EffectCanvas.Children.Add(floating);
        }  
    }
}
