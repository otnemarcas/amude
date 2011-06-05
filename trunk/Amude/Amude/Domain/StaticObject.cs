using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Amude.Domain.Graphics;

namespace Amude.Domain
{
    internal class StaticObject : Entity
    {
        override public void Update(float passedTime)
        {
            AnimatedMovement action = currentMovements.Peek();

            if (action.IsFinalized())
            {
                currentMovements.Dequeue();
                if (currentMovements.Count == 0)
                {
                    AddAnimatedMovement(AnimationType.Static);
                }
                currentMovements.Peek().Movement.Origin = action.Movement.Position;
                action = currentMovements.Peek();
                action.Animation.Start();
            }
            action.Movement.Update(passedTime);
            action.Animation.Position = action.Movement.Position;
            action.Animation.Update(passedTime);
        }

        override public void Draw(SpriteBatch spriteBatch)
        {
            currentMovements.Peek().Animation.Draw(spriteBatch);
        }
    }
}
