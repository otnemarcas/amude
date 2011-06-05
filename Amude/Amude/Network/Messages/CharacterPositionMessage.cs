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
using Amude.Domain;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Amude.Network.Messages
{
    [Serializable]
    internal class CharacterPositionMessage : AbstractMessage
    {
        public String PlayerName { get; set; }
        public Dictionary<String, Point> CharactersPosition { get; set; }

        public CharacterPositionMessage()
        {
            CharactersPosition = new Dictionary<String, Point>();
        }

        public void AddCharacter(String characterName, Point mapLocation)
        {
            if (!CharactersPosition.ContainsKey(characterName))
            {
                CharactersPosition.Add(characterName, mapLocation);
            }        
        }
    }
}
