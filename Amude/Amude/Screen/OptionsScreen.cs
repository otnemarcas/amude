using Amude.Core;
using Amude.Screen.Core;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Amude.Domain;
using Amude.Graphics;
using Amude.Screen.Core.MenuItem;
using Amude.Global;

namespace Amude.Screen
{
    class OptionsScreen : AbstractScreen
    {

        protected static string OPTIONS_ROOT_DIRECTORY = MENU_ROOT_DIRECTORY + "OptionsScreen/";
        protected static string OPTIONS_CONTAINER_PATH = OPTIONS_ROOT_DIRECTORY + "container";

        private List<IMenuItem> menuItems;
        private Texture2D containerTexture;
        private Texture2D videoTexture;
        private Texture2D audioTexture;
        private Vector2 containerPosition;
        private Pointer pointer;
        private Vector2 videoPosition;
        private Vector2 audioPosition;

        private int resolutionIndex = 0;

        private Dictionary<Point, Texture2D> resolutions;
        private List<Point> possibleResolutions;
        private Point actualResolution;
        private Vector2 resolutionPosition;

        private Dictionary<bool, Texture2D> fullScreen;
        private Vector2 fullScreenPosition;
        private bool actualFullScreenState;

        private Dictionary<int, Texture2D> effectsVolume;
        private Vector2 effectsVolumePosition;
        private int actualEffectsVolume;

        private Dictionary<int, Texture2D> musicVolume;
        private Vector2 musicVolumePosition;
        private int actualMusicVolume;

        public OptionsScreen()
        {
            containerTexture = IO.LoadSingleTexture(OPTIONS_CONTAINER_PATH);
            containerPosition = new Vector2(320, 100);
            videoPosition = new Vector2(350, 170);
            audioPosition = new Vector2(350, 350);
            
            musicVolume = new Dictionary<int,Texture2D>();
            musicVolumePosition = new Vector2(670, 400);

            effectsVolume = new Dictionary<int, Texture2D>();
            effectsVolumePosition = new Vector2(670, 460);

            resolutions = new Dictionary<Point, Texture2D>();
            possibleResolutions = new List<Point>();
            resolutionPosition = new Vector2(670, 220);

            fullScreen = new Dictionary<bool,Texture2D>();
            fullScreenPosition = new Vector2(670, 280);

            menuItems = new List<IMenuItem>();

            videoTexture = IO.LoadSingleTexture(OPTIONS_ROOT_DIRECTORY + "screen");
            audioTexture = IO.LoadSingleTexture(OPTIONS_ROOT_DIRECTORY + "audio");

            possibleResolutions.Add(new Point(800, 600));
            possibleResolutions.Add(new Point(1024, 768));
            possibleResolutions.Add(new Point(1280, 768));
            possibleResolutions.Add(new Point(1280, 800));

            for (int i = 0; i < possibleResolutions.Count; i++)
            {
                resolutions.Add(possibleResolutions[i], IO.LoadSingleTexture(
                    OPTIONS_ROOT_DIRECTORY + "resolution0" + (i+1)));
            }

            fullScreen.Add(true, IO.LoadSingleTexture(OPTIONS_ROOT_DIRECTORY + "yes"));
            fullScreen.Add(false, IO.LoadSingleTexture(OPTIONS_ROOT_DIRECTORY + "no"));

            for(int i = 0; i <= 10; i ++)
                musicVolume.Add(i, IO.LoadSingleTexture(OPTIONS_ROOT_DIRECTORY + "volume" + i));
            
            for (int i = 0; i <= 10; i ++)
                effectsVolume.Add(i, IO.LoadSingleTexture(OPTIONS_ROOT_DIRECTORY + "volume" + i));

            menuItems.Add(new ImageMenuItem("resolution", OPTIONS_ROOT_DIRECTORY));
            menuItems.Add(new ImageMenuItem("fullscreen", OPTIONS_ROOT_DIRECTORY));
            menuItems.Add(new ImageMenuItem("music", OPTIONS_ROOT_DIRECTORY));
            menuItems.Add(new ImageMenuItem("effects", OPTIONS_ROOT_DIRECTORY));
            menuItems.Add(new ImageMenuItem("apply", OPTIONS_ROOT_DIRECTORY));
            menuItems.Add(new ImageMenuItem("cancel", OPTIONS_ROOT_DIRECTORY));

            menuItems[0].Position = new Vector2(430, 260);
            menuItems[1].Position = new Vector2(430, 320);

            menuItems[2].Position = new Vector2(430, 440);
            menuItems[3].Position = new Vector2(430, 500);

            menuItems[4].Position = new Vector2(382, 580);
            menuItems[5].Position = new Vector2(630, 580);

            actualResolution = ResolutionManager.GetPreferredResolution();

            actualFullScreenState = ResolutionManager.IsFullScreen();

            actualMusicVolume = (int)(AudioManager.MusicVolume*10);

            actualEffectsVolume = (int)(AudioManager.EffectsVolume*10);
            
            for(int i = 0; i < possibleResolutions.Count; i++)
            {
                if (actualResolution == possibleResolutions[i])
                    resolutionIndex = i;
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

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Up) && pointer.Selection < 5)
            {
                pointer.Selection--;
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Down) && pointer.Selection < 4)
            {
                pointer.Selection++;
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Right))
            {
                // Aumentar a resolução
                if (menuItems[0].Selected && resolutionIndex < possibleResolutions.Count - 1)
                {
                    AudioManager.PlaySound("menu_select");
                    resolutionIndex++;
                    actualResolution = possibleResolutions[resolutionIndex];
                }

                // FullScreen on/off
                else if (menuItems[1].Selected)
                {
                    AudioManager.PlaySound("menu_select");
                    actualFullScreenState = !actualFullScreenState;
                }

                // Aumentar o volume da musica
                else if (menuItems[2].Selected && actualMusicVolume < 10)
                {
                    AudioManager.PlaySound("menu_select");
                    actualMusicVolume++;
                    AudioManager.MusicVolume = actualMusicVolume / 10f;
                }

                // Aumentar o volume dos efeitos
                else if (menuItems[3].Selected && actualEffectsVolume < 10)
                {
                    AudioManager.PlaySound("menu_select");
                    actualEffectsVolume++;
                    AudioManager.EffectsVolume = actualEffectsVolume / 10f;
                }
                else if (pointer.Selection == 4)
                {
                    pointer.Selection++;
                }
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Left))
            {
                // Diminuir a resolução
                if (menuItems[0].Selected && resolutionIndex > 0)
                {
                    AudioManager.PlaySound("menu_select");
                    resolutionIndex--;
                    actualResolution = possibleResolutions[resolutionIndex];
                }
                
                // FullScreen on/off
                else if (menuItems[1].Selected)
                {
                    AudioManager.PlaySound("menu_select");
                    actualFullScreenState = !actualFullScreenState;
                }

                // Diminuir o volume da musica
                else if (menuItems[2].Selected && actualMusicVolume >= 1)
                {
                    AudioManager.PlaySound("menu_select");
                    actualMusicVolume--;
                    AudioManager.MusicVolume = actualMusicVolume / 10f;
                }

                // Diminuir o volume dos efeitos
                else if (menuItems[3].Selected && actualEffectsVolume >= 1)
                {
                    AudioManager.PlaySound("menu_select");
                    actualEffectsVolume --;
                    AudioManager.EffectsVolume = actualEffectsVolume / 10f;
                }

                else if (pointer.Selection == 5)
                {
                    pointer.Selection--;
                }
            }

            if ((KeyboardManager.Manager.TypedKeys.Contains(Keys.Enter))) 
            {
                if (menuItems[4].Selected)
                {
                    AudioManager.PlaySound("menu_accept");

                    ResolutionManager.SetPreferredResolution(actualResolution);
                    ResolutionManager.SetFullScreen(actualFullScreenState);
                    ResolutionManager.UpdateGraphics();

                    ResolutionManager.UpdateConfigFile();
                    AudioManager.UpdateConfigFile();

                    ScreenManager.GetInstance().RemoveLastScreen();
                    
                }

                else if (menuItems[5].Selected)
                {
                    AudioManager.PlaySound("menu_accept");
                    ScreenManager.GetInstance().RemoveLastScreen();
                }
            }

            if (KeyboardManager.Manager.TypedKeys.Contains(Keys.Escape))
            {
                ScreenManager.GetInstance().RemoveLastScreen();
                AudioManager.PlaySound("menu_accept");
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
            spriteBatch.Draw(containerTexture, containerPosition, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_BACKLAYER_1);
            spriteBatch.Draw(videoTexture, videoPosition, null, Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);
            spriteBatch.Draw(audioTexture, audioPosition, null, Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);
            spriteBatch.Draw(resolutions[actualResolution], resolutionPosition, null, Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);
            spriteBatch.Draw(fullScreen[actualFullScreenState], fullScreenPosition, null, Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);
            spriteBatch.Draw(musicVolume[actualMusicVolume], musicVolumePosition, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);
            spriteBatch.Draw(effectsVolume[actualEffectsVolume], effectsVolumePosition, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);
            
            foreach (IMenuItem menuItem in menuItems)
            {
                menuItem.Draw(spriteBatch);
            }
            
        }
    }
}
