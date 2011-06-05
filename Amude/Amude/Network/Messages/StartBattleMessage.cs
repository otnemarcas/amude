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
using Amude.Network.Messages.Data;
using System.Collections.Generic;
using Amude.Core;
using Amude.Domain;

namespace Amude.Network.Messages
{
    [Serializable]
    internal class StartBattleMessage : AbstractMessage
    {
        public Turn Turn { get; set; }
        public KeyValuePair<CharacterKey, Affect[]>[] Affects { get; set; }
    }
}
