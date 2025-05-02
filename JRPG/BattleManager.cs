using JRPG.Enums;
using System.Collections.Generic;
using System;
using System.Linq;

namespace JRPG
{
    public class BattleManager
    {
        private List<Character> players;
        public int CurrentPlayerIndex { get; set; } = 0;
        private List<Enemy> enemies = new List<Enemy>();
        public BattleState State { get; private set; } = BattleState.PlayerTurn;

        public BattleManager(List<Character> players, List<Enemy> enemies)
        {
            this.players = players;
            this.enemies = enemies;
        }

        public int PlayerAttack(Enemy target)
        {
            var attacker = players[CurrentPlayerIndex];

            int damage = attacker.GetTotalAttack(new Dictionary<string, int>()) + 5;

            target.CurrentHP -= damage;
            if (target.CurrentHP <= 0)
            {
                target.CurrentHP = 0;
            }

            if (enemies.All(e => e.CurrentHP <= 0))
            {
                State = BattleState.Won;
            }

            return damage;
        }

        public string PlayerDefend()
        {
            var defender = players[CurrentPlayerIndex];
            // You can implement real defense mechanics later
            return $"{defender.Name} is defending.";
        }

        public string PlayerUseItem(string itemName)
        {
            var user = players[CurrentPlayerIndex];

            if (itemName == "Potion")
            {
                user.HP += 50;
                return $"{user.Name} used a Potion and restored 50 HP!";
            }
            else if (itemName == "Mega Potion")
            {
                user.HP += 100;
                return $"{user.Name} used a Mega Potion and restored 100 HP!";
            }
            else if (itemName == "Elixir")
            {
                user.HP = 999; // Full HP
                return $"{user.Name} used an Elixir and fully healed!";
            }

            return $"{user.Name} used {itemName}, but nothing happened.";
        }

        public string PlayerUseSkill(string skillName, Enemy target)
        {
            var user = players[CurrentPlayerIndex];

            // Define skill MP costs
            var skillCosts = new Dictionary<string, int>
            {
                { "Fireball", 10 },
                { "Power Slash", 5 },
                { "Heal", 8 }
            };

            if (skillCosts.ContainsKey(skillName))
            {
                int cost = skillCosts[skillName];
                if (user.MP < cost)
                    return $"{user.Name} does not have enough MP to cast {skillName}!";

                user.MP -= cost; // 🔥 Consume MP
            }

            switch (skillName)
            {
                case "Fireball":
                    int fireballDamage = user.GetTotalAttack(new Dictionary<string, int>()) + 15;
                    target.CurrentHP -= fireballDamage;
                    if (target.CurrentHP <= 0)
                    {
                        target.CurrentHP = 0;

                        if (enemies.All(e => e.CurrentHP <= 0))
                        {
                            State = BattleState.Won;
                            return $"defeated all enemies!";
                        }
                        
                        return $"{user.Name} casts Fireball and defeats {target.Name}!";
                    }
                    return $"{user.Name} casts Fireball, dealing {fireballDamage} damage!";

                case "Power Slash":
                    int slashDamage = user.GetTotalAttack(new Dictionary<string, int>()) + 10;
                    target.CurrentHP -= slashDamage;
                    if (target.CurrentHP <= 0)
                    {
                        target.CurrentHP = 0;

                        if (enemies.All(e => e.CurrentHP <= 0))
                        {
                            State = BattleState.Won;
                            return $"defeated all enemies!";
                        }

                        return $"{user.Name} uses Power Slash and defeats {target.Name}!";
                    }
                    return $"{user.Name} uses Power Slash, dealing {slashDamage} damage!";

                case "Heal":
                    user.HP += 50;
                    if (user.HP > user.MaxHP)
                        user.HP = user.MaxHP;
                    return $"{user.Name} casts Heal and restores 50 HP!";

                default:
                    return $"{user.Name} tried to use {skillName}, but nothing happened.";
            }
        }

        public string PlayerUseSkillOnEnemy(string skillName, Enemy target)
        {
            var user = players[CurrentPlayerIndex];

            var skillCosts = new Dictionary<string, int>
            {
                { "Fireball", 10 },
                { "Power Slash", 5 }
            };

            if (skillCosts.ContainsKey(skillName))
            {
                int cost = skillCosts[skillName];
                if (user.MP < cost)
                    return $"{user.Name} does not have enough MP to cast {skillName}!";
                user.MP -= cost;
            }

            switch (skillName)
            {
                case "Fireball":
                    int damage = user.GetTotalAttack(new Dictionary<string, int>()) + 15;
                    target.CurrentHP -= damage;
                    if (target.CurrentHP <= 0)
                        target.CurrentHP = 0;
                    return $"{user.Name} casts Fireball on {target.Name} for {damage} damage!";

                case "Power Slash":
                    int pDamage = user.GetTotalAttack(new Dictionary<string, int>()) + 10;
                    target.CurrentHP -= pDamage;
                    if (target.CurrentHP <= 0)
                        target.CurrentHP = 0;
                    return $"{user.Name} uses Power Slash on {target.Name} for {pDamage} damage!";
            }

            return $"{user.Name} used {skillName}, but nothing happened.";
        }

        public string PlayerUseSkillOnAlly(string skillName, Character ally)
        {
            var user = players[CurrentPlayerIndex];

            var skillCosts = new Dictionary<string, int>
            {
                { "Heal", 8 }
            };

            if (skillCosts.ContainsKey(skillName))
            {
                int cost = skillCosts[skillName];
                if (user.MP < cost)
                    return $"{user.Name} does not have enough MP to cast {skillName}!";
                user.MP -= cost;
            }

            switch (skillName)
            {
                case "Heal":
                    ally.HP += 50;
                    if (ally.HP > ally.MaxHP)
                        ally.HP = ally.MaxHP;
                    return $"{user.Name} casts Heal on {ally.Name}, restoring 50 HP!";
            }

            return $"{user.Name} used {skillName}, but nothing happened.";
        }

        public string EnemyAttack()
        {
            var alivePlayers = players.Where(p => p.HP > 0).ToList();
            if (alivePlayers.Count == 0)
            {
                State = BattleState.Lost;
                return $"All players defeated!";
            }

            var randomEnemy = enemies.Where(e => e.CurrentHP > 0).OrderBy(e => Guid.NewGuid()).FirstOrDefault();
            if (randomEnemy == null)
            {
                State = BattleState.Won;
                return $"All enemies defeated!";
            }

            var target = alivePlayers[new Random().Next(alivePlayers.Count)];
            int damage = randomEnemy.Attack;

            target.HP -= damage;
            if (target.HP < 0)
                target.HP = 0;

            if (players.All(p => p.HP <= 0))
            {
                State = BattleState.Lost;
                return $"{randomEnemy.Name} defeated the party!";
            }

            State = BattleState.PlayerTurn;
            CurrentPlayerIndex = 0;
            return $"{randomEnemy.Name} attacks {target.Name} for {damage} damage!";
        }

        public void NextPlayerTurn()
        {
            CurrentPlayerIndex++;
            if (CurrentPlayerIndex >= players.Count && (State != BattleState.Lost || State != BattleState.Won))
            {
                // All players acted, now enemy acts
                State = BattleState.EnemyTurn;
            }
        }

        public List<Enemy> GetAliveEnemies()
        {
            return enemies.Where(e => e.CurrentHP > 0).ToList();
        }
    }
}
