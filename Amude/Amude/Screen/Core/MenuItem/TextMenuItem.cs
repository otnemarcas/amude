using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Amude.Global;
using Amude.Core;

namespace Amude.Screen.Core.MenuItem
{
    internal class TextMenuItem : IMenuItem
    {
        protected bool selected;
        protected int modifier = 0;
        protected bool increase;

        public TextMenuItem(string text)
        {
            SpriteFont = IO.LoadFont("Data/Global/AmudeFont");
            this.Text = text;
            Selected = false;
            Position = Vector2.Zero;
            RenderPosition = Vector2.Zero;
            ColorModifier = Color.Black;
        }

        public string Text { get; set; }
        public Color ColorModifier { get; set; }
        public Vector2 RenderPosition { get; set; }
        private SpriteFont SpriteFont { get; set; }

        #region IMenuItem Members

        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                modifier = 0;
                increase = true;
                if (selected)
                    ColorModifier = Color.Red;
                else
                    ColorModifier = Color.Black;
            }
        }

        public Vector2 Position { get; set; }

        public void Update(float passedTime)
        {
            if (Selected)
            {
                if (increase && modifier <= 250)
                {
                    modifier += 2;
                    if (modifier > 250)
                        increase = false;
                }
                else
                {
                    modifier -= 2;
                    if (modifier < 150)
                        increase = true;
                }

                Color color = ColorModifier;
                color.R = (byte)modifier;
                color.G = 0;
                color.B = 0;

                ColorModifier = color;
            }
            else
            {
                ColorModifier = Color.Black;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(SpriteFont, Text, Position, ColorModifier, 0,
                RenderPosition, 1, SpriteEffects.None, Constants.LD_INFO);
        }

        #endregion

    }
}
