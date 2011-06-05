using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Domain;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Amude.Global;
using Amude.Graphics;
using System.Windows.Forms;

namespace Amude.Core
{
    internal class MapEngine : IDisposable
    {
        private PathFinder pathFinder;
        private Map currentMap;
        private Entity watchedObject;
        private Point watchedObjectLocation;
        private List<Entity> hiddenEntities;

        #region Singleton

        private static MapEngine instance;

        private MapEngine(Map map)
        {
            currentMap = map;
            hiddenEntities = new List<Entity>();
            pathFinder = new PathFinder(map);
        }

        public static MapEngine GetInstance()
        {
            if (instance == null)
            {
                throw new Exception("MapEngine não iniciada.");
            }
            return instance;
        }

        public static void Initialize(Map map)
        {
            if (instance != null)
            {
                instance.Dispose();
                instance = null;
                GC.Collect();
            }

            instance = new MapEngine(map);
        }

        #endregion

        public Map Map
        {
            get
            {
                return currentMap;
            }
        }

        public void Move(Entity entity, Point location)
        {
            Point actualLocation = entity.MapLocation;
            AnimationType animationType = AnimationType.StaticRight;
            Int32 topY = actualLocation.Y;
            
            if (currentMap.Objects[actualLocation.X, actualLocation.Y] != entity)
            {
                throw new Exception("Perda de referência da entidade com o mapa.");
            }
            if (currentMap.Objects[location.X, location.Y] != null)
            {
                Entity obstacle = currentMap.Objects[location.X, location.Y];
                if (obstacle is Character && ((Character)obstacle).Health.IsDead)
                {
                    hiddenEntities.Add(obstacle);
                    currentMap.Objects[location.X, location.Y] = null;
                }
                else
                {
                    throw new Exception("Destino ocupado por outra entidade.");
                }
            }

            List<Point> route = pathFinder.GetRoute(entity.MapLocation, location);
            Point lastPosition = entity.MapLocation;

            foreach (Point point in route)
            {
                if (point.Y > topY)
                {
                    topY = point.Y;
                }

                if (point.X > lastPosition.X)
                {
                    animationType = AnimationType.Right;
                }
                else if (point.X < lastPosition.X)
                {
                    animationType = AnimationType.Left;
                }
                else if (point.Y > lastPosition.Y)
                {
                    animationType = AnimationType.Down;
                }
                else if (point.Y < lastPosition.Y)
                {
                    animationType = AnimationType.Up;
                }
                
                if(animationType != AnimationType.StaticRight)
                {
                    entity.AddAnimatedMovement(animationType, lastPosition, point, entity.DefaultSpeed);
                }
                lastPosition = point;
            }

            entity.LayerDepth = Camera.GetInstance().GetCharacterLayerDepth(new Point(5, topY));

            Watch(entity, location);
        }

        public List<Point> GetAvaiablePositions(Character character)
        {
            return pathFinder.AvaliablePositions(character.MapLocation, character.Agility);
        }

        public List<Point> GetAttackPositions(Character character)
        {
            return pathFinder.GetAttackPositions(character.MapLocation, character.Attack.Range);
        }

        public void Update()
        {
            if (watchedObject == null)
            {
                return;
            }

            if (watchedObject.IsStopped())
            {
                Point oldPosition = watchedObject.MapLocation;
                Entity hiddenEntity = hiddenEntities.FirstOrDefault(q => q.MapLocation == oldPosition);
                
                if (hiddenEntity != null)
                {
                    Map.Objects[oldPosition.X, oldPosition.Y] = hiddenEntity;
                    hiddenEntities.Remove(hiddenEntity);
                }
                else
                {
                    Map.Objects[oldPosition.X, oldPosition.Y] = null;
                }

                Map.Objects[watchedObjectLocation.X, watchedObjectLocation.Y] = watchedObject;
                watchedObject.MapLocation = watchedObjectLocation;

                Watch(null, Point.Zero);
            }
        }

        private void Watch(Entity entity, Point destiny)
        {
            watchedObject = entity;
            watchedObjectLocation = destiny;
        }

        public void SetObject(Entity entity)
        {
            if (!Map.SetObject(entity))
            {
                throw new Exception("Destino ocupado por outra entidade.");
            }
        }

        public void RealocateObject(Entity entity, Point location)
        {
            if (currentMap.Objects[entity.MapLocation.X, entity.MapLocation.Y] != entity)
            {
                throw new Exception("Perda de referência da entidade com o mapa.");
            }
            if (currentMap.Objects[location.X, location.Y] != null)
            {
                throw new Exception("Destino ocupado por outra entidade.");
            }
            Map.Objects[entity.MapLocation.X, entity.MapLocation.Y] = null;
            entity.MapLocation = location;
            Map.Objects[entity.MapLocation.X, entity.MapLocation.Y] = entity;
        }

        #region IDisposable Members

        public void Dispose()
        {
            watchedObject = null;
            if (currentMap != null)
            {
                currentMap.Dispose();
            }
            
            currentMap = null;
        }

        #endregion
    }
}
