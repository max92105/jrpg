using JRPG.Core.Battles.Enums;
using JRPG.Core.Battles.Models;
using System.Collections.Generic;
using System.Linq;

namespace JRPG.Core.Systems.Parties
{
    public class PartyManager
    {
        public List<Character> AllCharacters { get; private set; } = new List<Character>();
        public List<Character> ActivePartyOrder { get; private set; } = new List<Character>();

        public int MaxPartySize { get; set; } = 4;

        public PartyManager()
        {
            InitializeCharacters();
            RefreshActivePartyOrder();
        }

        public void LoadFromSaveData(List<Character> characters, List<string> activeNames)
        {
            AllCharacters = characters;
            RefreshActivePartyOrder();

            // Force the saved active order (preserving current character instances)
            ActivePartyOrder = activeNames
                .Select(name => AllCharacters.FirstOrDefault(c => c.Name == name))
                .Where(c => c != null)
                .ToList();

            // Also re-flag IsActive based on that order
            foreach (var c in AllCharacters)
                c.IsActive = ActivePartyOrder.Contains(c);
        }

        private void InitializeCharacters()
        {
            AllCharacters.Add(new Character
            {
                Id = "0",
                Name = "Sarxian",
                PortraitFile = "Sarxian.png",
                BattleSpriteFile = "Sarxian.png",
                AttackVFXFile = "AttackPlayer.gif",
                Class = "Warrior",
                Level = 10,
                HP = 200,
                MaxHP = 300,
                MP = 200,
                MaxMP = 300,
                BaseAttack = 15,
                BaseDefense = 8,
                EXP = 250,
                EXPToNextLevel = 500,
                IsActive = true,
                IsMainCharacter = true,
                Skills = new List<Skill>
                {
                    new Skill { Name = "Fire Blast", IconFile = "Attack.png", MPCost = 10, EffectValue = 15, EffectType = EffectType.Damage, TargetType = TargetType.Enemy },
                    new Skill { Name = "Fireball", IconFile = "Attack.png", MPCost = 10, EffectValue = 20, EffectType = EffectType.Damage, TargetType = TargetType.AllEnemies },
                    new Skill { Name = "Heal", MPCost = 8, EffectValue = 15, EffectType = EffectType.Heal, TargetType = TargetType.Ally },
                    new Skill { Name = "Recovery", MPCost = 8, EffectValue = 30, EffectType = EffectType.Heal, TargetType = TargetType.Self },
                    new Skill { Name = "Heal All", MPCost = 8, EffectValue = 50, EffectType = EffectType.Heal, TargetType = TargetType.AllAllies },
                }
            });

            AllCharacters.Add(new Character
            {
                Id = "1",
                Name = "Jazog",
                PortraitFile = "Jazog.png",
                BattleSpriteFile = "Jazog.png",
                AttackVFXFile = "AttackPlayer.gif",
                Class = "Mage",
                Level = 9,
                HP = 120,
                MaxHP = 120,
                MP = 30,
                MaxMP = 30,
                BaseAttack = 5,
                BaseDefense = 3,
                EXP = 399,
                EXPToNextLevel = 400,
            });

            AllCharacters.Add(new Character
            {
                Id = "2",
                Name = "Teelle",
                PortraitFile = "Teelle.png",
                BattleSpriteFile = "Teelle.png",
                AttackVFXFile = "AttackPlayer.gif",
                Class = "Thief",
                Level = 11,
                HP = 190,
                MaxHP = 190,
                MP = 300,
                MaxMP = 300,
                BaseAttack = 12,
                BaseDefense = 5,
                EXP = 300,
                EXPToNextLevel = 600,
            });

            AllCharacters.Add(new Character
            {
                Id = "3",
                Name = "Zaruk",
                PortraitFile = "Zaruk.png",
                BattleSpriteFile = "Zaruk.png",
                AttackVFXFile = "AttackPlayer.gif",
                Class = "Healer",
                Level = 8,
                HP = 150,
                MaxHP = 150,
                MP = 30,
                MaxMP = 30,
                BaseAttack = 4,
                BaseDefense = 6,
                EXP = 100,
                EXPToNextLevel = 300,
            });

            AllCharacters.Add(new Character
            {
                Id = "4",
                Name = "Zanyr",
                PortraitFile = "Zanyr.png",
                BattleSpriteFile = "Zanyr.png",
                AttackVFXFile = "AttackPlayer.gif",
                Class = "Knight",
                Level = 12,
                HP = 300,
                MaxHP = 300,
                MP = 30,
                MaxMP = 30,
                BaseAttack = 18,
                BaseDefense = 12,
                EXP = 100,
                EXPToNextLevel = 300,
            });

            AllCharacters.Add(new Character
            {
                Id = "5",
                Name = "Thaali",
                PortraitFile = "Thaali.png",
                BattleSpriteFile = "Thaali.png",
                AttackVFXFile = "AttackPlayer.gif",
                Class = "Ranger",
                Level = 10,
                HP = 170,
                MaxHP = 170,
                MP = 30,
                MaxMP = 30,
                BaseAttack = 13,
                BaseDefense = 5,
                EXP = 100,
                EXPToNextLevel = 300,
            });

            AllCharacters.Add(new Character
            {
                Id = "6",
                Name = "Kamilia",
                PortraitFile = "Kamilia.png",
                BattleSpriteFile = "Kamilia.png",
                AttackVFXFile = "AttackPlayer.gif",
                Class = "Ranger",
                Level = 10,
                HP = 170,
                MaxHP = 170,
                MP = 30,
                MaxMP = 30,
                BaseAttack = 13,
                BaseDefense = 5,
                EXP = 100,
                EXPToNextLevel = 300,
            });
        }

        public void RefreshActivePartyOrder()
        {
            ActivePartyOrder = AllCharacters
                .Where(c => c.IsActive)
                .OrderBy(c => c.IsMainCharacter ? 0 : 1)
                .ToList();
        }

        public void SetPartyOrder(List<Character> newOrder)
        {
            // Ensure only valid active characters are set
            var validOrder = newOrder
                .Where(c => AllCharacters.Contains(c))
                .Distinct()
                .Take(MaxPartySize)
                .ToList();

            // Set all as inactive first
            foreach (var character in AllCharacters)
                character.IsActive = false;

            // Set active only those in the new order
            foreach (var character in validOrder)
                character.IsActive = true;

            ActivePartyOrder = validOrder;
        }

        public int GetActivePartyMemberCount() => AllCharacters.Count(c => c.IsActive);

        public void AddToParty(Character character)
        {
            if (!character.IsActive && GetActivePartyMemberCount() < MaxPartySize)
            {
                character.IsActive = true;
                ActivePartyOrder.Add(character);
            }
        }

        public void RemoveFromParty(Character character)
        {
            if (!character.IsMainCharacter)
            {
                character.IsActive = false;
                ActivePartyOrder.Remove(character);
            }
        }

        public Character GetCharacterById(string id)
        {
            return AllCharacters.FirstOrDefault(c => c.Id == id);
        }
    }
}
