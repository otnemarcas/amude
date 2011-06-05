/* 
 FATEC - Carapicuíba

 Trabalho de conclusão de curso - ASTI Jogos

 Gabriel Sacramento do Amaral
 Leandro Roque Razori
 Renato Ibanhez
 Sergio Alberto Pasqualino
 
 Setembro/2010
 */
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Amude.Screen.Core;
using Amude.Core;
using System.Collections.Generic;
using Amude.Domain;
using Amude.Graphics;
using Amude.Screen.Core.MenuItem;
using Amude.Global;

namespace Amude.Screen
{
    internal class MainMenuScreen : AbstractScreen
    {
        private const int DEFAULT_MENUITEM_X = 884;
        private const int DEFAULT_MENUITEM_Y = 120;
        private const int DEFAULT_MENUITEM_SPACING = 50;

        protected static string MAINMENU_ROOT_DIRECTORY = MENU_ROOT_DIRECTORY + "MainMenuScreen/";

        private List<IMenuItem> menuItems;
        private Texture2D containerTexture;
        private Vector2 containerPosition;
        private Pointer pointer;

        public MainMenuScreen()
        {
            containerTexture = IO.LoadSingleTexture(CONTAINER_PATH);
            containerPosition = new Vector2(852, 0);

            menuItems = new List<IMenuItem>();

            menuItems.Add(new ImageMenuItem("newgame", MAINMENU_ROOT_DIRECTORY));
            menuItems.Add(new ImageMenuItem("options", MAINMENU_ROOT_DIRECTORY));
            menuItems.Add(new ImageMenuItem("help", MAINMENU_ROOT_DIRECTORY));
            menuItems.Add(new ImageMenuItem("about", MAINMENU_ROOT_DIRECTORY));
            menuItems.Add(new ImageMenuItem("exit", MAINMENU_ROOT_DIRECTORY));

            int height = ((ImageMenuItem)menuItems[0]).Height;
            for (int i = 0; i < 4; i++)
            {
                menuItems[i].Position = new Vector2(DEFAULT_MENUITEM_X, DEFAULT_MENUITEM_Y + (height + DEFAULT_MENUITEM_SPACING) * i);
            }
            menuItems[4].Position = new Vector2(DEFAULT_MENUITEM_X, 650);

            pointer = new Pointer(menuItems);
        }

        public override void LoadContent() 
        {
            AudioManager.PlayMusic("mainTheme", true);
            pointer.Selection = 0;
        }

        public override void UnloadContent() { }

        public override void Update(float passedTime)
        {
            pointer.Update(passedTime);
            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Enter))
            {
                if (pointer.Selection == 0)
                {
                    ScreenManager.GetInstance().AddScreen(new ScriptScreen());
                    AudioManager.PlaySound("menu_accept");
                }
                if (pointer.Selection == 1)
                {
                    ScreenManager.GetInstance().AddScreen(new OptionsScreen());
                    AudioManager.PlaySound("menu_accept");
                }

                if (pointer.Selection == 2)
                {
                    ScreenManager.GetInstance().AddScreen(new HelpScreen());
                    AudioManager.PlaySound("menu_accept");
                }

                if (pointer.Selection == 3)
                {
                    ScreenManager.GetInstance().AddScreen(new AboutScreen());
                    AudioManager.PlaySound("menu_accept");
                }

                if (pointer.Selection == 4)
                {
                    Controller.GetInstance().Exit();
                }
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Up))
            {
                pointer.Selection -= 1;
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Down))
            {
                pointer.Selection += 1;
            }

            foreach (IMenuItem menuItem in menuItems)
            {
                menuItem.Update(passedTime);
            }

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.Draw(containerTexture, containerPosition, null, Color.White, 0,
                Vector2.Zero, 1, SpriteEffects.None, Constants.LD_BACKLAYER_1);
            pointer.Draw(spriteBatch);

            foreach (IMenuItem menuItem in menuItems)
            {
                menuItem.Draw(spriteBatch);
            }            
        }
    }
}
