using System;
using Amude.Core;
using Amude.Screen.Core;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Amude.Global;
using System.Collections.Generic;
using Amude.Screen.Core.MenuItem;

namespace Amude.Screen
{
    internal class ErrorScreen : AbstractScreen
    {
        private const String ERROR_SCREEN_ROOT = "Image/Menu/ErrorScreen/";
        private Exception exception;
        
        private Texture2D container;
        private Vector2 containerPosition;
        
        private SpriteFont spriteFont;
        private Vector2 titlePosition;
        private Color titleColor;
        private Vector2 descriptionPosition;
        private Color descriptionColor;

        private String title;
        private List<String> description;
        private List<IMenuItem> menuItems;
        private Pointer pointer;

        private ErrorScreen()
        {
            this.container = IO.LoadSingleTexture(ERROR_SCREEN_ROOT + "container");
            this.containerPosition = new Vector2(100, 50);
            this.spriteFont = IO.LoadFont("Data/Global/AmudeFont");

            this.titleColor = new Color(227, 4, 4);
            this.titlePosition = new Vector2(200, 150);

            this.descriptionColor = Color.Black;
            this.descriptionPosition = new Vector2(200, 250);

            this.menuItems = new List<IMenuItem>();
            this.menuItems.Add(new ImageMenuItem("ok", ERROR_SCREEN_ROOT));
            this.menuItems[0].Position = new Vector2(500, 650);
            this.menuItems[0].Selected = true;

            this.pointer = new Pointer(menuItems);
        }

        public ErrorScreen(Exception exception)
            : this()
        {
            this.exception = exception;
            this.title = "Ocorreu um erro";
            this.description = SharedFunctions.LayoutString(exception.Message, 50);
        }

        public ErrorScreen(String title, String description)
            : this()
        {
            this.title = title;
            this.description = SharedFunctions.LayoutString(description, 50);
        }

        public override void LoadContent() { }

        public override void UnloadContent() { }

        public override void Update(float passedTime)
        {
            foreach (IMenuItem menuItem in menuItems)
            {
                menuItem.Update(passedTime);
            }

            pointer.Update(passedTime);

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Escape) ||
                KeyboardManager.Manager.TypedKeys.Contains(Keys.Enter))
            {
                ScreenManager.GetInstance().RemoveLastScreen(true);
            }            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            foreach (IMenuItem menuItem in menuItems)
            {
                menuItem.Draw(spriteBatch);
            }

            pointer.Draw(spriteBatch);

            spriteBatch.Draw(container, containerPosition, null, Color.White, 0,
                             Vector2.Zero, 1, SpriteEffects.None, Constants.LD_BACKLAYER_1);

            spriteBatch.DrawString(spriteFont, title, titlePosition, titleColor, 0,
                                   Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);

            for (int i = 0; i < description.Count; i++)
            {
                Vector2 pos = descriptionPosition;
                pos.Y += (i * 40);

                spriteBatch.DrawString(spriteFont, description[i], pos, descriptionColor, 0,
                                       Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);                
            }

        }

    }
}
