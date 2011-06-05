using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amude.Network.Messages
{
    internal enum ConfirmationType
    {
        UpdateMessage,
        AffectsMessage
    }

    [Serializable]
    internal class ConfirmUpdateMessage : AbstractMessage
    {
        public ConfirmationType ConfirmationType { get; set; }
    }
}
