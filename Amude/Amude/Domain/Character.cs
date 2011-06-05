/* 
 FATEC - Carapicuíba

 Trabalho de conclusão de curso - ASTI Jogos

 Gabriel Sacramento do Amaral
 Leandro Roque Razori
 Renato Ibanhez
 Sergio Alberto Pasqualino
 
 Setembro/2010
 */

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Amude.Motion;
using Amude.Graphics;
using System;
using Amude.Core;
using Amude.Domain.Attribute;
using System.Linq;
using System.Data.Linq;
using Amude.Global;
using Amude.Screen.Component.Game;

namespace Amude.Domain
{
    internal class Character : Entity, ICloneable<Character>
    {
        protected Health health;
        protected Attack attack;
        protected Defense defense;
        protected int agility;
        protected float initiative;

        protected Character target;
        protected AnimationType targetAnimation;
        protected float targetDamage;
        protected bool targetCounterAttack;
        protected float targetCounterDamage;

        protected Dictionary<Affect, Animation> affects;
        private bool drawingAffects;

        protected HealthVariation healthVariation;

        public Character()
            : base()
        {
            affects = new Dictionary<Affect, Animation>();
            healthVariation = new HealthVariation(this);
            CounterAttack = true;
            drawingAffects = false;
        }

        public Player Owner { get; set; }

        public SpecialAbility SpecialAbility { get; set; }

        public Health Health
        {
            get
            {
                return health;
            }
            set
            {
                health = value;
            }
        }

        public Attack Attack
        {
            get
            {
                Attack ret = attack;
                foreach (Affect affect in affects.Keys)
                {
                    ret += affect.Attack;
                }
                return ret;
            }
            set
            {
                attack = value;
            }
        }

        public Defense Defense
        {
            get
            {
                Defense ret = defense;
                foreach (Affect affect in affects.Keys)
                {
                    ret += affect.Defense;
                }
                return ret;
            }
            set
            {
                defense = value;
            }
        }

        public int Agility
        {
            get
            {
                int ret = agility;
                foreach (Affect affect in affects.Keys)
                {
                    ret += affect.Agility;
                }
                return ret;
            }
            set
            {
                agility = value;
            }
        }

        public float Initiative
        {
            get
            {
                float ret = initiative;
                foreach (Affect affect in affects.Keys)
                {
                    ret += affect.Initiative;
                }
                return ret;
            }
            set
            {
                initiative = value;
            }
        }

        public IEnumerable<Affect> Affects
        {
            get
            {
                return affects.Keys;
            }
        }

        public HealthVariation HealthVariation
        {
            get
            {
                return healthVariation;
            }
        }

        public bool DrawingAffects
        {
            get
            {
                return drawingAffects;
            }
        }

        public Character Target
        {
            get
            {
                return target;
            }
        }

        public float ActionGauge { get; set; }

        public string Description { get; set; }

        public bool Ocupied { get; set; }

        public float ProjectileVelocity { get; set; }

        public bool CounterAttack { get; set; }

        override public void Update(float passedTime)
        {
            AnimatedMovement action = currentActions.Peek();

            if (IsIdle() && !Ocupied && !drawingAffects && RandomMath.Percentage(0.001f))
            {
                if (DefaultDirection == DefaultDirection.Right)
                {
                    AddAnimatedMovement(AnimationType.NoneRight);
                }
                else
                {
                    AddAnimatedMovement(AnimationType.NoneLeft);
                }

                currentActions.Peek().Origin = action.Movement.Position;
            }

            base.Update(passedTime);

            bool affectsFinished = true;
            foreach (Animation affectAnimation in affects.Values)
            {
                if (affectAnimation != null)
                {
                    if (affectAnimation.IsFinalized())
                    {
                        affectAnimation.IsVisible = false;
                    }
                    else
                    {
                        affectsFinished = false;
                    }

                    affectAnimation.Update(passedTime);
                    affectAnimation.Position = this.Position;
                    affectAnimation.LayerDepth = this.LayerDepth - 0.03f;
                }
            }

            drawingAffects = !affectsFinished;

            if (target != null && (IsReady() || IsIdle()) && !HealthVariation.IsVisible())
            {
                target.Health.ReceiveDamage(targetDamage);

                if (target.Health.IsDead)
                {
                    if (targetAnimation == AnimationType.HitRight)
                    {
                        target.AddAnimatedMovement(AnimationType.DieRight, target.MapLocation);
                        target.AddAnimatedMovement(AnimationType.DeadRight, target.MapLocation);
                    }
                    else if (targetAnimation == AnimationType.HitLeft)
                    {
                        target.AddAnimatedMovement(AnimationType.DieLeft, target.MapLocation);
                        target.AddAnimatedMovement(AnimationType.DeadLeft, target.MapLocation);
                    }

                    target.Ocupied = true;
                }

                else
                {
                    target.AddAnimatedMovement(targetAnimation, target.MapLocation);
                    target.Ocupied = false;
                }

                target.ShowHealthVariation(targetDamage * -1.0f);

                if (targetCounterAttack)
                {
                    target.AttackCharacter(this, targetCounterDamage, false, 0);
                }

                target = null;
            }

            healthVariation.Update(passedTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            healthVariation.Draw(spriteBatch);
            base.Draw(spriteBatch);

            if (drawingAffects)
            {
                foreach (Animation affectAnimation in affects.Values)
                {
                    if (affectAnimation != null)
                        affectAnimation.Draw(spriteBatch);
                }
            }
        }

        public void UpdateAffects(Affect[] updateAffects)
        {
            if (Health.IsDead)
            {
                affects.Clear();
            }
            else
            {
                List<Affect> affectsToRemove = new List<Affect>();

                foreach (Affect affect in affects.Keys)
                {
                    if ((affect.Duration == 1 && Bundle.GetSpecialAbility(affect.RootName).Type ==
                        SpecialAbilityType.Passive))
                    {
                        affectsToRemove.Add(affect);
                    }
                }

                foreach (Affect affect in affectsToRemove)
                {
                    affects.Remove(affect);
                }

                foreach (Affect affect in updateAffects)
                {
                    KeyValuePair<Affect, Animation> oldAffect = affects.FirstOrDefault(q => q.Key.RootName == affect.RootName);
                    if (oldAffect.Key != null)
                    {
                        affects.Remove(oldAffect.Key);
                    }

                    affects.Add(affect.Clone(), null);
                }
            }
        }

        public void DoAffects(bool turn)
        {
            List<Affect> oldAffects = new List<Affect>();
            float totalHealthAffect = 0;
            bool damaged = false;

            drawingAffects = true;

            List<KeyValuePair<Affect, Animation>> newAnimations = new List<KeyValuePair<Affect, Animation>>();

            foreach (Affect affect in affects.Keys)
            {
                if (affect.Duration == 0 || Health.IsDead)
                {
                    oldAffects.Add(affect);
                }
                else
                {
                    if (turn)
                    {
                        affect.Duration--;
                        Animation affectAnimation = Bundle.GetSpecialAbilityAnimation(affect.RootName);
                        affectAnimation.Position = this.Position;
                        affectAnimation.Redefine();
                        affectAnimation.Start();
                        affectAnimation.IsVisible = true;

                        if (affect.Health != 0)
                        {
                            totalHealthAffect += affect.Health;

                            if (affect.Health < 0)
                                damaged = true;                            
                        }

                        newAnimations.Add(new KeyValuePair<Affect, Animation>(affect, affectAnimation));
                    }
                }
            }

            if (totalHealthAffect != 0)
            {
                float oldHealth = health.Value;
                health.ReceiveHealth(totalHealthAffect);

                if (health.Value - oldHealth != 0)
                {
                    ShowHealthVariation(totalHealthAffect);
                }
            }

            if (damaged)
            {
                if (Health.IsDead)
                {
                    Ocupied = true;

                    if (DefaultDirection == DefaultDirection.Right)
                    {
                        ClearAnimatedMovements();
                        AddAnimatedMovement(AnimationType.DieRight, MapLocation);
                        AddAnimatedMovement(AnimationType.DeadRight, MapLocation);
                    }
                    else
                    {
                        ClearAnimatedMovements();
                        AddAnimatedMovement(AnimationType.DieLeft, MapLocation);
                        AddAnimatedMovement(AnimationType.DeadLeft, MapLocation);
                    }
                }
                else
                {
                    if (DefaultDirection == DefaultDirection.Right)
                    {
                        ClearAnimatedMovements();
                        AddAnimatedMovement(AnimationType.HitRight, MapLocation);
                    }
                    else
                    {
                        ClearAnimatedMovements();
                        AddAnimatedMovement(AnimationType.HitLeft, MapLocation);
                    }
                }
            }

            foreach (Affect affect in oldAffects)
            {
                affects.Remove(affect);
            }
            foreach (KeyValuePair<Affect, Animation> kv in newAnimations)
            {
                affects[kv.Key] = kv.Value;
            }
        }

        public void AttackCharacter(Character target, float damage, bool counterAttack, float counterDamage)
        {
            Point attackLocation;
            AnimationType attackAnimation = AnimationType.StaticRight;
            AnimationType hitAnimation;

            if (target.DefaultDirection == DefaultDirection.Left)
            {
                hitAnimation = AnimationType.HitLeft;
            }
            else
            {
                hitAnimation = AnimationType.HitRight;
            }

            if (Attack.Type == AttackType.MELEE)
            {
                attackLocation = this.MapLocation;
            }
            else
            {
                attackLocation = target.MapLocation;
            }

            if (target.MapLocation.X > MapLocation.X)
            {
                attackAnimation = AnimationType.AttackRight;
                hitAnimation = AnimationType.HitLeft;
            }
            else if (target.MapLocation.X < MapLocation.X)
            {
                attackAnimation = AnimationType.AttackLeft;
                hitAnimation = AnimationType.HitRight;
            }
            else if (target.MapLocation.Y > MapLocation.Y)
            {
                attackAnimation = AnimationType.AttackDown;
            }
            else if (target.MapLocation.Y < MapLocation.Y)
            {
                attackAnimation = AnimationType.AttackUp;
            }

            target.Ocupied = true;
            this.target = target;
            this.targetAnimation = hitAnimation;
            this.targetDamage = damage;
            this.targetCounterAttack = counterAttack;
            this.targetCounterDamage = counterDamage;

            if (attackAnimation != AnimationType.StaticRight)
            {
                if (Attack.Type == AttackType.MELEE)
                {
                    AddAnimatedMovement(attackAnimation, attackLocation);
                }
                else
                {
                    AddAnimatedMovement(attackAnimation, MapLocation, attackLocation, ProjectileVelocity);
                }
            }
        }

        public void ShowHealthVariation(float variation)
        {
            healthVariation.ShowVariation(variation);
        }

        #region ICloneable<Character> Members

        // @SupressWarning
        public Character Clone()
        {
            Character clone = (Character)base.Clone();
            clone.affects = new Dictionary<Affect, Animation>();
            clone.healthVariation = new HealthVariation(clone);
            clone.health = this.health.Clone();
            clone.attack = this.attack.Clone();
            clone.defense = this.defense.Clone();
            clone.Description = Description;
            clone.agility = this.agility;
            clone.initiative = this.initiative;
            clone.Owner = this.Owner;
            clone.ProjectileVelocity = this.ProjectileVelocity;
            clone.SpecialAbility = SpecialAbility;
            clone.CounterAttack = true;

            return clone;
        }

        #endregion

        #region IDisposable Members

        public override void Dispose()
        {
            if (affects != null)
            {
                foreach (Animation affectAnimation in affects.Values)
                {
                    if(affectAnimation != null)
                        affectAnimation.Dispose();
                }
            }

            health = null;
            attack = null;
            defense = null;
            Description = null;
            Owner = null;
            SpecialAbility = null;
            healthVariation = null;

            base.Dispose();
        }

        #endregion
    }
}
