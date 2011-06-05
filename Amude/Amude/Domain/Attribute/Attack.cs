using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Core;
using Amude.Global;

namespace Amude.Domain.Attribute
{
    internal enum AttackType
    {
        MELEE,
        RANGED 
    }

    internal class Attack : ICloneable<Attack>
    {
        private AttackType type;
        private float minValue;
        private float maxValue;
        private int range;

        public Attack(AttackType type, float minValue, float maxValue, int range)
        {
            this.type = type;
            if (minValue > maxValue)
            {
                throw new ArgumentException();
            }
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.range = range;

            if (this.minValue < 0)
            {
                this.minValue = 0;
            }
            if (this.maxValue < 0)
            {
                this.maxValue = 0;
            }
        }

        public AttackType Type
        {
            get
            {
                return type;
            }
        }

        public float MinValue
        {
            get
            {
                return minValue;
            }
        }

        public float MaxValue
        {
            get
            {
                return maxValue;
            }
        }

        public float Value
        {
            get
            {
                return RandomMath.RandomBetween(minValue,maxValue);
            }
        }

        public int Range
        {
            get
            {
                return range;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", MinValue, MaxValue);
        }

        public static Attack operator +(Attack value1, Attack value2)
        {
            return new Attack(value1.Type, value1.MinValue + value2.MinValue,
                value1.MaxValue + value2.MaxValue, Math.Max(value1.Range, value2.Range));
        }

        public static Attack operator +(Attack value1, float value2)
        {
            return new Attack(value1.Type, value1.MinValue + value2,
                value1.MaxValue + value2, value1.Range);
        }

        public static Attack operator -(Attack value1, Attack value2)
        {
            return new Attack(value1.Type, value1.MinValue - value2.MinValue,
                value1.MaxValue - value2.MaxValue, Math.Min(value1.Range, value2.Range));
        }

        public static Attack operator -(Attack value1, float value2)
        {
            return new Attack(value1.Type, value1.MinValue - value2,
                value1.MaxValue - value2, value1.Range);
        }

        #region ICloneable<Attack> Members

        public Attack Clone()
        {
            return (Attack)MemberwiseClone();
        }

        #endregion
    }
}
