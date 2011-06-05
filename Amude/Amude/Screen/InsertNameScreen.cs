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
using System.Linq;
using Amude.Core;
using Amude.Domain;
using Amude.Graphics;
using Amude.Screen.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Amude.Global;
using System.Text;
using Amude.Screen.Core.MenuItem;

namespace Amude.Screen
{
    internal class InsertNameScreen : AbstractScreen
    {
        protected static string INSERTNAME_ROOT_DIRECTORY = MENU_ROOT_DIRECTORY + "InsertNameScreen/";

        const int DEFAULT_MENUITEM_X = 380;
        const int DEFAULT_MENUITEM_Y = 500;
        const int DEFAULT_MENUITEM_SPACING = 230;
        const int USERNAME_MAX_LENTH = 15;

        private bool isServer;
        private SpriteFont spriteFont;
        private StringBuilder userName;
        private Vector2 userNamePosition;
        
        private Texture2D containerTexture;
        private Vector2 containerPosition;
        private List<IMenuItem> menuItems;
        private Pointer pointer;
        private Texture2D title;
        private Vector2 titlePosition;
        private Texture2D line;
        private Vector2 linePosition;

        public InsertNameScreen(bool isServer)
        {
            this.isServer = isServer;
            containerTexture = IO.LoadSingleTexture(INSERTNAME_ROOT_DIRECTORY + "container");
            containerPosition = new Vector2(320, 200);

            spriteFont = IO.LoadFont("Data/Global/AmudeFont");

            userName = new StringBuilder(Controller.GetInstance().GetLocalPlayer().Name);
            userNamePosition = new Vector2(450, 350);

            menuItems = new List<IMenuItem>();

            title = IO.LoadSingleTexture(INSERTNAME_ROOT_DIRECTORY + "playername");
            titlePosition = new Vector2(370, 260);

            line = IO.LoadSingleTexture(INSERTNAME_ROOT_DIRECTORY + "line");
            linePosition = new Vector2(440, 395);
            List<Texture2D> spritesLine = new List<Texture2D>();

            string menuItemsPath = INSERTNAME_ROOT_DIRECTORY;
            menuItems.Add(new ImageMenuItem("continue", menuItemsPath));
            menuItems.Add(new ImageMenuItem("cancel", menuItemsPath));

            int height = ((ImageMenuItem)menuItems[0]).Height;
            for (int i = 0; i < menuItems.Count; i++)
            {
                menuItems[i].Position = new Vector2(DEFAULT_MENUITEM_X + (height + DEFAULT_MENUITEM_SPACING) * i,  
                                                    DEFAULT_MENUITEM_Y);
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

            var keys = from key in KeyboardManager.Manager.TypedKeys
                        where ((key >= Keys.A && key <= Keys.Z) || 
                        (key >= Keys.NumPad0 && key <= Keys.NumPad9)) || 
                        (key >= Keys.D0 && key <= Keys.D9) ||
                        key == Keys.Back ||
                        key == Keys.Space
                        select key;

            foreach (Keys key in keys)
            {
                if (userName.Length < USERNAME_MAX_LENTH && key != Keys.Back)
                {
                    string k = SharedFunctions.GetText(key);
                    
                    if (KeyboardManager.Manager.IsCapsLock ^ KeyboardManager.Manager.IsShift)
                        userName.Append(k.Substring(k.Length - 1).ToUpper());
                    else
                        userName.Append(k.Substring(k.Length - 1).ToLower());

                    if (spriteFont.MeasureString(userName.ToString()).X >= line.Width)
                    {
                        userName.Remove(userName.Length - 1, 1);
                    }
                }

                if (key == Keys.Back && userName.Length > 0)
                {
                    userName.Remove(userName.Length - 1, 1);
                }
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Escape))
            {
                ScreenManager.GetInstance().RemoveLastScreen();
                AudioManager.PlaySound("menu_accept");
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Enter))
            {
                if (pointer.Selection == 0 && userName.Length > 0)
                {
                    Controller.GetInstance().GetLocalPlayer().Name = userName.ToString();

                    if (isServer)
                    {
                        Controller.GetInstance().InitializeServer();
                    }
                    else
                    {
                        ScreenManager.GetInstance().RemoveLastScreen();
                        ScreenManager.GetInstance().AddScreen(new RoomsScreen());
                    }
                    
                    AudioManager.PlaySound("menu_accept");
                }

                else if (pointer.Selection == 1)
                {
                    AudioManager.PlaySound("menu_accept");
                    ScreenManager.GetInstance().RemoveLastScreen();
                }
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Left))
            {
                pointer.Selection--;
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Right))
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
            spriteBatch.Draw(containerTexture, containerPosition, null, Color.White, 0,
                            Vector2.Zero, 1, SpriteEffects.None, Constants.LD_BACKLAYER_1);

            pointer.Draw(spriteBatch);

            spriteBatch.Draw(title, titlePosition, null, Color.White, 0, Vector2.Zero, 
                1, SpriteEffects.None, Constants.LD_INFO);

            spriteBatch.Draw(line, linePosition, null, Color.White, 0, Vector2.Zero, 1, 
                SpriteEffects.None, Constants.LD_INFO);
            
            spriteBatch.DrawString(spriteFont, userName, userNamePosition, Color.Black,
                0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);

            foreach (IMenuItem menuItem in menuItems)
            {
                menuItem.Draw(spriteBatch);
            }            

            base.Draw(spriteBatch);
        }
    }
}
