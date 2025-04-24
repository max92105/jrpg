using JRPG.Enums;
using System.Windows;
using System.Windows.Controls;

namespace JRPG
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class BattleUI : UserControl
    {
        private BattleManager battle;
        private Fighter player;
        private Enemy enemy;

        public event System.Action OnBattleEnd;

        public BattleUI(Fighter player, Enemy enemy)
        {
            InitializeComponent();
            this.player = player;
            this.enemy = enemy;
            battle = new BattleManager(player, enemy);

            PlayerName.Text = player.Name;
            EnemyName.Text = enemy.Name;

            UpdateUI();
        }

        private void UpdateUI()
        {
            PlayerHP.Text = $"HP: {player.CurrentHP}/{player.MaxHP}";
            EnemyHP.Text = $"HP: {enemy.CurrentHP}/{enemy.MaxHP}";

            AttackButton.IsEnabled = (battle.State == BattleState.PlayerTurn);
            DefendButton.IsEnabled = (battle.State == BattleState.PlayerTurn);
            RunButton.IsEnabled = (battle.State == BattleState.PlayerTurn);
        }

        private void AttackButton_Click(object sender, RoutedEventArgs e)
        {
            var result = battle.PlayerAttack();
            MessageBox.Show(result);
            UpdateUI();
            if (battle.State == BattleState.EnemyTurn)
                EnemyTurn();
            else if (battle.State == BattleState.Won)
                EndBattle("You won!");
        }

        private void DefendButton_Click(object sender, RoutedEventArgs e)
        {
            var result = battle.PlayerDefend();
            MessageBox.Show(result);
            EnemyTurn();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            EndBattle("You ran away!");
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
            OnBattleEnd?.Invoke();
        }
    }
}
