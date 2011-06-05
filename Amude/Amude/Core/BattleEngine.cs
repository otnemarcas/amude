using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Domain;
using Amude.Global;
using Amude.Domain.Attribute;
using Microsoft.Xna.Framework;

namespace Amude.Core
{
    internal enum LastAction
    {
        None,
        Walked,
        Attacked,
        Passed
    }

    internal class AttackEvent
    {
        public float Damage { get; set; }
        public bool CounterAttack { get; set; }
        public float CounterDamage { get; set; }
    }

    [Serializable]
    internal sealed class CharacterKey
    {
        private String owner;
        private String character;

        public CharacterKey(String owner, String character)
        {
            this.owner = owner;
            this.character = character;
        }

        public String Owner
        {
            get
            {
                return owner;
            }
        }

        public String Character
        {
            get
            {
                return character;
            }
        }
    }

    internal class BattleEngine
    {
        private Dictionary<CharacterKey, Character> characters;
        private Dictionary<CharacterKey, Dictionary<String, Affect>> affects;
        private CharacterKey actualCharacter;

        #region Singleton

        private static BattleEngine instance;

        private BattleEngine(Player localPlayer, Player remotePlayer)
        {
            characters = new Dictionary<CharacterKey, Character>();
            affects = new Dictionary<CharacterKey, Dictionary<String, Affect>>();

            foreach (Character character in localPlayer.GetCharacters())
            {
                CharacterKey key = new CharacterKey(localPlayer.Name, character.RootName);
                characters.Add(key, character);
                affects.Add(key, new Dictionary<String, Affect>());
            }

            foreach (Character character in remotePlayer.GetCharacters())
            {
                CharacterKey key = new CharacterKey(remotePlayer.Name, character.RootName);
                characters.Add(key, character);
                affects.Add(key, new Dictionary<String, Affect>());
            }

            // Inicie a ActionGauge com valores de 0 - 0.25
            foreach (Character character in characters.Values)
            {
                character.ActionGauge = RandomMath.RandomBetween(0.0f, 0.25f);
            }
        }

        public static BattleEngine GetInstance()
        {
            if (instance == null)
            {
                throw new Exception("BattleEngine não iniciada.");
            }
            return instance;
        }

        public static void Initialize(Player localPlayer, Player remotePlayer)
        {
            instance = new BattleEngine(localPlayer, remotePlayer);
        }

        #endregion

        public Character ActualCharacter
        {
            get
            {
                return characters[actualCharacter];
            }
        }

        public String ActualPlayer
        {
            get
            {
                return actualCharacter.Owner;
            }
        }

        public bool ActualCharacterHasWalked { get; set; }

        public bool ActualCharacterHasAttacked { get; set; }

        public LastAction LastAction { get; set; }

        public void NextCharacterTurn()
        {
            LastAction = LastAction.None;
            var selectedCharacters = from key in characters.Keys
                                     where characters[key].ActionGauge > 1
                                     select key;
            int count = selectedCharacters.Count();

            while (count == 0)
            {
                IncreaseActionGauges();
                selectedCharacters = from key in characters.Keys
                                     where characters[key].ActionGauge > 1
                                     select key;
                count = selectedCharacters.Count();
            }

            // Dos personagens prontos para jogar, selecione aleatóriamente
            float index = RandomMath.RandomBetween(0, count);
            if (index == count)
            {
                index--;
            }

            actualCharacter = selectedCharacters.ElementAt((int)Math.Floor(index));
            if (ActualCharacter.Health.IsDead)
            {
                ActualCharacterHasAttacked = true;
                ActualCharacterHasWalked = true;
                FinalizeCharacterTurn();
                NextCharacterTurn();
            }

            ActualCharacterHasAttacked = false;
            ActualCharacterHasWalked = false;
        }

        public void FinalizeCharacterTurn()
        {
            if (ActualCharacterHasWalked != ActualCharacterHasAttacked)
            {
                characters[actualCharacter].ActionGauge -= 0.8f;
            }
            else if (ActualCharacterHasWalked && ActualCharacterHasAttacked)
            {
                characters[actualCharacter].ActionGauge -= 1f;
            }
            else
            {
                characters[actualCharacter].ActionGauge -= 0.5f;
            }

            ActualCharacter.CounterAttack = true;
            actualCharacter = null;
        }

        public AttackEvent ProcessAttack(String targetPlayer, String target)
        {
            AttackEvent attackEvent = new AttackEvent();
            Player player = Controller.GetInstance().GetPlayer(targetPlayer);
            Character character = player.GetCharacter(target);
            Point actorPosition = ActualCharacter.MapLocation;
            Point targetPosition = character.MapLocation;
            int displacement = 0;

            ActualCharacterHasAttacked = true;
            ProcessSpecialAbilityAttack(ActualCharacter, character);
            attackEvent.Damage = ActualCharacter.Attack.Value * (1 - character.Defense.Value / 10);

            displacement += Math.Abs(actorPosition.X - targetPosition.X);
            displacement += Math.Abs(actorPosition.Y - targetPosition.Y);

            if (displacement <= 1 && character.CounterAttack && character.Health.Value > attackEvent.Damage) 
            {
                character.CounterAttack = false;
                attackEvent.CounterAttack = true;
                ProcessSpecialAbilityAttack(character, ActualCharacter);
                attackEvent.CounterDamage = character.Attack.Value * (1 - ActualCharacter.Defense.Value / 10);
            }

            return attackEvent;
        }

        public Dictionary<CharacterKey, Dictionary<String, Affect>> ProcessSpecialAbilities()
        {
            foreach (Character actor in characters.Values)
            {
                if (actor.SpecialAbility != null &&
                    actor.SpecialAbility.Type == SpecialAbilityType.Passive)
                {
                    bool friendly = actor.SpecialAbility.Friendly;

                    foreach (KeyValuePair<CharacterKey, Character> kv in characters)
                    {
                        if ((actor.Owner.Name != kv.Key.Owner || actor.RootName != kv.Value.RootName) && 
                            ((friendly && actor.Owner.Name == kv.Key.Owner) ||
                            (!friendly && actor.Owner.Name != kv.Key.Owner)) && 
                            !actor.Health.IsDead && !kv.Value.Health.IsDead)
                        {
                            Affect affect = actor.SpecialAbility.Process(actor, kv.Value);
                            if (affect != null)
                            {
                                if (affects[kv.Key].ContainsKey(affect.RootName))
                                {
                                    affects[kv.Key][affect.RootName] = affect;
                                }
                                else
                                {
                                    affects[kv.Key].Add(affect.RootName, affect);
                                }
                            }
                        }
                    }
                }
            }

            return affects;
        }

        public void ClearAffects()
        {
            foreach (Dictionary<String, Affect> affectsPerCharacter in affects.Values)
            {
                affectsPerCharacter.Clear();
            }
        }

        public bool IsGameOver()
        {
            Controller controller = Controller.GetInstance();
            bool isOver = true;

            foreach (Player player in controller.GetPlayers())
            {
                isOver = true;
                foreach (Character character in player.GetCharacters())
                {
                    if (!character.Health.IsDead)
                    {
                        isOver = false;
                        break;
                    }
                }
                if (isOver)
                {
                    break;
                }
            }

            return isOver;
        }

        private void ProcessSpecialAbilityAttack(Character actor, Character target)
        {
            if (actor.SpecialAbility != null && 
                actor.SpecialAbility.Type == SpecialAbilityType.Attack)
            {
                Affect affect = actor.SpecialAbility.Process(actor, target);
                if (affect != null)
                {
                    Dictionary<String, Affect> characterAffects =
                        affects.First(q => q.Key.Owner == target.Owner.Name && 
                            q.Key.Character == target.RootName).Value;

                    if (characterAffects.ContainsKey(affect.RootName))
                    {
                        characterAffects[affect.RootName] = affect;
                    }
                    else
                    {
                        characterAffects.Add(affect.RootName, affect);
                    }
                }
            }
        }
            
        private void IncreaseActionGauges()
        {
            foreach (Character character in characters.Values)
            {
                character.ActionGauge += character.Initiative / 100f;
            }
        }
    }
}