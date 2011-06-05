/* 
 FATEC - Carapicuíba

 Trabalho de conclusão de curso - ASTI Jogos

 Gabriel Sacramento do Amaral
 Leandro Roque Razori
 Renato Ibanhez
 Sergio Alberto Pasqualino
 
 Outubro/2010
*/
using System;

namespace Amude.Network.Messages
{
    [Serializable]
    internal class SelectServerMessage : AbstractMessage
    {
        public string PlayerName { get; set; }
        public string RemotePlayerName { get; set; }
        public bool Success { get; set; }
    }
}
