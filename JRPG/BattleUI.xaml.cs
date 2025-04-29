using JRPG.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            EnemyList.Items.Clear();

            foreach (var enemy in battle.GetAliveEnemies())
            {
                EnemyList.Items.Add($"{enemy.Name} - HP: {enemy.CurrentHP}/{enemy.MaxHP}");
            }

            PlayerList.Items.Clear();
            for (int i = 0; i < characters.Count; i++)
            {
                var player = characters[i];
                string displayName = $"{player.Name} - HP: {player.HP} / MP: {player.MP}/{player.MaxMP}";

                if (player.HP <= 0)
                    displayName = $"{player.Name} - (KO)";

                if (i == battle.CurrentPlayerIndex)
                    displayName = "➔ " + displayName; // 🔥 Mark active player

                PlayerList.Items.Add(displayName);
            }

            AttackButton.IsEnabled = (battle.State == BattleState.PlayerTurn);
            DefendButton.IsEnabled = (battle.State == BattleState.PlayerTurn);
            RunButton.IsEnabled = (battle.State == BattleState.PlayerTurn);
            UseItemButton.IsEnabled = (battle.State == BattleState.PlayerTurn);
            SkillButton.IsEnabled = (battle.State == BattleState.PlayerTurn);
        }

        private void AttackButton_Click(object sender, RoutedEventArgs e)
        {
            if (EnemyList.SelectedIndex == -1)
            {
                MessageBox.Show("Select an enemy to attack!");
                return;
            }

            var aliveEnemies = battle.GetAliveEnemies();
            var target = aliveEnemies[EnemyList.SelectedIndex];

            var result = battle.PlayerAttack(target);
            MessageBox.Show(result);

            if (battle.State != BattleState.Won)
                battle.NextPlayerTurn();

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
            if (inventory == null || !inventory.Any(x => x.Quantity != 0))
            {
                MessageBox.Show("No items available!");
                return;
            }

            var itemSelectionWindow = new Window
            {
                Title = "Select Item",
                Width = 300,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = Brushes.Black,
                Owner = Application.Current.MainWindow // 🔥 Make MainWindow the owner!
            };

            var itemList = new ListBox { Background = Brushes.Black, Foreground = Brushes.White };

            foreach (var item in inventory)
            {
                itemList.Items.Add($"{item.Name} (x{item.Quantity})");
            }

            itemList.SelectionChanged += (s, ev) =>
            {
                if (itemList.SelectedItem != null)
                {
                    string selected = itemList.SelectedItem.ToString();
                    string itemName = selected.Split('(')[0].Trim();

                    var result = battle.PlayerUseItem(itemName);
                    MessageBox.Show(result);

                    if (inventory.Any(x => x.Name == itemName))
                    {
                        var item = inventory.Find(x => x.Name == itemName);
                        item.Quantity--;

                        if (item.Quantity <= 0)
                            inventory.RemoveAll(x => x.Name == itemName); // Remove from inventory if none left
                    }

                    if (battle.State != BattleState.Won)
                        battle.NextPlayerTurn();

                    CheckNextState();
                    UpdateUI();

                    itemSelectionWindow.Close();
                }
            };

            itemSelectionWindow.Content = itemList;
            itemSelectionWindow.ShowDialog();
        }

        private void SkillButton_Click(object sender, RoutedEventArgs e)
        {
            var currentChar = characters[battle.CurrentPlayerIndex];

            if (currentChar.Skills == null || currentChar.Skills.Count == 0)
            {
                MessageBox.Show($"{currentChar.Name} has no skills!");
                return;
            }

            var skillSelectionWindow = new Window
            {
                Title = "Select Skill",
                Width = 300,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = Brushes.Black,
                Owner = Application.Current.MainWindow
            };

            var skillList = new ListBox { Background = Brushes.Black, Foreground = Brushes.White };

            foreach (var skill in currentChar.Skills)
            {
                skillList.Items.Add(skill);
            }

            skillList.SelectionChanged += (s, ev) =>
            {
                if (skillList.SelectedItem != null)
                {
                    string selectedSkill = skillList.SelectedItem.ToString();
                    skillSelectionWindow.Close();

                    // 🔥 After selecting skill, pick target
                    SelectSkillTarget(selectedSkill);
                }
            };

            skillSelectionWindow.Content = skillList;
            skillSelectionWindow.ShowDialog();
        }

        private void SelectSkillTarget(string skillName)
        {
            bool isHealSkill = (skillName == "Heal"); // 🔥 You can expand this later

            var targetWindow = new Window
            {
                Title = "Select Target",
                Width = 300,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = Brushes.Black,
                Owner = Application.Current.MainWindow
            };

            var targetList = new ListBox { Background = Brushes.Black, Foreground = Brushes.White };

            if (isHealSkill)
            {
                foreach (var player in characters.Where(c => c.HP > 0))
                {
                    targetList.Items.Add(player.Name);
                }
            }
            else
            {
                foreach (var enemy in battle.GetAliveEnemies())
                {
                    targetList.Items.Add(enemy.Name);
                }
            }

            targetList.SelectionChanged += (s, ev) =>
            {
                if (targetList.SelectedItem != null)
                {
                    string selectedTarget = targetList.SelectedItem.ToString();

                    string result = "";

                    if (isHealSkill)
                    {
                        var targetPlayer = characters.First(p => p.Name == selectedTarget);
                        result = battle.PlayerUseSkillOnAlly(skillName, targetPlayer);
                    }
                    else
                    {
                        var aliveEnemies = battle.GetAliveEnemies();
                        var targetEnemy = aliveEnemies.First(e => e.Name == selectedTarget);
                        result = battle.PlayerUseSkillOnEnemy(skillName, targetEnemy);
                    }

                    MessageBox.Show(result);

                    battle.NextPlayerTurn();
                    CheckNextState();
                    UpdateUI();

                    targetWindow.Close();
                }
            };

            targetWindow.Content = targetList;
            targetWindow.ShowDialog();
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
            MessageBox.Show(result);
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


    }
}
