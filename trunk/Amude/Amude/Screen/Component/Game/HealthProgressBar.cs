using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Domain.Attribute;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Amude.Screen.Component.Game
{
    internal class HealthProgressBar : ProgressBar
    {
        private Health health;

        public HealthProgressBar(Health health, int width, int height, Vector2 position)
            : base(health.MaxValue, width, height, position)
        {
            if (health == null)
                throw new ArgumentNullException();
            this.health = health;
        }

        public HealthProgressBar(Health health, int width, int height, Vector2 position,
                           float layerDepthBackground, float layerDepthValue)
            : base(health.MaxValue, width, height, position, layerDepthBackground, layerDepthValue)
        {
            this.health = health;
        }

        public Health Health
        {
            get
            {
                return health;
            }
        }

        public override void Update(float passedTime)
        {
            base.MaxValue = health.MaxValue;
            base.Value = health.Value;
            base.Update(passedTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
