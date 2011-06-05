using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Motion;
using Amude.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Amude.Domain
{
    internal class AnimatedMovement : IDisposable
    {

        protected Animation animation;
        protected Movement movement;
        protected AnimatedMovement child;

        public Animation Animation
        {
            get
            {
                return animation;
            }
            set
            {
                animation = value;
            }
        }

        public Movement Movement
        {
            get
            {
                return movement;
            }
            set
            {
                movement = value;
            }
        }

        public Vector2 Origin
        {
            get
            {
                return Movement.Origin;
            }
            set
            {
                Movement.Origin = value;
                Animation.Position = value;
                if (child != null)
                {
                    child.Movement.Origin = value;
                    child.Animation.Position = value;
                }
            }
        }

        public Vector2 Destiny
        {
            get
            {
                return Movement.Destiny;
            }
            set
            {
                Movement.Destiny = value;
                if (child != null)
                {
                    child.Destiny = value;
                }
            }
        }

        public virtual bool IsVisible
        {
            get
            {
                return Animation.IsVisible;
            }
            set
            {
                Animation.IsVisible = value;
            }
        }

        public AnimatedMovement Child
        {
            get
            {
                return child;
            }
        }

        public AnimatedMovement(Animation animation, Movement movement)
        {
            this.animation = animation;
            movement.Redefine();

            if (animation.HasChild)
            {
                switch (animation.MovementBehavior)
                {
                    case MovementBehavior.ActAndDelegate:
                        child = new AnimatedMovement(animation.Child.Clone(), movement.Clone());
                        this.movement = movement;
                        break;
                    case MovementBehavior.Act:
                        child = new AnimatedMovement(animation.Child.Clone(),
                        MovementProvider.GetStaticMovement(movement.Origin));
                        this.movement = movement;
                        break;
                    case MovementBehavior.Delegate:
                        child = new AnimatedMovement(animation.Child.Clone(), movement.Clone());
                        this.movement = MovementProvider.GetStaticMovement(movement.Origin);
                        break;
                }
            }
            else
            {
                this.movement = movement;
            }

            Origin = movement.Origin;
        }

        public bool IsFinalized()
        {
            if (child == null)
            {
                 return animation.IsFinalized() || movement.IsFinalized();
            }
            return child.IsFinalized() && (animation.IsFinalized() || movement.IsFinalized());
        }

        public bool IsReady
        {
            get
            {
                if (child == null)
                {
                    return Animation.IsReady;
                }
                else
                {
                    return child.IsReady;
                }
            }
        }

        public void Start()
        {
            animation.Redefine();
            movement.Redefine();
            animation.Start();
            movement.Start();
        }

        public virtual void Update(float passedTime)
        {
            if (child != null)
            {
                if (child.IsFinalized())
                {
                    child.Finish();
                }

                if(animation.IsFinalized() || movement.IsFinalized())
                {
                    Finish();
                }

                if (child.Animation.Status == AnimationStatus.StandBy && 
                    (animation.IsFinalized() || Animation.IsReady || movement.IsFinalized()))
                {
                    child.Movement.Origin = Movement.Destiny;
                    child.Start();
                }

                child.Update(passedTime);
            }

            movement.Update(passedTime);
            animation.Position = movement.Position;
            animation.Update(passedTime);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            animation.Draw(spriteBatch);
            if (child != null)
            {
                child.Draw(spriteBatch);
            }
        }

        public void Finish()
        {
            animation.Finish();
            movement.Finish();
        }

        #region IDisposable Members

        public void Dispose()
        {
            animation.Dispose();
            animation = null;
            movement = null;
            if (child != null)
            {
                child.Dispose();
                child = null;
            }
        }

        #endregion
    }
}
