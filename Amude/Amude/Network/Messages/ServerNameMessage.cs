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
using System.Net;

namespace Amude.Network.Messages
{
    [Serializable]
    internal class ServerNameMessage : AbstractMessage
    {
        public IPAddress ServerAddress { get; set; }
        public String ServerName { get; set; }
        public String PlayerName { get; set; }

        public ServerNameMessage(String serverName, String playerName)
        {
            this.ServerName = serverName;
            this.PlayerName = playerName;
        }
    }
}
