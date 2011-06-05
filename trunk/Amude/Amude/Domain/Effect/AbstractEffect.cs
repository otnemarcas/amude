using Amude.Domain.Attribute;
using System;

namespace Amude.Domain.Effect
{
    internal abstract class AbstractEffect : Entity
    {
        Character character;

        public AbstractEffect(Character character)
        {
            this.character = character;
            Health = new Health(0);
            Attack = new Attack(AttackType.MELEE,0,0,1);
            Defense = new Defense(0,0);
            Agility = 0;
            Initiative = 0;
        }

        public virtual Health Health { get; set; }

        public virtual Attack Attack { get; set; }

        public virtual Defense Defense { get; set; }

        public virtual float Agility { get; set; }

        public virtual float Initiative { get; set; }

        #region IDisposable Members

        public override void Dispose()
        {
            this.character = null;
            Health = null;
            Attack = null;
            Defense = null;

            base.Dispose();
        }

        #endregion
    }
}
