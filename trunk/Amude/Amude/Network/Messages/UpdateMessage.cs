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
using Amude.Network.Messages.Data;
using System.Collections.Generic;
using Amude.Domain;
using Amude.Core;

namespace Amude.Network.Messages
{
    [Serializable]
    internal class UpdateMessage : AbstractMessage
    {
        public ActionMessage Action { get; set; }
    }
}
