using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Screen.Core;
using Amude.Motion;
using Amude.Core;
using Amude.Global;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Amude.Screen
{
    internal class ScriptScreen : AbstractScreen
    {
        private const string SCRIPTSCREEN_ROOT_DIRECTORY = MENU_ROOT_DIRECTORY + "ScriptScreen/";
        
        private const int END_TEXT_Y_POSITION = 0;
        private const int START_TEXT_Y_POSITION = 760;
        private const int TEXT_X_POSITION = 540;
        private const int TEXT_WIDTH = 635;
        private const float LINE_GAP_TIME = 2f;
        private const float TEXT_SPEED = 14;

        private const int CONTAINER_X = 430;
        private const int CONTAINER_BORDER_X = 429;
        private const int CONTAINER_BOTTOM_Y = 730;

        private SpriteFont scriptFont;
        private Texture2D containerTexture;
        private Texture2D containerTopBorderTexture;
        private Texture2D containerBottomBorderTexture;
        private Vector2 containerPosition;
        private Vector2 containerTopBorderPosition;
        private Vector2 containerBottomBorderPosition;
        private List<Movement> lineMotions;
        private List<String> textLines;
        private float lineGapBuffer;
        private List<int> movingLinesIndex;

        public ScriptScreen()
        {
            containerTexture = IO.LoadSingleTexture(SCRIPTSCREEN_ROOT_DIRECTORY + "container");
            containerTopBorderTexture = IO.LoadSingleTexture(SCRIPTSCREEN_ROOT_DIRECTORY + "container_top_border");
            containerBottomBorderTexture = IO.LoadSingleTexture(SCRIPTSCREEN_ROOT_DIRECTORY + "container_bottom_border");
            containerPosition = new Vector2(CONTAINER_X, 0);
            containerTopBorderPosition = new Vector2(CONTAINER_BORDER_X, 0);
            containerBottomBorderPosition = new Vector2(CONTAINER_BORDER_X, CONTAINER_BOTTOM_Y);

            scriptFont = IO.LoadFont("Data/Global/ScriptFont");
            String scriptContent = Bundle.Texts["script"];
            textLines = SharedFunctions.LayoutString(scriptContent, TEXT_WIDTH, scriptFont);
        }

        public override void LoadContent()
        {
            lineMotions = new List<Movement>();
            movingLinesIndex = new List<int>();
            foreach (String textLine in textLines)
            {
                Movement lineMotion = MovementProvider.GetDirectMovement(
                    new Vector2(TEXT_X_POSITION, START_TEXT_Y_POSITION),
                    new Vector2(TEXT_X_POSITION, END_TEXT_Y_POSITION), TEXT_SPEED);
                lineMotions.Add(lineMotion);
            }

            lineGapBuffer = LINE_GAP_TIME;
        }

        public override void UnloadContent() { }

        public override void Update(float passedTime)
        {
            lineGapBuffer += passedTime;

            if (KeyboardManager.Manager.TypedKeys.Count > 0)
            {
                ScreenManager.GetInstance().RemoveLastScreen();
                ScreenManager.GetInstance().AddScreen(new NewGameScreen());
                AudioManager.PlaySound("menu_accept");
            }

            if (lineGapBuffer >= LINE_GAP_TIME)
            {
                Movement nextLine = lineMotions.FirstOrDefault(q => q.Status == MovementStatus.StandBy);
                if (nextLine != null)
                {
                    nextLine.Start();
                }
                lineGapBuffer -= LINE_GAP_TIME;
            }

            foreach (Movement lineMotion in lineMotions)
            {
                lineMotion.Update(passedTime);
            }

            bool finished = true;
            foreach (Movement lineMotion in lineMotions)
            {
                if (lineMotion.Status != MovementStatus.Finished)
                {
                    finished = false;
                }
            }

            if (finished)
            {
                ScreenManager.GetInstance().RemoveLastScreen();
                ScreenManager.GetInstance().AddScreen(new NewGameScreen());
                AudioManager.PlaySound("menu_accept");
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(containerTexture, containerPosition, null, Color.White, 0,
                Vector2.Zero, 1, SpriteEffects.None, Constants.LD_BACKLAYER_1);

            spriteBatch.Draw(containerTopBorderTexture, containerTopBorderPosition, null,
                Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_FRONTLAYER_0);

            spriteBatch.Draw(containerBottomBorderTexture, containerBottomBorderPosition, null,
                Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_FRONTLAYER_0);

            movingLinesIndex.Clear();

            for (int i = 0; i < lineMotions.Count; i++)
            {
                if (lineMotions[i].Status == MovementStatus.Executing)
                {
                    movingLinesIndex.Add(i);
                }
            }

            foreach (int index in movingLinesIndex)
            {
                spriteBatch.DrawString(scriptFont, textLines[index], lineMotions[index].Position, Color.Black, 
                    0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INTERFACE_1);
            }

            base.Draw(spriteBatch);
        }
    }
}
