/* 
 FATEC - Carapicuíba

 Trabalho de conclusão de curso - ASTI Jogos

 Gabriel Sacramento do Amaral
 Leandro Roque Razori
 Renato Ibanhez
 Sergio Alberto Pasqualino
 
 Outubro/2010
*/
using System.Collections.Generic;
using Amude.Domain;
using System;

namespace Amude.Network.Messages
{
    [Serializable]
    internal class CharacterSelectionMessage : AbstractMessage
    {
        public List<String> SelectedCharacters { get; set; }

        public CharacterSelectionMessage()
        {
            SelectedCharacters = new List<String>();
        }
    }
}
