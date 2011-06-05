using Microsoft.Xna.Framework;

namespace Amude.Motion
{
    internal class StaticMovement : Movement
    {
        public StaticMovement(Vector2 origin)
            : base(origin, origin, 0)
        {
        }

        // Override em Update para performance
        public override void Update(float passedTime) { }

        public override bool IsFinalized()
        {
            return false;
        }
    }
}
