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
using Amude.Core;
using Amude.Domain;
using Amude.Graphics;
using Amude.Screen.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Amude.Screen.Core.MenuItem;
using Amude.Global;

namespace Amude.Screen
{
    internal class NewGameScreen : AbstractScreen
    {
        private const int DEFAULT_MENUITEM_X = 884;
        private const int DEFAULT_MENUITEM_Y = 120;
        const int DEFAULT_MENUITEM_SPACING = 50;

        protected static string CREATEGAME_ROOT_DIRECTORY = MENU_ROOT_DIRECTORY + "NewGameScreen/";

        private List<IMenuItem> menuItems;
        private Texture2D containerTexture;
        private Vector2 containerPosition;
        private Pointer pointer;

        public NewGameScreen()
        {                        
            containerTexture = IO.LoadSingleTexture(CONTAINER_PATH);
            containerPosition = new Vector2(852, 0);

            menuItems = new List<IMenuItem>();

            menuItems.Add(new ImageMenuItem("creategame", CREATEGAME_ROOT_DIRECTORY));
            menuItems.Add(new ImageMenuItem("joingame", CREATEGAME_ROOT_DIRECTORY));
            menuItems.Add(new ImageMenuItem("back", CREATEGAME_ROOT_DIRECTORY));

            int height = ((ImageMenuItem)menuItems[0]).Height;
            for (int i = 0; i < menuItems.Count; i++)
            {
                menuItems[i].Position = new Vector2(DEFAULT_MENUITEM_X, DEFAULT_MENUITEM_Y + (height + DEFAULT_MENUITEM_SPACING) * i);
            }

            pointer = new Pointer(menuItems);
        }


        public override void LoadContent()
        {
            pointer.Selection = 0;
        }

        public override void UnloadContent() { }

        public override void Update(float passedTime)
        {
            pointer.Update(passedTime);

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Escape))
            {
                ScreenManager.GetInstance().RemoveLastScreen();
                AudioManager.PlaySound("menu_accept");
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Enter))
            {
                if (pointer.Selection == 0)
                {
                    ScreenManager.GetInstance().RemoveLastScreen();
                    ScreenManager.GetInstance().AddScreen(new InsertNameScreen(true));
                    AudioManager.PlaySound("menu_accept");
                }
                if (pointer.Selection == 1)
                {
                    ScreenManager.GetInstance().RemoveLastScreen();
                    ScreenManager.GetInstance().AddScreen(new InsertNameScreen(false));
                    AudioManager.PlaySound("menu_accept");
                }
                if (pointer.Selection == 2)
                {
                    ScreenManager.GetInstance().RemoveLastScreen();
                    AudioManager.PlaySound("menu_accept");
                }
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Up))
            {
                pointer.Selection--;
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Down))
            {
                pointer.Selection++;
            }

            foreach (IMenuItem menuItem in menuItems)
            {
                menuItem.Update(passedTime);
            }

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            pointer.Draw(spriteBatch);
            spriteBatch.Draw(containerTexture, containerPosition, null, Color.White, 0,
                            Vector2.Zero, 1, SpriteEffects.None, Constants.LD_BACKLAYER_1);

            foreach (IMenuItem menuItem in menuItems)
            {
                menuItem.Draw(spriteBatch);
            }            
            
        }

    }
}
