using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amude.Network.Messages
{
    [Serializable]
    internal class ActionMessageAttack : ActionMessage
    {
        public String TargetPlayer { get; set; }
        public String Target { get; set; }
        public float Damage { get; set; }
        public bool CounterAttack { get; set; }
        public float CounterDamage { get; set; }
    }
}
