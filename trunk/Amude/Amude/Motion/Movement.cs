using Microsoft.Xna.Framework;
using System;
using Amude.Core;
using Amude.Global;

namespace Amude.Motion
{
    internal class Movement : ICloneable<Movement>
    {
        private MovementStatus movementStatus;
        private Vector2 origin;
        private Vector2 destiny;
        private float rawSpeed; //px/s
        private Vector2 speed; //px/s por eixo
        private Vector2 position;

        public Movement(Vector2 origin, Vector2 destiny, float speed)
        {
            movementStatus = MovementStatus.StandBy;
            this.origin = origin;
            this.position = origin;
            this.destiny = destiny;
            this.rawSpeed = speed;
            Redefine();
        }

        public Vector2 Origin
        {
            get { return origin; }
            set 
            {
                origin = value;
                position = value;
                Redefine();
            }
        }

        public Vector2 Destiny
        {
            get { return destiny; }
            set
            {
                destiny = value;
                Redefine();
            }
        }

        public float Speed
        {
            get { return rawSpeed; }
            set
            {
                rawSpeed = value;
                Redefine();
            }
        }

        public Vector2 Position
        {
            get { return position; }
        }

        public MovementStatus Status
        {
            get { return movementStatus; }
        }

        public void Redefine()
        {
            speed = new Vector2();
            if (origin != destiny)
            {
                float totalDisplacement = Vector2.Distance(origin, destiny);
                float duration = totalDisplacement / rawSpeed;
                speed.X = (destiny.X - origin.X) / duration;
                speed.Y = (destiny.Y - origin.Y) / duration;
            }
            else
            {
                speed.X = 0;
                speed.Y = 0;
            }
        }

        public void Start()
        {
            movementStatus = MovementStatus.Executing;
        }

        public virtual void Update(float passedTime)
        {
            float gapX, gapY;
            Vector2 error;

            if (movementStatus != MovementStatus.Executing)
                return;

            position += speed * passedTime;
            
            gapX = Math.Abs(position.X - destiny.X);
            gapY = Math.Abs(position.Y - destiny.Y);
            error =  speed / 60;

            if (gapX >= 0 && gapX <= Math.Abs(error.X) && 
                gapY >= 0 && gapY <= Math.Abs(error.Y))
            {
                position = destiny;
                Finish();
            }
        }

        public virtual bool IsFinalized()
        {
            return movementStatus == MovementStatus.Finished;
        }

        public void Finish()
        {
            movementStatus = MovementStatus.Finished;
        }

        #region ICloneable<Movement> Members

        // @SupressWarning
        public Movement Clone()
        {
            return (Movement)this.MemberwiseClone();
        }

        #endregion
    }
}
