/* 
 FATEC - Carapicuíba

 Trabalho de conclusão de curso - ASTI Jogos

 Gabriel Sacramento do Amaral
 Leandro Roque Razori
 Renato Ibanhez
 Sergio Alberto Pasqualino
 
 Setembro/2010
 */
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Amude.Screen.Core
{
    internal class ScreenManager
    {
        private static ScreenManager instance;
        private Stack<AbstractScreen> screens = null;

        private ScreenManager()
        {
            screens = new Stack<AbstractScreen>();
        }

        public void Update(float passedTime)
        {
            AbstractScreen screen = GetActiveScreen();
            if (screen != null)
            {
                screen.Update(passedTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            AbstractScreen screen = GetActiveScreen();
            if (screen != null)
            {
                screen.Draw(spriteBatch);
            }
        }

        public void AddScreen(AbstractScreen screen)
        {
            screen.LoadContent();
            screens.Push(screen);
        }

        public void RemoveLastScreen()
        {
            AbstractScreen screen = screens.Pop();
            screen.UnloadContent();
        }

        public void RemoveLastScreen(bool loadContent)
        {
            AbstractScreen screen = screens.Pop();
            screen.UnloadContent();
            if (screens.Count > 0 && loadContent)
            {
                screens.Peek().LoadContent();
            }
        }

        public AbstractScreen GetActiveScreen()
        {
            return screens.Peek();
        }

        public static ScreenManager GetInstance()
        {
            if (instance == null)
                instance = new ScreenManager();

            return instance;
        }

    }
}
