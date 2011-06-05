using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Network.Messages.Data;

namespace Amude.Network.Messages
{
    [Serializable]
    internal class TurnMessage : AbstractMessage
    {
        public Turn Turn { get; set; }
    }
}
