using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Graphics;

namespace Amude.Domain
{
    internal class Map : IDisposable
    {
        private int width;
        private int height;
        private Entity[,] objects;
        private Terrain[,] terrains;

        public Map(int width, int height)
        {
            this.width = width;
            this.height = height;
            terrains = new Terrain[width, height];
            objects = new Entity[width, height];
        }

        public int Width
        {
            get
            {
                return width;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }
        }

        public Entity[,] Objects
        {
            get
            {
                return objects;
            }
        }

        public Terrain[,] Terrain
        {
            get
            {
                return terrains;
            }
        }


        public int MapWidth
        {
            get
            {
                return width;
            }
        }

        public int MapHeight
        {
            get
            {
                return height;
            }
        }

        public Boolean SetObject(Entity entity)
        {
            if (objects[entity.MapLocation.X, entity.MapLocation.Y] == null)
            {
                objects[entity.MapLocation.X, entity.MapLocation.Y] = entity;
                return true;
            }
            return false;
        }

        #region IDisposable Members

        public void Dispose()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (objects[x, y] != null)
                    {
                        objects[x, y].Dispose();
                        objects[x, y] = null;
                    }
                    terrains[x, y].Dispose();
                    terrains[x, y] = null;
                }
            }
        }

        #endregion
    }
}
