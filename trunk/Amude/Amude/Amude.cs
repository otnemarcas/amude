using System;
using System.Configuration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Amude.Core;
using Amude.Screen.Core;
using System.Threading;
using System.Windows.Forms;

/* 
 FATEC - Carapicuíba

 Trabalho de conclusão de curso - ASTI Jogos

Gabriel Sacramento do Amaral - gabsacramento@gmail.com
Leandro Roque Razori - roque.razori@gmail.com
Renato Ibanhez - renato.ibanhez@gmail.com
Sergio Alberto Pasqualino - sergio.pasqualino@gmail.com

Amude é um software-livre; você pode redistribuí-lo e/ou modificá-lo sobre os termos da 
LGPL: GNU Lesser General Public License como publicado pela Free Software Foundation na 
versão 3 desta licença ou qualquer versão posterior.
    
Setembro/2010
 */

namespace Amude
{
    public class Amude : Game
    {
        const string NAME = "Amude";

        Controller controller;
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;

        public Amude()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Form frm = (Form)Form.FromHandle(this.Window.Handle);
            frm.FormClosing += new FormClosingEventHandler(WindowClosingHandler);
        }

        public void ApplyGraphicChanges()
        {
            graphics.PreferredBackBufferWidth = ResolutionManager.GetPreferredScreenWidth();
            graphics.PreferredBackBufferHeight = ResolutionManager.GetPreferredScreenHeight();
            graphics.IsFullScreen = ResolutionManager.IsFullScreen();
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            Window.AllowUserResizing = false;
            this.Window.Title = NAME;
            controller = Controller.GetInstance();
            controller.Initialize(this);
            ResolutionManager.Initialize();
            ApplyGraphicChanges();
            base.Initialize();
            AudioManager.Initialize();
            ScreenManager.GetInstance().AddScreen(new Screen.MainMenuScreen());
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent() { }

        protected override void Update(GameTime gameTime)
        {
            controller.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront,
                SaveStateMode.None, ResolutionManager.GetGlobalTransformation());

            controller.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void WindowClosingHandler(object sender, FormClosingEventArgs e)
        {
            Controller.GetInstance().SupressUpdate();

            Controller.GetInstance().QuitGame(true, false);
            Controller.GetInstance().Exit();

            Controller.GetInstance().ReleaseUpdate();
        }
    }
}
