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
    internal abstract class AbstractMessage
    {
        public Guid Guid { get; set; }
    }
}
