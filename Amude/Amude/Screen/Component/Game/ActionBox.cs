using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Amude.Global;

namespace Amude.Screen.Component.Game
{
    internal sealed class ActionBox
    {
        private const int SCREEN_WIDTH = 984;
        private const int ACTIONBOX_BOTTOM = 763;
        private const int ACTIONBOX_BORDER = 25;
        private const int ACTIONBOX_MIN_WIDTH = 280;
        private const int ACTIONBOX_ITEMS_HEIGHT = 35;

        private SpriteFont actionFont;
        private Texture2D actionBoxTexture;

        private List<String> actionBoxOptions;
        private Dictionary<String, float> actionBoxItemsWidth;
        private Rectangle actionBoxBounds;
        private int actionBoxSelection;

        public ActionBox(SpriteFont actionFont, Texture2D actionBoxTexture)
        {
            this.actionFont = actionFont;
            this.actionBoxTexture = actionBoxTexture;
            actionBoxOptions = new List<String>();
            actionBoxItemsWidth = new Dictionary<String, float>();
            actionBoxBounds = new Rectangle();
            ActionBoxNonSelectedItemColor = new Color(156, 117, 70);
            ActionBoxSelectedItemColor = Color.White;
            actionBoxSelection = 0;
        }

        public Color ActionBoxNonSelectedItemColor { get; set; }

        public Color ActionBoxSelectedItemColor { get; set; }

        public int Selection
        {
            get
            {
                return actionBoxSelection;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                else if (value >= actionBoxOptions.Count && actionBoxOptions.Count != 0)
                {
                    value = actionBoxOptions.Count - 1;
                }

                actionBoxSelection = value;
            }
        }

        public bool IsVisible
        {
            get
            {
                return actionBoxOptions.Count != 0;
            }
        }

        public String SelectedValue
        {
            get
            {
                try
                {
                    return actionBoxOptions[actionBoxSelection];
                }
                catch
                {
                    return null;
                }
            }
        }

        public void AddOption(String option)
        {
            if (!actionBoxItemsWidth.ContainsKey(option))
            {
                InitializeOption(option);
            }

            actionBoxOptions.Add(option);
        }

        public void ClearOptions()
        {
            actionBoxOptions.Clear();
            this.Selection = 0;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (actionBoxOptions.Count == 0)
            {
                return;
            }
            
            float max_width = ACTIONBOX_MIN_WIDTH;
            foreach (string text in actionBoxOptions)
            {
                max_width = Math.Max(max_width, actionBoxItemsWidth[text]);
            }

            actionBoxBounds.Width = 2 * ACTIONBOX_BORDER + (int)Math.Round(max_width);
            actionBoxBounds.Height = 2 * ACTIONBOX_BORDER + actionBoxOptions.Count * ACTIONBOX_ITEMS_HEIGHT;
            actionBoxBounds.X = (int)Math.Round((SCREEN_WIDTH / 2f) - actionBoxBounds.Width / 2);
            actionBoxBounds.Y = ACTIONBOX_BOTTOM - actionBoxBounds.Height;

            spriteBatch.Draw(actionBoxTexture, actionBoxBounds, null, Color.White,
                0, Vector2.Zero, SpriteEffects.None, Constants.LD_INTERFACE_2);

            Vector2 itemPosition = new Vector2();
            for (int i = 0; i < actionBoxOptions.Count; i++)
            {
                itemPosition.X = actionBoxBounds.X + ACTIONBOX_BORDER + (max_width / 2) - (actionBoxItemsWidth[actionBoxOptions[i]] / 2f);
                itemPosition.Y = actionBoxBounds.Y + ACTIONBOX_BORDER + (i * ACTIONBOX_ITEMS_HEIGHT);

                if (i == actionBoxSelection)
                {
                    spriteBatch.DrawString(actionFont, actionBoxOptions[i], itemPosition, ActionBoxSelectedItemColor,
                        0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INTERFACE_3);
                }
                else
                {
                    spriteBatch.DrawString(actionFont, actionBoxOptions[i], itemPosition, ActionBoxNonSelectedItemColor,
                        0, Vector2.Zero, 1, SpriteEffects.None, Constants.LD_INTERFACE_3);
                }
            }
        }

        private void InitializeOption(String option)
        {
            actionBoxItemsWidth.Add(option, actionFont.MeasureString(option).X);
        }
    }
}
