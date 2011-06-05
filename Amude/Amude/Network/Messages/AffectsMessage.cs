using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Core;
using Amude.Domain;
using Amude.Network.Messages.Data;

namespace Amude.Network.Messages
{
    [Serializable]
    internal class AffectsMessage : AbstractMessage
    {
        public Turn Turn { get; set; }
        public KeyValuePair<CharacterKey, Affect[]>[] Affects { get; set; }
    }
}
