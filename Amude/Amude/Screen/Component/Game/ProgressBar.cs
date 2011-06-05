using System;
using Amude.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Amude.Global;

namespace Amude.Screen.Component
{
    internal class ProgressBar
    {
        private int width;
        private int height;
        private Vector2 backgroundScale;
        private Vector2 valueScale;
        private Texture2D pixel;
        private Color actualColor;
        private float maxValue;
        private float value;

        public ProgressBar(float maxValue, int width, int height, Vector2 position)
        {
            this.value = 0;
            this.maxValue = maxValue;
            this.width = width;
            this.height = height;
            this.Position = position;
            this.pixel = IO.LoadSingleTexture("Image/Global/pixel");
            backgroundScale = new Vector2(width, height);
            valueScale = new Vector2(0, height);
            ValueColor = Color.Green;
            DangerColor = Color.Red;
            BackgroundColor = Color.Black;
            LayerDepthBackGround = Constants.LD_INTERFACE_4;
            LayerDepthValue = Constants.LD_INTERFACE_5;
        }

        public ProgressBar(float maxValue, int width, int height, Vector2 position, 
                           float layerDepthBackground, float layerDepthValue)
            : this(maxValue, width, height, position)
        {
            LayerDepthBackGround = layerDepthBackground;
            LayerDepthValue = layerDepthValue;
        }

        public Vector2 Position { get; set; }
        public Color BackgroundColor { get; set; }
        public Color ValueColor { get; set; }
        public Color DangerColor { get; set; }
        public float LayerDepthBackGround { get; set; }
        public float LayerDepthValue { get; set; }
        
        public float MaxValue 
        {
            get
            {
                return maxValue;
            }
            set
            {
                if (value < 0)
                    value = 0;

                maxValue = value;
                if (this.value > value)
                {
                    this.value = value;
                }
            }
        }
        
        public float Value
        {
            get
            {
                return value;
            }
            set
            {
                if (value <= MaxValue)
                    this.value = value;

                if (value < 0)
                    this.value = 0;
            }
        }

        public virtual void Update(float passedTime)
        {
            if (value <= (MaxValue / 3.0f))
            {
                actualColor = DangerColor;
            }
            else
            {
                actualColor = ValueColor;
            }

            valueScale.X = ((float)value / (float)maxValue) * (float)width;

        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(pixel, Position, null, BackgroundColor, 0, Vector2.Zero,
                backgroundScale, SpriteEffects.None, LayerDepthBackGround);

            spriteBatch.Draw(pixel, Position, null, actualColor, 0, Vector2.Zero, 
                valueScale, SpriteEffects.None, LayerDepthValue);   
        }

    }
}
