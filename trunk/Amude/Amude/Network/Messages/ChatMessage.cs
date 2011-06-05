using System;

namespace Amude.Network.Messages
{
    [Serializable]
    internal class ChatMessage : AbstractMessage
    {
        public String UserName { get; set; }
        public String Message { get; set; }
    }
}
