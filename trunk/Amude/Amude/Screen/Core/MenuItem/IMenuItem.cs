using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Amude.Screen.Core.MenuItem
{
    internal interface IMenuItem
    {
        bool Selected { get; set; }
        Vector2 Position { get; set; }

        void Update(float passedTime);
        void Draw(SpriteBatch spriteBatch);
    }
}
