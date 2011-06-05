using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amude.Domain.Attribute.SpecialAbility
{
    class ParalysingWave : ISpecialAbility
    {
        #region ISpecialAbility Members

        public void Process(Character actor, Character target)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
