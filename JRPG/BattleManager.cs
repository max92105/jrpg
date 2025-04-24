using JRPG.Enums;

namespace JRPG
{
    public class BattleManager
    {
        public Fighter Player { get; private set; }
        public Enemy Enemy { get; private set; }
        public BattleState State { get; private set; }

        public BattleManager(Fighter player, Enemy enemy)
        {
            Player = player;
            Enemy = enemy;
            State = BattleState.PlayerTurn;
        }

        public string PlayerAttack()
        {
            int dmg = Enemy.TakeDamage(Player.Attack);
            if (!Enemy.IsAlive)
            {
                State = BattleState.Won;
                return $"You dealt {dmg} damage and defeated {Enemy.Name}!";
            }
            else
            {
                State = BattleState.EnemyTurn;
                return $"You dealt {dmg} damage to {Enemy.Name}.";
            }
        }

        public string PlayerDefend()
        {
                State = BattleState.EnemyTurn;
                return $"You brace for impact (no effect).";
        }

        public string EnemyAttack()
        {
            int dmg = Player.TakeDamage(Enemy.Attack);
            if (!Player.IsAlive)
            {
                State = BattleState.Lost;
                return $"{Enemy.Name} attacks for {dmg} damage and defeats you!";
            }
            else
            {
                State = BattleState.PlayerTurn;
                return $"{Enemy.Name} attacks for {dmg} damage.";
            }
        }
    }
}
