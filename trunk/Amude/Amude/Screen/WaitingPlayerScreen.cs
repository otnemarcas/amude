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
using System.Text;
using Amude.Core;
using Amude.Global;
using Amude.Graphics;
using Amude.Network.Messages;
using Amude.Screen.Core;
using Amude.Screen.Core.MenuItem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Amude.Screen
{
    internal class WaitingPlayerScreen : AbstractScreen
    {
        public static string WAITINGPLAYER_ROOT_DIRECTORY = MENU_ROOT_DIRECTORY + "WaitingPlayerScreen/";

        private Texture2D containerTexture;
        private Vector2 containerPosition;
        private float passedTime;

        private SpriteFont spriteFont;
        private StringBuilder actualProcess;
        private Vector2 actualProcessPosition;

        private List<IMenuItem> menuItems;
        private Pointer pointer;

        private Animation searchIcon;

        public WaitingPlayerScreen()
        {
            containerTexture = IO.LoadSingleTexture(WAITINGPLAYER_ROOT_DIRECTORY + "container");
            containerPosition = new Vector2(315, 250);

            spriteFont = IO.LoadFont("Data/Global/AmudeFont");
            actualProcess = new StringBuilder("Aguardando adversário");
            actualProcessPosition = new Vector2(450, 360);

            menuItems = new List<IMenuItem>();
            ImageMenuItem cancelButton = new ImageMenuItem("cancel", WAITINGPLAYER_ROOT_DIRECTORY);
            cancelButton.Position = new Vector2(512, 500);
            menuItems.Add(cancelButton);

            searchIcon = new Animation(AnimationType.StaticRight, 1f, 
                IO.LoadSprite(WAITINGPLAYER_ROOT_DIRECTORY + "searching", 24));
            searchIcon.ColorModifier = new Color(255, 0, 0);
            searchIcon.IsCyclic = true;
            searchIcon.Position = new Vector2(400, 403);
            searchIcon.Start();

            pointer = new Pointer(menuItems);
            passedTime = 0;
        }

        public override void LoadContent() 
        {
            pointer.Selection = 0;
        }

        public override void UnloadContent() 
        {
            
        }

        public override void Update(float passedTime)
        {
            this.passedTime += passedTime;

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Escape) || 
                KeyboardManager.Manager.TypedKeys.Contains(Keys.Enter))
            {
                Controller.GetInstance().StopServer();
                ScreenManager.GetInstance().RemoveLastScreen();
                AudioManager.PlaySound("menu_accept");
            }

            if (this.passedTime > 0.7f && !actualProcess.ToString().EndsWith("."))
            {
                actualProcess.Append(".");
            }
            else if (this.passedTime > 1.4f && !actualProcess.ToString().EndsWith(".."))
            {
                actualProcess.Append(".");
            }
            else if (this.passedTime > 2.1f && !actualProcess.ToString().EndsWith("..."))
            {
                actualProcess.Append(".");
            }

            if (this.passedTime > 2.8f)
            {
                this.passedTime -= 2.8f;
                actualProcess.Remove(actualProcess.Length - 3, 3);
            }

            searchIcon.Update(passedTime);

            pointer.Update(passedTime);

            foreach (IMenuItem item in menuItems)
            {
                item.Update(passedTime);
            }
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(containerTexture, containerPosition, null, Color.White, 0,
                            Vector2.Zero, 1, SpriteEffects.None, Constants.LD_BACKLAYER_1);

            spriteBatch.DrawString(spriteFont, actualProcess, actualProcessPosition, Color.Black,
                0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);

            pointer.Draw(spriteBatch);

            searchIcon.Draw(spriteBatch);

            foreach (IMenuItem item in menuItems)
            {
                item.Draw(spriteBatch);
            }

            base.Draw(spriteBatch);
        }
    }
}