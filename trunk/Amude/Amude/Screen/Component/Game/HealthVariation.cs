using Amude.Core;
using Amude.Domain;
using Amude.Global;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Amude.Screen.Component.Game
{
    internal class HealthVariation
    {
        private const float DURATION = 2;
        private float totalTime;
        private bool executing;
        private float variation;
        private int y_gap;
        
        private Color color;
        private SpriteFont font;
        private Vector2 position;
        private Entity entity;

        public HealthVariation(Entity entity)
        {
            this.entity = entity;
            executing = false;
            variation = 0;
            totalTime = 0;            
            font = IO.LoadFont("Data/Global/HealthVariationFont");
        }

        public void Update(float passedTime)
        {
            position = entity.Position;
            position.X += Constants.TILE_SIZE / 2.0f;
            position.Y -= 120;
            

            if (executing)
            {
                totalTime += passedTime;
                
                if (color.A > 0)
                {
                    color.A -= 1;

                    if (y_gap < 40)
                    {
                        y_gap++;
                    }

                }

                if (totalTime > DURATION)
                {
                    executing = false;
                }

                position.Y -= y_gap;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            String value = variation < 0 ? "-" + Math.Abs(variation).ToString("0") :
                "+" + Math.Abs(variation).ToString("0");

            if (executing)
            {
                spriteBatch.DrawString(font, value, position, color, 0, Vector2.Zero, 1f,
                    SpriteEffects.None, Constants.LD_INTERFACE_1);
            }
        }

        public void ShowVariation(float variation)
        {
            this.y_gap = 0;
            this.executing = true;
            this.totalTime = 0;
            this.variation = variation;

            if (variation < 0)
            {
                color = Color.Red;
            }
            else
            {
                color = new Color(178, 255, 0);           
            }
        }

        public bool IsVisible()
        {
            return this.executing;
        }
    }
}
