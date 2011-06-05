using Amude.Domain.Attribute;
using System;
using Amude.Global;

namespace Amude.Domain
{
    [Serializable]
    internal class Affect : ICloneable<Affect>
    {
        public String RootName { get; set; }

        public int Duration { get; set; }

        public float Health { get; set; }

        public int Attack { get; set; }

        public int Defense { get; set; }

        public int Agility { get; set; }

        public float Initiative { get; set; }

        #region ICloneable<Affect> Members

        public Affect Clone()
        {
            return (Affect)this.MemberwiseClone();
        }

        #endregion
    }
}
