using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Global;

namespace Amude.Domain.Attribute
{
    internal class Health : ICloneable<Health>
    {
        private float actualHealth;
        private float maxHealth;

        public Health(float maxHealth)
        {
            this.maxHealth = maxHealth;
            this.actualHealth = maxHealth;
        }

        public Health(float maxHealth, float actualHealth)
        {
            this.maxHealth = maxHealth;
            this.actualHealth = actualHealth;
        }

        public float Value
        {
            get
            {
                return actualHealth;
            }
        }
        public float MaxValue
        {
            get
            {
                return maxHealth;
            }
        }
        public Boolean IsDead
        {
            get
            {
                return actualHealth == 0;
            }
        }

        public void ReceiveDamage(float damage)
        {
            actualHealth -= damage;
            if (actualHealth < 0)
            {
                actualHealth = 0;
            }
        }

        public void ReceiveHealth(float health)
        {
            actualHealth += health;
            if (actualHealth > maxHealth)
            {
                actualHealth = maxHealth;
            }
            else if(actualHealth < 0)
            {
                actualHealth = 0;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} / {1}", Math.Ceiling(Value).ToString("0"), MaxValue.ToString("0"));
        }

        public static Health operator +(Health value1, Health value2)
        {
            Health ret = new Health(Math.Max(value1.MaxValue, value2.MaxValue), value1.Value);
            ret.ReceiveHealth(value2.Value);
            return ret;
        }

        public static Health operator +(Health value1, float value2)
        {
            Health ret = new Health(value1.MaxValue, value1.Value);
            ret.ReceiveHealth(value2);
            return ret;
        }

        public static Health operator -(Health value1, Health value2)
        {
            Health ret = new Health(Math.Max(value1.MaxValue, value2.MaxValue), value1.Value);
            ret.ReceiveDamage(value2.Value);
            return ret;
        }

        public static Health operator -(Health value1, float value2)
        {
            Health ret = new Health(value1.MaxValue, value1.Value);
            ret.ReceiveDamage(value2);
            return ret;
        }

        #region ICloneable<Health> Members

        public Health Clone()
        {
            return (Health)MemberwiseClone();
        }

        #endregion
    }
}
