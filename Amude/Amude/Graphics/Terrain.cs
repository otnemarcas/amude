using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Amude.Global;

namespace Amude.Graphics
{
    internal class Terrain : ICloneable<Terrain>, IDisposable
    {
        private Vector2 renderPosition;

        public Terrain(Texture2D texture)
        {
            this.Texture = texture;
            ColorModifier = Color.White;
            MapLocation = Point.Zero;
            Position = Vector2.Zero;
            renderPosition = Vector2.Zero;
            LayerDepth = Constants.LD_TERRAIN;
        }

        public Texture2D Texture { get; set; }

        public String RootName { get; set; }

        public Color ColorModifier { get; set; }

        public Point MapLocation { get; set; }

        public Vector2 Position { get; set; }

        public float LayerDepth { get; set; }

        public void Draw(SpriteBatch spriteBatch)
        {
            renderPosition.Y = Texture.Height;
            spriteBatch.Draw(Texture, Position, null, ColorModifier, 0, renderPosition, 1,
                SpriteEffects.None, 1f);
        }

        #region ICloneable<Terrain> Members

        public Terrain Clone()
        {
            return (Terrain)this.MemberwiseClone();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.Texture = null;
            RootName = null;
        }

        #endregion
    }
}
