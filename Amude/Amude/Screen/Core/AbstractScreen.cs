/* 
 FATEC - Carapicuíba

 Trabalho de conclusão de curso - ASTI Jogos

 Gabriel Sacramento do Amaral
 Leandro Roque Razori
 Renato Ibanhez
 Sergio Alberto Pasqualino
 
 Setembro/2010
 */
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Amude.Core;
using Amude.Global;

namespace Amude.Screen.Core
{
    internal abstract class AbstractScreen
    {
        public const string MENU_ROOT_DIRECTORY = "Image/Menu/";
        public const string LOGO_PATH = MENU_ROOT_DIRECTORY + "logo";
        public const string BACKGROUND_PATH = MENU_ROOT_DIRECTORY + "background";
        public const string CONTAINER_PATH = MENU_ROOT_DIRECTORY + "container";
        public const string CHARACTER_PROFILES_PATH = "Image/Global/Profile/";

        protected static Color CONTAINER_COLOR = new Color((byte)255, (byte)244, (byte)83, (byte)200);
        protected static Vector2 LOGO_POSITION = new Vector2(186,12);
        
        private Texture2D background;
        private static Texture2D logoTexture;


        public AbstractScreen()
        {
            background = IO.LoadSingleTexture(BACKGROUND_PATH);
            logoTexture = IO.LoadSingleTexture(LOGO_PATH);
        }

        public abstract void LoadContent();

        public abstract void UnloadContent();

        public abstract void Update(float passedTime);

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, Vector2.Zero, null, Color.White, 0,
                Vector2.Zero, 1, SpriteEffects.None, Constants.LD_BACKGROUND);
            spriteBatch.Draw(logoTexture, LOGO_POSITION, null, Color.White, 0,
                Vector2.Zero, 1, SpriteEffects.None, Constants.LD_BACKLAYER_0);
        }
    }
}
