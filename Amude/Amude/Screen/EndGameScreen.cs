using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Amude.Core;
using Amude.Domain;
using Amude.Global;
using Amude.Screen.Component.Game;
using Amude.Screen.Core;
using Amude.Screen.Core.MenuItem;

namespace Amude.Screen
{
    internal class EndGameScreen : AbstractScreen
    {
        private const String ENDGAME_SCREEN_ROOT = "Image/Menu/EndGameScreen/";
        private const int MESSAGE_WIDTH = 330;
        private const int MESSAGE_X = 470;
        private const int MESSAGE_Y = 100;
        private const int MESSAGE_Y_GAP = 30;
        private const int VICTORIUS_X = 250;
        private const int VICTORIUS_Y = 100;
        private const int VICTORIUS_NAME_X = 170;
        private const int DEFEAT_X = 885;
        private const int DEFEAT_Y = 100;
        private const int DEFEAT_NAME_X = 800;
        private const int NAME_WIDTH = 280;

        private bool localPlayerVictory;
        private List<String> message;
        private String winner;
        private String defeated;
        private Dictionary<Texture2D, HealthProgressBar> winners;
        private Texture2D diedTexture;
        private List<IMenuItem> menuItems;
        private Pointer pointer;

        private Texture2D container;
        private Vector2 containerPosition;

        private SpriteFont font;
        private Vector2 messagePosition;
        private Vector2 victoryPosition;
        private Vector2 victoryNamePosition;
        private Vector2 defeatPosition;
        private Vector2 defeatNamePosition;
        private Vector2 winnersPosition;

        public EndGameScreen(String winner)
        {
            winners = new Dictionary<Texture2D, HealthProgressBar>();
            Controller controller = Controller.GetInstance();
            Player winnerPlayer = controller.GetPlayer(winner);

            font = IO.LoadFont("Data/Global/EndGameFont");
            messagePosition = new Vector2(MESSAGE_X, MESSAGE_Y);
            victoryPosition = new Vector2(VICTORIUS_X, VICTORIUS_Y);
            defeatPosition = new Vector2(DEFEAT_X, DEFEAT_Y);
            winnersPosition = new Vector2(565, 220);

            menuItems = new List<IMenuItem>();
            menuItems.Add(new ImageMenuItem("ok", ENDGAME_SCREEN_ROOT));
            menuItems[0].Position = new Vector2(529, 730);
            menuItems[0].Selected = true;
            pointer = new Pointer(menuItems);

            container = IO.LoadSingleTexture(ENDGAME_SCREEN_ROOT + "container");
            containerPosition = new Vector2(100, 0);

            diedTexture = IO.LoadSingleTexture(ENDGAME_SCREEN_ROOT + "died");

            if (winner == controller.GetLocalPlayer().Name)
            {
                localPlayerVictory = true;
                message = SharedFunctions.LayoutString(Bundle.Texts["end_victory"], MESSAGE_WIDTH, font);
            }
            else
            {
                localPlayerVictory = false;
                message = SharedFunctions.LayoutString(Bundle.Texts["end_defeat"], MESSAGE_WIDTH, font);
            }

            int index = 0;
            Vector2 characterPosition = new Vector2(330, 330);
            foreach (Character character in winnerPlayer.GetCharacters().OrderByDescending(q => q.Health.Value))
            {
                winners.Add(IO.LoadSingleTexture(AbstractScreen.CHARACTER_PROFILES_PATH + character.RootName + "-profile"),
                    new HealthProgressBar(character.Health.Clone(), 300, 30, characterPosition));
                index++;
                characterPosition.X = 320 + (index / 3) * 430;
                characterPosition.Y = 330 + (index % 3) * 130;
            }

            this.winner = winner;
            victoryNamePosition = new Vector2(VICTORIUS_NAME_X + (NAME_WIDTH - font.MeasureString(winner).X)/2,
                VICTORIUS_Y + 30);

            defeated = controller.GetPlayers().First(q => q.Name != winner).Name;
            defeatNamePosition = new Vector2(DEFEAT_NAME_X + (NAME_WIDTH - font.MeasureString(defeated).X)/2, 
                DEFEAT_Y + 30);
        }

        override public void LoadContent()
        {
            if (localPlayerVictory)
                AudioManager.PlayMusic("victory", false);
            else
                AudioManager.PlayMusic("defeat", false);
        }

        override public void UnloadContent() { }

        override public void Update(float passedTime)
        {
            foreach (IMenuItem menuItem in menuItems)
            {
                menuItem.Update(passedTime);
            }

            foreach (HealthProgressBar health in winners.Values)
            {
                health.Update(passedTime);
            }

            pointer.Update(passedTime);

            if(KeyboardManager.Manager.TypedKeys.Contains(Keys.Enter) ||
                KeyboardManager.Manager.TypedKeys.Contains(Keys.Escape))
            {
                ScreenManager.GetInstance().RemoveLastScreen(true);
                AudioManager.PlaySound("menu_accept");
            }
        }

        override public void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            spriteBatch.Draw(container, containerPosition, null, Color.White, 0,
                             Vector2.Zero, 1, SpriteEffects.None, Constants.LD_BACKLAYER_1);

            messagePosition.Y = MESSAGE_Y;
            foreach (String messageLine in message)
            {
                float message_x = messagePosition.X;
                messagePosition.X += (MESSAGE_WIDTH - font.MeasureString(messageLine).X) / 2;
                spriteBatch.DrawString(font, messageLine, messagePosition, Color.Black, 0,
                    Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);

                messagePosition.X = message_x;
                messagePosition.Y += MESSAGE_Y_GAP;
            }

            spriteBatch.DrawString(font, "Vitorioso", victoryPosition, Color.Green, 0,
                    Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);

            spriteBatch.DrawString(font, winner, victoryNamePosition, Color.Black, 0,
                Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);

            spriteBatch.DrawString(font, "Derrotado", defeatPosition, Color.Red, 0,
                    Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);

            spriteBatch.DrawString(font, defeated, defeatNamePosition, Color.Black, 0,
                Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);

            spriteBatch.DrawString(font, "Vitoriosos", winnersPosition, Color.Green, 0,
                Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);

            foreach (KeyValuePair<Texture2D, HealthProgressBar> kv in winners)
            {
                Vector2 relativePosition = new Vector2(kv.Value.Position.X - 90, kv.Value.Position.Y - 65);
                spriteBatch.Draw(kv.Key, relativePosition, null, Color.White, 0, 
                    Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INFO);
                if (kv.Value.Health.IsDead)
                {
                    spriteBatch.Draw(diedTexture, relativePosition, null, Color.White,
                        0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_FRONTLAYER_0);
                }
                kv.Value.Draw(spriteBatch);
            }

            foreach (IMenuItem menuItem in menuItems)
            {
                menuItem.Draw(spriteBatch);
            }

            pointer.Draw(spriteBatch);
        }
    }
}
