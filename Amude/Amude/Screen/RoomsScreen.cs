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
using System.Collections.Generic;
using Amude.Core;
using Amude.Global;
using Amude.Network.Messages;
using Amude.Screen.Core;
using Amude.Screen.Core.MenuItem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Amude.Network;
using Amude.Graphics;
using System.Threading;

namespace Amude.Screen
{
    internal class RoomsScreen : AbstractScreen
    {
        protected static string ROOMS_ROOT_DIRECTORY = MENU_ROOT_DIRECTORY + "RoomsScreen/";

        private const int DEFAULT_MENUITEM_X = 500;
        private const int DEFAULT_MENUITEM_Y = 150;
        const int DEFAULT_MENUITEM_SPACING = 40;

        private List<ServerNameMessage> rooms;
        private List<IMenuItem> menuItems;
        private Texture2D title;
        private Vector2 titlePosition;
        private Texture2D containerTexture;
        private Vector2 containerPosition;
        private Pointer pointer;

        private DateTime beginSearch;
        private Animation searchIcon;
        private Boolean searchServers;
        private int searchTime = 10;

        public RoomsScreen()
        {
            containerTexture = IO.LoadSingleTexture(ROOMS_ROOT_DIRECTORY + "container");
            containerPosition = new Vector2(320, 100);

            menuItems = new List<IMenuItem>();

            Controller.GetInstance().InitializeClient();

            searchServers = true;
            Controller.GetInstance().Client.FindServers(new ServerDiscovered(ServerDiscovered));
            beginSearch = DateTime.Now;

            title = IO.LoadSingleTexture(ROOMS_ROOT_DIRECTORY + "availablegames");
            titlePosition = new Vector2(480, 150);
            rooms = new List<ServerNameMessage>();

            menuItems.Add(new ImageMenuItem("update", ROOMS_ROOT_DIRECTORY));
            menuItems.Add(new ImageMenuItem("back", ROOMS_ROOT_DIRECTORY));

            menuItems[0].Position = new Vector2(DEFAULT_MENUITEM_X - 100, 650);
            menuItems[1].Position = new Vector2(DEFAULT_MENUITEM_X + 100, 650);

            pointer = new Pointer(menuItems);

            searchIcon = new Animation(AnimationType.StaticRight, 1f,
            IO.LoadSprite(ROOMS_ROOT_DIRECTORY + "searching", 24));
            searchIcon.ColorModifier = new Color(255, 0, 0);
            searchIcon.IsCyclic = true;
            searchIcon.Position = new Vector2(DEFAULT_MENUITEM_X + 100, 350);
            searchIcon.Start();
        }

        public override void LoadContent()
        {
            pointer.Selection = 0;
        }

        public override void UnloadContent() { }

        public override void Update(float passedTime)
        {
            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Escape))
            {
                Controller.GetInstance().StopClient();
                ScreenManager.GetInstance().RemoveLastScreen();
                AudioManager.PlaySound("menu_accept");
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Enter))
            {
                if (pointer.Selection < rooms.Count)
                {
                    Controller.GetInstance().Client.SetServerAddress(rooms[pointer.Selection].ServerAddress);
                    Controller.GetInstance().Client.SendData(new SelectServerMessage() { PlayerName = Controller.GetInstance().GetLocalPlayer().Name });

                    AudioManager.PlaySound("menu_accept");
                }
                else if (pointer.Selection == rooms.Count)
                {
                    searchServers = true;
                    rooms = new List<ServerNameMessage>();
                    Controller.GetInstance().Client.StopFindServers();
                    Controller.GetInstance().Client.FindServers(new ServerDiscovered(ServerDiscovered));
                    beginSearch = DateTime.Now;
                }
                else
                {
                    Controller.GetInstance().StopClient();
                    ScreenManager.GetInstance().RemoveLastScreen();
                    AudioManager.PlaySound("menu_accept");
                }
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Up) || KeyboardManager.Manager.TypedKeys.Contains(Keys.Left))
            {
                pointer.Selection--;
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Down) || KeyboardManager.Manager.TypedKeys.Contains(Keys.Right))
            {
                pointer.Selection++;
            }

            if (searchServers && DateTime.Now.Subtract(beginSearch).Seconds > searchTime)
            {
                if (Controller.GetInstance().Client != null)
                {
                    Controller.GetInstance().Client.StopFindServers();
                }
                searchServers = false;
            }

            pointer.Update(passedTime);

            searchIcon.Update(passedTime);

            foreach (IMenuItem menuItem in menuItems)
            {
                menuItem.Update(passedTime);
            }

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(containerTexture, containerPosition, null, Color.White, 0,
                Vector2.Zero, 1, SpriteEffects.None, Constants.LD_BACKLAYER_1);

            spriteBatch.Draw(title, titlePosition, null, Color.White, 0,
                Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);

            pointer.Draw(spriteBatch);

            if (searchServers)
            {
                searchIcon.Draw(spriteBatch);
            }

            foreach (IMenuItem menuItem in menuItems)
            {
                menuItem.Draw(spriteBatch);
            }

            base.Draw(spriteBatch);
        }

        private void ServerDiscovered(ServerNameMessage message)
        {
            lock (this)
            {
                Controller.GetInstance().SupressUpdate();

                rooms.Add(message);

                menuItems.Clear();

                for (int i = 0; i < rooms.Count; i++)
                {
                    TextMenuItem textItem = new TextMenuItem(rooms[i].PlayerName);

                    textItem.Position = new Vector2(DEFAULT_MENUITEM_X - 40,
                        DEFAULT_MENUITEM_Y + (i + 2) * DEFAULT_MENUITEM_SPACING + 10);

                    textItem.RenderPosition = new Vector2(-40, 51);

                    menuItems.Add(textItem);
                }

                menuItems.Add(new ImageMenuItem("update", ROOMS_ROOT_DIRECTORY));
                menuItems.Add(new ImageMenuItem("back", ROOMS_ROOT_DIRECTORY));

                menuItems[menuItems.Count - 2].Position = new Vector2(DEFAULT_MENUITEM_X - 100, 650);
                menuItems[menuItems.Count - 1].Position = new Vector2(DEFAULT_MENUITEM_X + 100, 650);

                pointer.Selection = menuItems.Count - 2;

                Controller.GetInstance().ReleaseUpdate();

            }
        }
    }
}
