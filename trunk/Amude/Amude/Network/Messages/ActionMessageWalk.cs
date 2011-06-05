using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Amude.Network.Messages
{
    [Serializable]
    internal class ActionMessageWalk : ActionMessage
    {
        public Point Point { get; set; }
    }
}
