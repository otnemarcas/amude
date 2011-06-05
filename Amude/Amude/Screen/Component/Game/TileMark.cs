using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Graphics;
using Amude.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Amude.Global;

namespace Amude.Screen.Component.Game
{
    internal class TileMark
    {
        private const string BLANK_PATH = "Image/Global/pixel";
        
        private static Color MARK_COLOR = new Color(0, 250, 0, 100);
        private static Color SELECTED_MARK_COLOR = new Color(255, 255, 0, 150);

        private Texture2D blankTexture;
        private Vector2 position;
        private Rectangle tileMarkBounds;

        public TileMark(Point mapLocation)
        {
            blankTexture = IO.LoadSingleTexture(BLANK_PATH);
            position = Vector2.Zero;
            tileMarkBounds = new Rectangle(0, 0, Constants.TILE_SIZE, Constants.TILE_SIZE);
            Selected = false;
            MapLocation = mapLocation;
        }

        public Point MapLocation { get; set; }

        public bool Selected { get; set; }

        public void Update()
        {
            position = Camera.GetInstance().ToPixels(MapLocation);
            position.Y -= Constants.TILE_SIZE;
            tileMarkBounds.X = (int)Math.Round(position.X);
            tileMarkBounds.Y = (int)Math.Round(position.Y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Selected)
            {
                spriteBatch.Draw(blankTexture, tileMarkBounds, null, SELECTED_MARK_COLOR,
                0, Vector2.Zero, SpriteEffects.None, Constants.LD_TERRAIN_TOP0);
            }
            else
            {
                spriteBatch.Draw(blankTexture, tileMarkBounds, null, MARK_COLOR, 
                    0, Vector2.Zero, SpriteEffects.None, Constants.LD_TERRAIN_TOP0);
            }
        }
    }
}
