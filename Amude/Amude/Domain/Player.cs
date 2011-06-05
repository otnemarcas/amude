/* 
 FATEC - Carapicuíba

 Trabalho de conclusão de curso - ASTI Jogos

 Gabriel Sacramento do Amaral
 Leandro Roque Razori
 Renato Ibanhez
 Sergio Alberto Pasqualino
 
 Setembro/2010
 */
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Net;


namespace Amude.Domain
{
    internal class Player
    {
        protected Dictionary<String, Character> characters;
        protected String name;
        protected bool ready;
        protected IPAddress ipAddress;

        public Player(IPAddress address)
        {
            characters = new Dictionary<string, Character>();
            name = String.Empty;
            ipAddress = address;
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public bool Ready
        {
            get { return ready; }
            set { ready = value; }
        }

        public IPAddress IPAddress
        {
            get { return ipAddress; }
            set { ipAddress = value; }
        }
        
        public void AddCharacter(Character character)
        {
            character.Owner = this;
            characters.Add(character.RootName, character);
        }

        public Character GetCharacter(String characterName)
        {
            return characters[characterName];
        }

        public List<Character> GetCharacters()
        {
            return characters.Values.ToList();
        }

        public void ClearCharacters()
        {
            characters.Clear();
        }

        public void Update(float passedTime)
        {
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            
        }
    }
}
