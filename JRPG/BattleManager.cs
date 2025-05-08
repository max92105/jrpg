using JRPG.Enums;
using JRPG.Core.Battles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using JRPG.Core.Battles.Enum;

namespace JRPG
{
    public class BattleManager
    {
        private readonly List<BattleActor> battleActors;

        public List<BattleActor> GetAliveEnemies() => battleActors.Where(battleActors => battleActors is Enemy && battleActors.HP > 0).ToList();
        public List<BattleActor> GetAlivePlayers() => battleActors.Where(battleActors => battleActors is Character && battleActors.HP > 0).ToList();

        private Queue<TurnEntry> turnQueue = new Queue<TurnEntry>();

        public BattleState State { get; private set; } = BattleState.PlayerTurn;

        public TurnEntry CurrentTurn => turnQueue.Peek();

        public Queue<TurnEntry> TurnQueue => turnQueue;

        public BattleManager(List<BattleActor> battleActors)
        {
            this.battleActors = battleActors;
            InitializeTurnOrder();
        }

        public void InitializeTurnOrder()
        {
            turnQueue.Clear();
            var allUnits = new List<TurnEntry>();

            allUnits.AddRange(battleActors.Select(p => new TurnEntry { BattleActor = p }));

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
            while (CurrentTurn.BattleActor.HP <= 0);

            State = CurrentTurn.IsPlayer ? BattleState.PlayerTurn : BattleState.EnemyTurn;
        }

        public bool IsBattleOver()
        {
            if (!battleActors.Where(actor => actor is Character && actor.HP > 0).Any())
            {
                State = BattleState.Lost;
                return true;
            }

            if (!battleActors.Where(actor => actor is Enemy && actor.HP > 0).Any())
            {
                State = BattleState.Won;
                return true;
            }

            return false;
        }

        public string ExecuteCurrentTurnAction(CombatAction combatAction, List<BattleActor> targets, object param = null)
        {          
            var current = CurrentTurn;

            if (CurrentTurn.IsPlayer)
            {
                return ExecutePlayerAction((Character)current.BattleActor, combatAction, targets, param);
            }
            else
            {
                return ExecuteEnemyAction(targets);
            }
        }

        private string ExecutePlayerAction(Character character, CombatAction action, List<BattleActor> targets, object param = null)
        {
            switch (action)
            {
                case CombatAction.Attack:
                    if (targets.Count > 0)
                        return Attack(character, targets);
                    return "No valid enemy target.";

                case CombatAction.Defend:
                    return $"{character.Name} is defending."; // TODO: Add logic

                case CombatAction.Item:
                    if (param is InventoryItem item)
                        return PlayerUseItem(character, item, targets);
                    return "Invalid item.";

                case CombatAction.Skill:
                    if (param is Skill skill)
                        return PlayerUseSkill(character, skill, targets);
                    return "Invalid skill.";

                default:
                    return "Unknown action.";
            }
        }

        private string Attack(BattleActor attacker, List<BattleActor> targets)
        {
            foreach (var target in targets)
            {
                var damage = target.TakeDamage(attacker.GetTotalAttack());
            }
          
            return $"{attacker.Name} attacks";
        }

        private string PlayerUseItem(Character user, InventoryItem item, List<BattleActor> targets)
        {
            if (item.TargetType == TargetType.Enemy || item.TargetType == TargetType.AllEnemies)
            {
                foreach (var target in targets)
                {
                    target.TakeDamage(item.EffectValue);
                }

                item.Quantity--;

                return $"{user.Name} used {item.Name} for {item.EffectValue} damage!";
            }
            else
            {
                foreach (var target in targets)
                {
                    target.Heal(item.EffectValue);
                }

                item.Quantity--;

                return $"{user.Name} used {item.Name} heals for {item.EffectValue} HP!";
            }
        }

        private string PlayerUseSkill(Character user, Skill skill, List<BattleActor> targets)
        {
            if (user.MP < skill.MPCost)
            {
                return $"{user.Name} does not have enough MP for {skill.Name}!";
            }

            user.UseMP(skill.MPCost);

            if (skill.TargetType == Core.Battles.Enum.TargetType.Enemy || skill.TargetType == Core.Battles.Enum.TargetType.AllEnemies)
            {
                foreach (var target in targets)
                {
                    target.TakeDamage(skill.EffectValue);
                }

                return $"{user.Name} used {skill.Name} for {skill.EffectValue} damage!";
            }
            else
            {
                foreach (var target in targets)
                {
                    target.Heal(skill.EffectValue);
                }

                return $"{user.Name} heals for {skill.EffectValue} HP!";
            }
        }

        private string ExecuteEnemyAction(List<BattleActor> targets)
        {
            return Attack(CurrentTurn.BattleActor, targets);
        }
    }
}