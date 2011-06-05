using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amude.Global
{
    internal static class RandomMath
    {
        #region Random Singleton

        private static Random random = new Random();
        public static Random Random
        {
            get { return random; }
        }

        #endregion

        internal static float RandomBetween(float minimum, float maximum)
        {
            return minimum + (float)random.NextDouble() * (maximum - minimum);
        }

        internal static Boolean Percentage(float chance)
        {
            return chance >= RandomBetween(0,1);
        }
    }
}
