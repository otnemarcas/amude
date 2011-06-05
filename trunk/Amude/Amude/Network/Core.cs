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
using Amude.Network.Messages;

namespace Amude.Network
{    
    internal delegate void DataArrival(String address, AbstractMessage message);
    internal delegate void ServerDiscovered(ServerNameMessage serverNameMessage);

    internal class Core
    {
        public const int RECEIVE_BUFFER_SIZE = 4;
        public const int PING = 20;
    }

    internal class Params
    {
        public String address;
        public AbstractMessage message;
    }

}
 