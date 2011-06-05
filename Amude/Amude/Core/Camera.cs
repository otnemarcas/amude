using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Domain;
using Microsoft.Xna.Framework;
using Amude.Global;
using Microsoft.Xna.Framework.Graphics;
using Amude.Motion;

namespace Amude.Core
{
    internal class Camera : IDisposable
    {
        public const int START_X = 12;
        public const int START_Y = 12;
        public const int CAMERA_WIDTH = 10;
        public const int CAMERA_HEIGHT = 8;
        public const float CAMERA_SPEED = 900;

        private Map map;
        private Point cameraSize;
        private Point cameraLocation;
        private volatile bool updateLocations;

        private bool moving;
        private bool blocked;
        private bool watching;

        private Movement softMove;
        private Point softMoveStartLocation;
        private Point softMoveDisplacement;

        private List<Character> watchedCharacters;

        #region Singleton

        private static Camera instance;

        private Camera(Map map)
        {
            watchedCharacters = new List<Character>();
            this.map = map;
            cameraSize = new Point(CAMERA_WIDTH, CAMERA_HEIGHT);
            cameraLocation = Point.Zero;
            moving = false;
            blocked = false;
            watching = false;
        }

        public static Camera GetInstance()
        {
            if (instance == null)
            {
                throw new Exception("Camera não iniciada.");
            }
            return instance;
        }

        public static void Initialize(Map map)
        {
            if (instance != null)
            {
                instance.Dispose();
                instance = null;
            }

            instance = new Camera(map);
            instance.UpdateObjectsLocation();
            instance.moving = false;
        }

        #endregion

        public bool UpdateLocations
        {
            get
            {
                return updateLocations;
            }
            set
            {
                updateLocations = value;
            }
        }

        public Point Location
        {
            get
            {
                return cameraLocation;
            }
        }

        public Point CenterLocation
        {
            get
            {
                return new Point(cameraLocation.X + (CAMERA_WIDTH / 2), cameraLocation.Y + (CAMERA_HEIGHT / 2));
            }
        }

        public bool IsMoving
        {
            get
            {
                return moving;
            }
        }

        public bool IsBlocked
        {
            get
            {
                return blocked;
            }
        }

        public void Update(float passedTime)
        {
            if (moving)
            {
                cameraLocation = softMoveStartLocation;
                softMove.Update(passedTime);
                cameraLocation.X += (int)Math.Floor(softMove.Position.X / Constants.TILE_SIZE);
                cameraLocation.Y += (int)Math.Floor(softMove.Position.Y / Constants.TILE_SIZE);
                updateLocations = true;

                if (softMove.IsFinalized())
                {
                    moving = false;
                    blocked = false;
                }
            }

            if (updateLocations)
            {
                UpdateObjectsLocation();
            }

            if (watchedCharacters.Count != 0)
            {
                if (watching)
                {
                    bool isIdle = true;
                    foreach (Character character in watchedCharacters)
                    {
                        if(!character.IsIdle() || character.HealthVariation.IsVisible() 
                            || character.DrawingAffects)
                        {
                            isIdle = false;
                            break;
                        }
                    }

                    if (isIdle)
                    {
                        watchedCharacters.Clear();
                        watching = false;
                        blocked = false;
                    }
                }
                else if(watchedCharacters.Count > 0 && watchedCharacters[0].IsStopped())
                {
                    SoftMove(watchedCharacters[0].MapLocation);
                    watchedCharacters.Clear();
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int x = cameraLocation.X - 1; x < cameraLocation.X + cameraSize.X && x < map.MapWidth; x++)
            {
                for (int y = cameraLocation.Y - 1; y <= cameraLocation.Y + cameraSize.Y && y < map.Height; y++)
                {
                    if (x < 0 || y < 0)
                    {
                        continue;
                    }
                    map.Terrain[x, y].Draw(spriteBatch);
                    if (map.Objects[x, y] != null)
                    {
                        map.Objects[x, y].Draw(spriteBatch);
                    }
                }
            }
        }

        public Vector2 ToPixels(Point mapPosition)
        {
            Vector2 softPositionGap = Vector2.Zero;
            if (moving)
            {
                softPositionGap.X = softMove.Position.X % Constants.TILE_SIZE;
                softPositionGap.Y = softMove.Position.Y % Constants.TILE_SIZE;

                if (cameraLocation.X < softMoveStartLocation.X &&
                    cameraLocation.X == softMoveStartLocation.X + softMoveDisplacement.X)
                {
                    softPositionGap.X = 0;
                }
                if (cameraLocation.Y < softMoveStartLocation.Y &&
                    cameraLocation.Y == softMoveStartLocation.Y + softMoveDisplacement.Y)
                {
                    softPositionGap.Y = 0;
                }
            }

            return new Vector2((mapPosition.X - cameraLocation.X) * Constants.TILE_SIZE + START_X - softPositionGap.X,
                (mapPosition.Y - cameraLocation.Y) * Constants.TILE_SIZE + START_Y + Constants.TILE_SIZE - softPositionGap.Y);
        }

        private void UpdateObjectsLocation()
        {
            for (int x = cameraLocation.X - 1; x < cameraLocation.X + cameraSize.X && x < map.MapWidth; x++)
            {
                for (int y = cameraLocation.Y - 1; y <= cameraLocation.Y + cameraSize.Y && y < map.Height; y++)
                {
                    if (x < 0 || y < 0)
                    {
                        continue;
                    }
                    map.Terrain[x, y].Position = ToPixels(map.Terrain[x, y].MapLocation);
                    if (map.Objects[x, y] != null)
                    {
                        map.Objects[x, y].LayerDepth = GetCharacterLayerDepth(map.Objects[x, y].MapLocation);
                        map.Objects[x, y].Origin = ToPixels(map.Objects[x, y].MapLocation);
                    }
                }
            }

            updateLocations = false;
        }

        public void SetLocation(Point cameraLocation)
        {
            if (!IsBlocked)
            {
                if (cameraLocation.X < 0)
                {
                    cameraLocation.X = 0;
                }
                if (cameraLocation.Y < 0)
                {
                    cameraLocation.Y = 0;
                }
                if (cameraLocation.X + cameraSize.X > map.Width)
                {
                    cameraLocation.X = map.Width - cameraSize.X;
                }
                if (cameraLocation.Y + cameraSize.Y > map.Height)
                {
                    cameraLocation.Y = map.Height - cameraSize.Y;
                }

                this.cameraLocation = cameraLocation;
                UpdateObjectsLocation();
            }
        }

        public void SoftMove(Point destiny)
        {
            softMoveStartLocation = cameraLocation;
            destiny.X -= cameraSize.X / 2;
            destiny.Y -= cameraSize.Y / 2;

            if (destiny.X < 0)
            {
                destiny.X = 0;
            }
            if (destiny.Y < 0)
            {
                destiny.Y = 0;
            }
            if (destiny.X + cameraSize.X > map.Width)
            {
                destiny.X = map.Width - cameraSize.X;
            }
            if (destiny.Y + cameraSize.Y > map.Height)
            {
                destiny.Y = map.Height - cameraSize.Y;
            }

            destiny.X -= cameraLocation.X;
            destiny.Y -= cameraLocation.Y;

            softMoveDisplacement = new Point(destiny.X, destiny.Y);

            destiny.X *= Constants.TILE_SIZE;
            destiny.Y *= Constants.TILE_SIZE;


            softMove = MovementProvider.GetDirectMovement(Vector2.Zero,
                new Vector2(destiny.X, destiny.Y), CAMERA_SPEED);
            softMove.Start();
            moving = true;
            updateLocations = true;
        }

        public void Follow(Character character)
        {
            watchedCharacters.Clear();
            watchedCharacters.Add(character);
            blocked = true;
        }

        public void Watch(Character character)
        {
            watchedCharacters.Add(character);
            blocked = true;
            watching = true;
        }

        public void FocusOn(Point target)
        {
            if (!IsBlocked)
            {
                target.X -= cameraSize.X / 2;
                target.Y -= cameraSize.Y / 2;
                SetLocation(target);
            }
        }

        public float GetCharacterLayerDepth(Point point)
        {
            return SharedFunctions.GetCharacterLayerDepth(point.X - cameraLocation.X + 1,
                                                          point.Y - cameraLocation.Y + 1);        
        }


        #region IDisposable Members

        public void Dispose()
        {
            softMove = null;
            watchedCharacters = null;
        }

        #endregion
    }
}
