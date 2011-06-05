using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Core;
using Amude.Global;

namespace Amude.Domain.Attribute
{
    internal class Defense : ICloneable<Defense>
    {
        private float minValue;
        private float maxValue;

        public Defense(float minValue, float maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentException();
            }

            this.minValue = minValue;
            this.maxValue = maxValue;

            if (this.minValue < 0)
            {
                this.minValue = 0;
            }
            if (this.maxValue < 0)
            {
                this.maxValue = 0;
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

        public override string ToString()
        {
            return string.Format("{0} - {1}", MinValue, MaxValue);
        }

        public static Defense operator +(Defense value1, Defense value2)
        {
            return new Defense(value1.MinValue + value2.MinValue,
                value1.MaxValue + value2.MaxValue);
        }

        public static Defense operator +(Defense value1, float value2)
        {
            return new Defense(value1.MinValue + value2, value1.MaxValue + value2);
        }

        public static Defense operator -(Defense value1, Defense value2)
        {
            return new Defense(value1.MinValue - value2.MinValue,
                value1.MaxValue - value2.MaxValue);
        }

        public static Defense operator -(Defense value1, float value2)
        {
            return new Defense(value1.MinValue - value2, value1.MaxValue - value2);
        }

        #region ICloneable<Defense> Members

        public Defense Clone()
        {
            return (Defense)MemberwiseClone();
        }

        #endregion
    }
}
