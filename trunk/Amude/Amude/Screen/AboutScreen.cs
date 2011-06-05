using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Screen.Core;
using Amude.Screen.Core.MenuItem;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Amude.Core;
using Microsoft.Xna.Framework.Input;

namespace Amude.Screen
{
    internal class AboutScreen : AbstractScreen
    {
        protected static string ABOUT_ROOT_DIRECTORY = MENU_ROOT_DIRECTORY + "AboutScreen/";
        protected static string ABOUT_CONTAINER_PATH = ABOUT_ROOT_DIRECTORY + "AboutScreen";

        private Texture2D containerTexture;
        private Vector2 containerPosition;

        public AboutScreen()
        {
            containerTexture = IO.LoadSingleTexture(ABOUT_CONTAINER_PATH);
            containerPosition = new Vector2(320,80);
        }

        public override void LoadContent(){}

        public override void UnloadContent(){}

        public override void Update(float passedTime)
        {
            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Enter) || KeyboardManager.Manager.TypedKeys.Contains(Keys.Escape))
            {
                ScreenManager.GetInstance().RemoveLastScreen();
                AudioManager.PlaySound("menu_accept");
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.Draw(containerTexture, containerPosition, Color.White);
        }
    }
}
