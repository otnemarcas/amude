using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Amude.Domain;

namespace Amude.Screen.Core.MenuItem
{
    internal class EntityMenuItem : IMenuItem
    {
        protected Entity entity;

        public EntityMenuItem(Entity entity)
        {
            this.entity = entity;
        }

        #region IMenuItem Members

        public bool Selected { get; set; }

        public Vector2 Position
        {
            get
            {
                return entity.Position;
            }
            set
            {
                entity.Origin = value;
            }
        }

        public void Update(float passedTime)
        {
            entity.Update(passedTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            entity.Draw(spriteBatch);
        }

        #endregion
    }
}
