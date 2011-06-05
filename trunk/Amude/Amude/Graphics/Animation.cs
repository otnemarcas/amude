/* 
 FATEC - Carapicuíba

 Trabalho de conclusão de curso - ASTI Jogos

 Gabriel Sacramento do Amaral
 Leandro Roque Razori
 Renato Ibanhez
 Sergio Alberto Pasqualino
 
 Setembro/2010
 */
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Amude.Core;
using Amude.Motion;
using Microsoft.Xna.Framework.Audio;
using Amude.Global;

namespace Amude.Graphics
{
    internal enum AnimationType
    {
        StaticRight,
        StaticLeft,
        NoneRight,
        NoneLeft,
        Up,
        Down,
        Left,
        Right,
        AttackUp,
        AttackDown,
        AttackLeft,
        AttackRight,
        HitRight,
        HitLeft,
        DieRight,
        DieLeft,
        DeadRight,
        DeadLeft
    }

    internal enum MovementBehavior
    {
        Act,
        Delegate,
        ActAndDelegate
    }

    internal enum OnFinalize
    {
        Hide,
        DoNothing
    }

    internal class Animation : ICloneable<Animation>, IDisposable
    {
        public const int INFINITY = -1;

        private AnimationType animationType;
        private AnimationStatus animationStatus;
        private List<Texture2D> sprites;
        private Vector2 renderPosition;
        private int currentSprite;
        private float timeSlice;
        private float duration;
        private float totalTime;

        private float rotation;

        private String soundName;
        private Boolean stopSoundOnFinalize;
        private Cue sound;

        private Animation child;
        private Boolean hasChild;
        private Boolean isVisible;

        public Animation(AnimationType animationType)
        {
            IsCyclic = false;
            animationStatus = AnimationStatus.StandBy;
            MovementBehavior = MovementBehavior.ActAndDelegate;
            OnFinalize = OnFinalize.DoNothing;
            this.animationType = animationType;
            Ready = INFINITY;
            ColorModifier = Color.White;
            isVisible = true;
            LayerDepth = 0.8f;
            FlipHorizontally = false;
            Rotation = 0;
            renderPosition = Vector2.Zero;
            hasChild = false;
        }

        public Animation(AnimationType animationType, List<Texture2D> sprites)
            : this(animationType)
        {
            this.duration = 0;
            this.sprites = sprites;
            Redefine();
        }

        public Animation(AnimationType animationType, float duration, List<Texture2D> sprites)
            : this(animationType)
        {
            this.duration = duration;
            this.sprites = sprites;
            Redefine();
        }

        public bool IsCyclic { get; set; }
        public Vector2 Position { get; set; }
        public AnimationStatus Status
        {
            get { return animationStatus; }
        }

        public AnimationType Type
        {
            get
            {
                return animationType;
            }
        }

        public float Duration
        {
            get
            {
                return duration;
            }
            set
            {
                duration = value;
                Redefine();
            }
        }

        public List<Texture2D> Sprites
        {
            get
            {
                return sprites;
            }
            set
            {
                this.sprites = value;
                Redefine();
            }
        }

        public int Ready { get; set; }

        public bool IsReady
        {
            get
            {
                return Ready != INFINITY && currentSprite >= Ready;
            }
        }

        public Color ColorModifier { get; set; }

        public float LayerDepth { get; set; }

        public virtual bool IsVisible 
        {
            get { return isVisible; }
            set { isVisible = value; }
        }

        public bool FlipHorizontally { get; set; }

        public float Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                if (value >= 360 || value <= -360)
                {
                    value %= 360;
                }

                rotation = value;
            }
        }

        public MovementBehavior MovementBehavior { get; set; }

        public OnFinalize OnFinalize { get; set; }

        public Animation Child
        {
            get
            {
                return child;
            }
        }

        public Boolean HasChild
        {
            get
            {
                return hasChild;
            }
        }

        public void Redefine()
        {
            totalTime = 0;
            timeSlice = duration / sprites.Count;
            currentSprite = 0;
        }

        public virtual void Start()
        {
            animationStatus = AnimationStatus.Executing;
            if (!string.IsNullOrEmpty(soundName) && sound == null)
            {
                sound = AudioManager.PlaySound(soundName);
            }
        }

        public bool IsFinalized()
        {
            return (animationStatus == AnimationStatus.Finished);
        }

        public virtual void Update(float passedTime)
        {
            if (animationStatus == AnimationStatus.StandBy ||
                IsFinalized() || duration == INFINITY)
                return;

            lock (this)
            {
                totalTime += passedTime;

                if (totalTime >= timeSlice)
                {
                    currentSprite++;
                    totalTime -= timeSlice;

                    if (currentSprite >= sprites.Count)
                    {
                        if (IsCyclic)
                        {
                            currentSprite = 0;
                        }
                        else
                        {
                            currentSprite = sprites.Count - 1;
                            Finish();
                        }
                    }
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (IsVisible && animationStatus != AnimationStatus.StandBy)
            {
                renderPosition.Y = sprites[currentSprite].Height;

                if (FlipHorizontally)
                {
                    spriteBatch.Draw(sprites[currentSprite], Position, null, ColorModifier,
                    MathHelper.ToRadians(Rotation), renderPosition, 1, SpriteEffects.FlipHorizontally, LayerDepth);
                }
                else
                {
                    spriteBatch.Draw(sprites[currentSprite], Position, null, ColorModifier,
                    MathHelper.ToRadians(Rotation), renderPosition, 1, SpriteEffects.None, LayerDepth);
                }
            }
        }

        public Animation Clone()
        {
            return (Animation)this.MemberwiseClone();
        }

        public void SetChild(Animation child)
        {
            if (child == null)
            {
                throw new ArgumentNullException();
            }
            this.child = child;
            hasChild = true;
        }

        public void SetSound(string soundName, Boolean stopOnFinalize)
        {
            this.soundName = soundName;
            this.stopSoundOnFinalize = stopOnFinalize;
        }

        public void Finish()
        {
            if (sound != null && stopSoundOnFinalize &&
                (!sound.IsStopped || !sound.IsStopping))
            {
                sound.Stop(AudioStopOptions.AsAuthored);
            }
            animationStatus = AnimationStatus.Finished;
            
            if (OnFinalize == OnFinalize.Hide)
            {
                IsVisible = false;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            soundName = null;

            if (sound != null)
            {
                sound.Stop(AudioStopOptions.Immediate);
                sound.Dispose();
                sound = null;
            }

            if (child != null)
            {
                child.Dispose();
                child = null;
            }
        }

        #endregion
    }
}
