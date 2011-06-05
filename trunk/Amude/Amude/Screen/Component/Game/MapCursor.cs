using Amude.Core;
using Amude.Domain;
using Amude.Global;
using Amude.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Amude.Screen.Component.Game
{
    internal class MapCursor : Animation
    {
        private const string MAP_CURSOR_PATH = "Image/Animation/Interface/MapCursor/mapcursor-";
        private const int CURSOR_GAP_Y = 120;
        private const int CURSOR_GAP_X = 40;

        private Map map;
        private Point position;

        private Character character;
        private Texture2D detailsBackground;
        private Color detailsColor;
        private Color detailsBackgroudColor;
        private Vector2 detailsPosition;
        private SpriteFont detailsFont;
        private HealthProgressBar progressBar;
        private Vector2 progressBarPosition;
        private Vector2 scale;

        public MapCursor()
            : base(AnimationType.StaticRight)
        {
            character = null;
            map = MapEngine.GetInstance().Map;
            detailsBackground = IO.LoadSingleTexture("Image/Global/pixel");
            detailsFont = IO.LoadFont("Data/Global/MapCursorFont");
            detailsBackgroudColor = Color.White;
            detailsBackgroudColor.A = 130;
            
            base.Sprites = IO.LoadSprite(MAP_CURSOR_PATH, 4);
            base.Duration = 0.9f;
            base.LayerDepth = Constants.LD_INTERFACE_0;
            if (Controller.GetInstance().IsServer)
            {
                base.ColorModifier = Constants.SERVER_COLOR;
            }
            else
            {
                base.ColorModifier = Constants.CLIENT_COLOR;
            }

            base.IsCyclic = true;
            base.IsVisible = false;
            MapLocation = new Point(0,1);
            base.Start();
        }

        public Point MapLocation
        {
            get
            {
                return position;
            }
            set
            {
                if (value.X < 0)
                {
                    value.X = 0;
                }
                else if (value.X >= map.MapWidth)
                {
                    value.X = map.MapWidth - 1;
                }

                if (value.Y < 1)
                {
                    value.Y = 1;
                }
                else if (value.Y >= map.MapHeight)
                {
                    value.Y = map.MapHeight - 1;
                }

                this.position = value;
            }
        }

        public override void Update(float passedTime)
        {
            Vector2 actualPosition = Camera.GetInstance().ToPixels(position);
            actualPosition.Y -= CURSOR_GAP_Y;
            actualPosition.X += CURSOR_GAP_X;
            base.Position = actualPosition;
            base.Update(passedTime);


            Entity entity = map.Objects[position.X, position.Y];
            if (entity is Character)
            {
                character = (Character)entity;
                scale = detailsFont.MeasureString(character.Name);

                if (character.Owner.Name == Controller.GetInstance().GetLocalPlayer().Name)
                {
                    if (Controller.GetInstance().IsServer)
                    {
                        detailsColor = Color.Red;
                    }
                    else
                    {
                        detailsColor = Color.Blue;
                    }
                }
                else                 
                {
                    if (Controller.GetInstance().IsServer)
                    {
                        detailsColor = Color.Blue; 
                    }
                    else
                    {
                        detailsColor = Color.Red;
                    }
                }

                if (position.X < map.MapWidth - 1)
                {
                    detailsPosition = new Vector2(actualPosition.X + 20, actualPosition.Y);
                    progressBarPosition = new Vector2(detailsPosition.X + 20, detailsPosition.Y + 25);
                }
                else
                {
                    detailsPosition = new Vector2(actualPosition.X - (scale.X + 40), actualPosition.Y);
                    progressBarPosition = new Vector2(detailsPosition.X + 20, detailsPosition.Y + 25);
                }

                if ((progressBar == null) || progressBar.Health != character.Health)
                {
                    progressBar = new HealthProgressBar(character.Health, (int)detailsFont.MeasureString(character.Name).X,
                    10, progressBarPosition, Constants.LD_MAPINFO_1, Constants.LD_MAPINFO_2);
                }
                else
                {
                    progressBar.Position = progressBarPosition;
                }

                progressBar.Update(passedTime);
            }
            else
            {
                character = null;
            }

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (character != null && IsVisible)
            {
                string details = character.Name;

                spriteBatch.Draw(detailsBackground, detailsPosition, null, detailsBackgroudColor, 0, 
                    Vector2.Zero, new Vector2(scale.X + 40, 40), SpriteEffects.None, Constants.LD_MAPINFO_0);

                spriteBatch.DrawString(detailsFont, details, 
                    new Vector2(detailsPosition.X + 20, detailsPosition.Y + 5), detailsColor, 0,
                    Vector2.Zero, 1, SpriteEffects.None, Constants.LD_MAPINFO_1);

                progressBar.Draw(spriteBatch);
            }

            base.Draw(spriteBatch);
        }
        
    }
}
