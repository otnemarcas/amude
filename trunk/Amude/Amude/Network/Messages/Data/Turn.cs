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

namespace Amude.Network.Messages.Data
{
    [Serializable]
    internal class Turn
    {
        public String CharacterName { get; set;}
        public String PlayerName { get; set; }
        public Boolean HasWalked { get; set; }

        public Turn(String characterName, String playerName, Boolean hasWalked)
        {
            CharacterName = characterName;
            PlayerName = playerName;
            HasWalked = hasWalked;
        }
    }
}
