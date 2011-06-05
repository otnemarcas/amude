using System.Collections.Generic;
using Amude.Graphics;
using Amude.Motion;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Amude.Core;
using System;
using Amude.Global;
using System.Threading;

namespace Amude.Domain
{
    internal enum DefaultDirection
    {
        Right,
        Left
    }

    internal class Entity : ICloneable<Entity>, IDisposable
    {
        protected float layerDepth;
        protected Color colorModifier;
        protected Dictionary<AnimationType, Animation> definedAnimations;
        protected volatile Queue<AnimatedMovement> currentActions;

        public Entity()
        {
            definedAnimations = new Dictionary<AnimationType, Animation>();
            currentActions = new Queue<AnimatedMovement>();
            colorModifier = Color.White;
            DefaultDirection = DefaultDirection.Right;
            MapLocation = Point.Zero;
            LayerDepth = 0f;
        }

        public String RootName { get; set; }

        public String Name { get; set; }

        public float DefaultSpeed { get; set; }

        public float LayerDepth
        {
            get
            {
                return layerDepth;
            }
            set
            {
                layerDepth = value;
                foreach (AnimatedMovement action in currentActions)
                {
                    action.Animation.LayerDepth = value;
                }
            }
        }

        public Point MapLocation { get; set; }

        public Color ColorModifier 
        {
            get { return colorModifier; }
            set { colorModifier = value; }
        }

        public DefaultDirection DefaultDirection { get; set; }

        public Vector2 Position
        {
            get
            {
                return currentActions.Peek().Movement.Position;
            }
        }

        public Vector2 Origin
        {
            get
            {
                return currentActions.Peek().Origin;
            }
            set
            {
                currentActions.Peek().Origin = value;
            }
        }

        public Vector2 Destiny
        {
            get
            {
                return currentActions.Peek().Destiny;
            }
            set
            {
                currentActions.Peek().Destiny = value;
            }
        }

        public bool IsVisible
        {
            get
            {
                return currentActions.Peek().IsVisible;
            }
            set
            {
                currentActions.Peek().IsVisible = value;
            }
        }

        public virtual void Update(float passedTime)
        {
            AnimatedMovement action = currentActions.Peek();
            if (action.IsFinalized())
            {
                RemoveActualAnimatedMovement();
            }
            action.Update(passedTime);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            currentActions.Peek().Animation.ColorModifier = colorModifier;
            currentActions.Peek().Draw(spriteBatch);
        }

        public void DefineAnimatedMovement(Dictionary<AnimationType, Animation> definedAnimations)
        {
            foreach (KeyValuePair<AnimationType,Animation> kv in definedAnimations)
            {
                Animation animation = definedAnimations[kv.Key].Clone();
                animation.LayerDepth = LayerDepth;
                animation.ColorModifier = ColorModifier;
                this.definedAnimations.Add(kv.Key, animation);
            }
        }

        public void DefineAnimatedMovement(AnimationType key, Animation value)
        {
            Animation animation = value.Clone();
            animation.LayerDepth = LayerDepth;
            animation.ColorModifier = ColorModifier;
            definedAnimations.Add(key, animation);
        }

        public void AddAnimatedMovement(AnimationType key)
        {
            Animation nextAnimation = definedAnimations[key].Clone();
            nextAnimation.LayerDepth = LayerDepth;
            nextAnimation.ColorModifier = ColorModifier;

            AnimatedMovement nextAction = new AnimatedMovement(nextAnimation,
                MovementProvider.GetStaticMovement());
            
            AddNextAction(nextAction);
        }

        public void AddAnimatedMovement(AnimationType key, Vector2 origin)
        {
            Animation nextAnimation = definedAnimations[key].Clone();
            nextAnimation.LayerDepth = LayerDepth;
            nextAnimation.ColorModifier = ColorModifier;

            AnimatedMovement nextAction = new AnimatedMovement(nextAnimation,
                MovementProvider.GetStaticMovement(origin));
            
            AddNextAction(nextAction);
        }

        public void AddAnimatedMovement(AnimationType key, Vector2 origin, Vector2 destiny, float speed)
        {
            Animation nextAnimation = definedAnimations[key].Clone();
            nextAnimation.LayerDepth = LayerDepth;
            nextAnimation.ColorModifier = ColorModifier;

            AnimatedMovement nextAction = new AnimatedMovement(nextAnimation,
                MovementProvider.GetDirectMovement(origin, destiny, speed));

            AddNextAction(nextAction);
        }

        public void AddAnimatedMovement(AnimationType key, Point cell)
        {
            Animation nextAnimation = definedAnimations[key].Clone();
            nextAnimation.LayerDepth = LayerDepth;
            nextAnimation.ColorModifier = ColorModifier;

            AnimatedMovement nextAction = new AnimatedMovement(nextAnimation,
                MovementProvider.GetStaticMovement(cell));

            AddNextAction(nextAction);
        }

        public void AddAnimatedMovement(AnimationType key, Point endCell, float speed)
        {
            Animation nextAnimation = definedAnimations[key].Clone();
            nextAnimation.LayerDepth = LayerDepth;
            nextAnimation.ColorModifier = ColorModifier;
            
            AnimatedMovement nextAction = new AnimatedMovement(nextAnimation,
                MovementProvider.GetMapMovement(endCell, speed));

            AddNextAction(nextAction);
        }

        public void AddAnimatedMovement(AnimationType key, Point startCell, Point endCell, float speed)
        {
            Animation nextAnimation = definedAnimations[key].Clone();
            nextAnimation.LayerDepth = LayerDepth;
            nextAnimation.ColorModifier = ColorModifier;

            AnimatedMovement nextAction = new AnimatedMovement(nextAnimation,
                MovementProvider.GetMapMovement(startCell, endCell, speed));

            AddNextAction(nextAction);
        }

        public void RemoveActualAnimatedMovement()
        {
            if (currentActions.Count > 0)
            {
                
                AnimatedMovement action = currentActions.Dequeue();
                if (currentActions.Count == 0)
                {
                    if (DefaultDirection == DefaultDirection.Right)
                    {
                        AddAnimatedMovement(AnimationType.StaticRight, action.Animation.Position);
                    }
                    else
                    {
                        AddAnimatedMovement(AnimationType.StaticLeft, action.Animation.Position);
                    }
                }

                AnimatedMovement nextAction = currentActions.Peek();
                nextAction.Origin = action.Movement.Position;
                action.Finish();
                nextAction.Start();
            }
        }

        public void ClearAnimatedMovements()
        {
            currentActions.Clear();
        }

        public bool IsIdle()
        {
            if (currentActions.Count == 1)
            {
                AnimationType animationType = currentActions.Peek().Animation.Type;
                if (animationType == AnimationType.StaticRight ||
                    animationType == AnimationType.StaticLeft ||
                    animationType == AnimationType.DeadRight ||
                    animationType == AnimationType.DeadLeft)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual bool IsStopped()
        {
            return currentActions.Count == 1 && (currentActions.Peek().Movement is StaticMovement
                || currentActions.Peek().Movement.IsFinalized());
        }

        public virtual bool IsReady()
        {
            return currentActions.Count > 0 && currentActions.Peek().IsReady;
        }

        public Animation GetDefinedAnimation(AnimationType animationType)
        {
            return definedAnimations[animationType].Clone();
        }

        private void AddNextAction(AnimatedMovement nextAction)
        {            
            if (currentActions.Count == 0)
            {
                nextAction.Start();
            }
            if (IsIdle())
            {
                nextAction.Origin = currentActions.Dequeue().Movement.Position;
                nextAction.Start();
            }
            currentActions.Enqueue(nextAction);
        }

        #region ICloneable<Entity> Members

        public Entity Clone()
        {
            Entity clone = (Entity)MemberwiseClone();
            clone.currentActions = new Queue<AnimatedMovement>();
            return clone;
        }

        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {
            while (currentActions.Count > 0)
            {
                AnimatedMovement action = currentActions.Dequeue();
                action.Dispose();
            }
        }

        #endregion
    }
}