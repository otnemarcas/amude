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
using Microsoft.Xna.Framework;

namespace Amude.Network.Messages
{
    [Serializable]
    internal abstract class ActionMessage : AbstractMessage
    {
        public String PlayerName { get; set; }
        public String Actor { get; set; }
    }
}
