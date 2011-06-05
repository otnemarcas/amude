using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Amude.Core;
using Amude.Global;

namespace Amude.Motion
{
    internal static class MovementProvider
    {
        public static Movement GetStaticMovement(){
            return new StaticMovement(Vector2.Zero);
        }

        public static Movement GetStaticMovement(Vector2 origin)
        {
            return new StaticMovement(origin);
        }

        public static Movement GetStaticMovement(Point cell)
        {
            return new StaticMovement(Camera.GetInstance().ToPixels(cell));
        }

        public static Movement GetDirectMovement(Vector2 origin, Vector2 destiny, float speed)
        {
            return new Movement(origin, destiny, speed);
        }

        public static Movement GetMapMovement(Point endCell, float speed)
        {
            return new Movement(Vector2.Zero, Camera.GetInstance().ToPixels(endCell), speed);
        }

        public static Movement GetMapMovement(Point startCell, Point endCell, float speed)
        {
            return new Movement(Camera.GetInstance().ToPixels(startCell), Camera.GetInstance().ToPixels(endCell), speed);
        }
    }
}
