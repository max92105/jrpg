using JRPG.Enums;
using JRPG.Core.Battles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using JRPG.Core.Battles.Enum;
using System.Windows;

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
            if (!battleActors.Any(actor => actor is Character && actor.HP > 0))
            {
                State = BattleState.Lost;
                return true;
            }

            if (!battleActors.Any(actor => actor is Enemy && actor.HP > 0))
            {
                State = BattleState.Won;
                return true;
            }

            return false;
        }

        public List<TurnActionResult> ExecuteCurrentTurnAction(CombatAction combatAction, List<BattleActor> targets, object param = null)
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

        private List<TurnActionResult> ExecutePlayerAction(Character character, CombatAction action, List<BattleActor> targets, object param = null)
        {
            switch (action)
            {
                case CombatAction.Attack:
                    {
                        if (targets.Count > 0)
                            return Attack(character, targets);

                        throw new Exception("No valid enemy target.");
                    }

                case CombatAction.Defend:
                    {
                        return new List<TurnActionResult>(); // TODO: Add logic
                    }

                case CombatAction.Item:
                    {
                        if (param is InventoryItem item)
                        {
                            return PlayerUseItem(character, item, targets);
                        }

                        throw new Exception("Invalid item.");
                    }

                case CombatAction.Skill:
                    {
                        if (param is Skill skill)
                        {
                            return PlayerUseSkill(character, skill, targets);
                        }

                        throw new Exception("Invalid skill.");
                    }

                default:
                    throw new Exception("Unknown action.");
            }
        }

        private List<TurnActionResult> Attack(BattleActor attacker, List<BattleActor> targets)
        {
            List<TurnActionResult> result = new List<TurnActionResult>();

            foreach (var target in targets)
            {
                var damage = target.TakeDamage(attacker.GetTotalAttack());

                result.Add(new TurnActionResult(target, damage, EffectType.Damage));
            }

            return result;
        }

        private List<TurnActionResult> PlayerUseItem(Character user, InventoryItem item, List<BattleActor> targets)
        {
            List<TurnActionResult> result = new List<TurnActionResult>();

            if (item.TargetType == TargetType.Enemy || item.TargetType == TargetType.AllEnemies)
            {
                foreach (var target in targets)
                {
                    target.TakeDamage(item.EffectValue);

                    result.Add(new TurnActionResult(target, item.EffectValue, item.EffectType));
                }

                item.Quantity--;

                return result;
            }
            else
            {
                foreach (var target in targets)
                {
                    target.Heal(item.EffectValue);

                    result.Add(new TurnActionResult(target, item.EffectValue, item.EffectType));
                }

                item.Quantity--;

                return result;
            }
        }

        private List<TurnActionResult> PlayerUseSkill(Character user, Skill skill, List<BattleActor> targets)
        {
            List<TurnActionResult> result = new List<TurnActionResult>();

            if (user.MP < skill.MPCost)
            {
                return result;
            }

            user.UseMP(skill.MPCost);

            if (skill.TargetType == Core.Battles.Enum.TargetType.Enemy || skill.TargetType == Core.Battles.Enum.TargetType.AllEnemies)
            {
                foreach (var target in targets)
                {
                    target.TakeDamage(skill.EffectValue);

                    result.Add(new TurnActionResult(target, skill.EffectValue, skill.EffectType));
                }

                return result;
            }
            else
            {
                foreach (var target in targets)
                {
                    target.Heal(skill.EffectValue);

                    result.Add(new TurnActionResult(target, skill.EffectValue, skill.EffectType));
                }

                return result;
            }
        }

        private List<TurnActionResult> ExecuteEnemyAction(List<BattleActor> targets)
        {
            return Attack(CurrentTurn.BattleActor, targets);
        }
    }
}