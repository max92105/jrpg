using JRPG.Enums;
using JRPG.Core.Battles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using JRPG.UI.Battle;

namespace JRPG
{
    public class BattleManager
    {
        private readonly List<Character> players;
        private readonly List<Enemy> enemies;
        private Queue<TurnEntry> turnQueue = new Queue<TurnEntry>();

        public BattleState State { get; private set; } = BattleState.PlayerTurn;

        public TurnEntry CurrentTurn => turnQueue.Peek();
        public Queue<TurnEntry> TurnQueue => new Queue<TurnEntry>(turnQueue);

        public BattleManager(List<Character> players, List<Enemy> enemies)
        {
            this.players = players;
            this.enemies = enemies;
            InitializeTurnOrder();
        }

        public void InitializeTurnOrder()
        {
            turnQueue.Clear();
            var allUnits = new List<TurnEntry>();

            allUnits.AddRange(players.Select(p => new TurnEntry { Character = p }));
            allUnits.AddRange(enemies.Select(e => new TurnEntry { Enemy = e }));

            // Later: Sort by initiative or speed
            turnQueue = new Queue<TurnEntry>(allUnits.OrderBy(_ => Guid.NewGuid())); // shuffle for now

            State = CurrentTurn.IsPlayer ? BattleState.PlayerTurn : BattleState.EnemyTurn;
        }

        public void AdvanceTurn()
        {
            do
            {
                var current = turnQueue.Dequeue();
                turnQueue.Enqueue(current);
            }
            while ((CurrentTurn.IsPlayer && CurrentTurn.Character.HP <= 0)
                    || (!CurrentTurn.IsPlayer && CurrentTurn.Enemy.CurrentHP <= 0));

            State = CurrentTurn.IsPlayer ? BattleState.PlayerTurn : BattleState.EnemyTurn;
        }

        public bool IsBattleOver()
        {
            if (players.All(p => p.HP <= 0))
            {
                State = BattleState.Lost;
                return true;
            }
            if (enemies.All(e => e.CurrentHP <= 0))
            {
                State = BattleState.Won;
                return true;
            }
            return false;
        }

        public string ExecuteCurrentTurnAction(string action, object target = null)
        {
            if (State == BattleState.Won || State == BattleState.Lost)
                return "Battle is already over.";

            var current = CurrentTurn;

            if (current.IsPlayer)
            {
                return ExecutePlayerAction(current.Character, action, target);
            }
            else
            {
                return ExecuteEnemyAction(current.Enemy);
            }
        }

        private string ExecutePlayerAction(Character player, string action, object target)
        {
            switch (action)
            {
                case "Attack":
                    if (target is Enemy enemyTarget)
                        return PlayerAttack(player, enemyTarget);
                    return "No valid enemy target.";

                case "Defend":
                    return $"{player.Name} is defending."; // TODO: Add logic

                case "UseItem":
                    // Simplify later for UI call
                    return PlayerUseItem(player, target as string);

                case "SkillEnemy":
                    if (target is Tuple<string, Enemy> skillInfo)
                        return PlayerUseSkillOnEnemy(player, skillInfo.Item1, skillInfo.Item2);
                    return "Invalid skill or target.";

                case "SkillAlly":
                    if (target is Tuple<string, Character> healInfo)
                        return PlayerUseSkillOnAlly(player, healInfo.Item1, healInfo.Item2);
                    return "Invalid skill or ally.";

                default:
                    return "Unknown action.";
            }
        }

        private string ExecuteEnemyAction(Enemy enemy)
        {
            var alivePlayers = players.Where(p => p.HP > 0).ToList();
            if (alivePlayers.Count == 0)
            {
                State = BattleState.Lost;
                return $"All players defeated!";
            }

            var target = alivePlayers[new Random().Next(alivePlayers.Count)];
            int damage = enemy.Attack;
            target.HP -= damage;
            if (target.HP < 0) target.HP = 0;

            if (players.All(p => p.HP <= 0))
            {
                State = BattleState.Lost;
                return $"{enemy.Name} defeated the party!";
            }

            return $"{enemy.Name} attacks {target.Name} for {damage} damage!";
        }

        private string PlayerAttack(Character attacker, Enemy target)
        {
            int damage = attacker.GetTotalAttack(new Dictionary<string, int>()) + 5;
            target.CurrentHP -= damage;
            if (target.CurrentHP < 0) target.CurrentHP = 0;

            if (IsBattleOver())
                return $"{attacker.Name} attacked and ended the battle!";
            return $"{attacker.Name} attacks {target.Name} for {damage} damage!";
        }

        private string PlayerUseItem(Character user, string itemName)
        {
            switch (itemName)
            {
                case "Potion":
                    user.HP += 50;
                    return $"{user.Name} used a Potion and restored 50 HP!";
                case "Mega Potion":
                    user.HP += 100;
                    return $"{user.Name} used a Mega Potion and restored 100 HP!";
                case "Elixir":
                    user.HP = user.MaxHP;
                    return $"{user.Name} used an Elixir and fully healed!";
                default:
                    return $"{user.Name} used {itemName}, but nothing happened.";
            }
        }

        private string PlayerUseSkillOnEnemy(Character user, string skillName, Enemy target)
        {
            var skillCosts = new Dictionary<string, int>
            {
                { "Fireball", 10 },
                { "Power Slash", 5 }
            };

            if (skillCosts.TryGetValue(skillName, out int cost))
            {
                if (user.MP < cost)
                    return $"{user.Name} does not have enough MP for {skillName}!";
                user.MP -= cost;
            }

            int baseDamage = user.GetTotalAttack(new Dictionary<string, int>());
            int bonus = skillName == "Fireball" ? 15 : 10;
            int damage = baseDamage + bonus;

            target.CurrentHP -= damage;
            if (target.CurrentHP < 0) target.CurrentHP = 0;

            if (IsBattleOver())
                return $"{user.Name} used {skillName} and won the battle!";
            return $"{user.Name} used {skillName} on {target.Name} for {damage} damage!";
        }

        private string PlayerUseSkillOnAlly(Character user, string skillName, Character ally)
        {
            var skillCosts = new Dictionary<string, int> { { "Heal", 8 } };

            if (skillCosts.TryGetValue(skillName, out int cost))
            {
                if (user.MP < cost)
                    return $"{user.Name} does not have enough MP for {skillName}!";
                user.MP -= cost;
            }

            if (skillName == "Heal")
            {
                ally.HP = Math.Min(ally.HP + 50, ally.MaxHP);
                return $"{user.Name} heals {ally.Name} for 50 HP!";
            }

            return $"{user.Name} tried to use {skillName}, but nothing happened.";
        }

        public List<Enemy> GetAliveEnemies() => enemies.Where(e => e.CurrentHP > 0).ToList();
        public List<Character> GetAlivePlayers() => players.Where(p => p.HP > 0).ToList();
    }
}