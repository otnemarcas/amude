using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Screen.Core;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Amude.Core;
using Microsoft.Xna.Framework.Input;

namespace Amude.Screen
{
    internal class HelpScreen : AbstractScreen
    {
        private const int screenCount = 5;
        private static string HELPSCREEN_ROOT_DIRECTORY = MENU_ROOT_DIRECTORY + "HelpScreen/";
        //private static string HELPSCREEN_CONTAINER_PATH = HELPSCREEN_ROOT_DIRECTORY + "HelpScreen";


        private List<Texture2D> containerTexture;
        private Vector2 containerPosition;
        int index;
        


        public HelpScreen()
        {
            containerTexture = new List<Texture2D>();
            for (int count = 1; count <= screenCount; count++)
            {
                containerTexture.Add(IO.LoadSingleTexture(HELPSCREEN_ROOT_DIRECTORY + "helpscreen-" + count.ToString("00")));
            }
            containerPosition = new Vector2(90, 40);
        }


        public override void LoadContent()
        {
            index = 0;
        }

        public override void UnloadContent(){}

        public override void Update(float passedTime)
        {
            if(KeyboardManager.Manager.TypedKeys.Contains(Keys.Right)){
                if (index < screenCount - 1)
                {
                    AudioManager.PlaySound("menu_select");
                    index++;
                }
            }
            if(KeyboardManager.Manager.TypedKeys.Contains(Keys.Left)){
                if (index > 0)
                {
                    AudioManager.PlaySound("menu_select");
                    index--;
                }
            }
            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Escape))
            {
                AudioManager.PlaySound("menu_accept");
                ScreenManager.GetInstance().RemoveLastScreen();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            spriteBatch.Draw(containerTexture[index], containerPosition, Color.White);
        }
    }


}
