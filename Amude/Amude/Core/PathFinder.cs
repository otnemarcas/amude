using System;
using System.Collections.Generic;
using System.Linq;
using Amude.Domain;
using Microsoft.Xna.Framework;
using System.Threading;

namespace Amude.Core
{
    enum SearchType
    { 
        AvailablePositions,
        Route,
        AttackPositions
    }

    enum Orientation
    { 
        Horizontal,
        Vertical
    }

    internal class Position
    {
        public Point Point { get; set; }
        public int Steps { get; set; }
        public Position Parent { get; set; }

        public Position(int x, int y, int steps, Position parent)
            : this(x, y , steps)
        {
            this.Parent = parent;    
        }

        public Position(int x, int y, int steps)
            : this(x, y)
        {
            this.Steps = steps;
        }

        public Position(int x, int y)
        {
            this.Point = new Point(x, y);
        }

        public Position(Point point)
        {
            this.Point = point;
        }
    }

    internal class PositionComparer : IEqualityComparer<Position>
    {

        public bool Equals(Position x, Position y)
        {
            return x.Steps <= y.Steps && x.Point.X == y.Point.X && x.Point.Y == y.Point.Y;
        }

        public int GetHashCode(Position obj)
        {
            return obj.Point.X * obj.Point.Y * obj.Steps;
        }

    }

    internal class PathFinder
    {
        private Map map;
        private Queue<Position> positiosToCheck;
        private Queue<Position> checkedPositions;
        private Position origin;
        private Position destiny;
        private int range;
        private SearchType searchType;
        private PositionComparer positionComparer;
        private List<Point> attackPositions;

        public PathFinder(Map map)
        {
            positionComparer = new PositionComparer();
            positiosToCheck = new Queue<Position>();
            checkedPositions = new Queue<Position>();
            this.map = map;
        }

        public List<Point> AvaliablePositions(int x, int y, int range) 
        {
            return AvaliablePositions(new Point(x, y), range);
        }

        public List<Point> AvaliablePositions(Point point, int range)
        {
            List<Point> ret = null;

            if (IsInsideMap(point))
            {
                searchType = SearchType.AvailablePositions;
                positiosToCheck = new Queue<Position>();
                checkedPositions = new Queue<Position>();

                this.origin = new Position(point);
                this.range = range;

                positiosToCheck.Enqueue(origin);

                Process();

                ret = checkedPositions.Select<Position, Point>(p => p.Point).ToList();
                
                ret.RemoveAt(0);
            }
            else
            {
                throw new Exception("Posição informada esta fora do intervalo do mapa.");
            }

            return ret;
        }

        public List<Point> GetRoute(Point origin, Point destiny)
        {
            List<Point> route = null;

            if(!IsInsideMap(origin))
                throw new Exception("Posição de origem esta fora do intervalo do mapa.");
            
            if(!IsInsideMap(destiny))
                throw new Exception("Posição de destino esta fora do intervalo do mapa.");
            
            searchType = SearchType.Route;
            positiosToCheck = new Queue<Position>();
            checkedPositions = new Queue<Position>();

            this.origin = new Position(origin);
            this.destiny = new Position(destiny);

            positiosToCheck.Enqueue(this.origin);

            Process();

            route = SplitRoute();

            return route;
        }

        public List<Point> GetAttackPositions(Point origin, int range)
        {
            if (IsInsideMap(origin))
            {
                searchType = SearchType.AttackPositions;
                positiosToCheck = new Queue<Position>();
                checkedPositions = new Queue<Position>();
                attackPositions = new List<Point>();

                this.origin = new Position(origin);
                this.range = range;

                positiosToCheck.Enqueue(this.origin);

                AttackPositioins();

                attackPositions = RemoveInvalidPositions(attackPositions);
            }
            else
            {
                throw new Exception("Posição informada esta fora do intervalo do mapa.");
            }

            return attackPositions;
        }

        private void Process()
        {
            Position position = positiosToCheck.Dequeue();

            while (!(Finalize(position)))
            {
                if (NotVerified(position))
                {
                    checkedPositions.Enqueue(position);
                }

                int steps = position.Steps + 1;

                Position up = new Position(position.Point.X, position.Point.Y - 1, steps, position);
                if (position.Point.Y > 0
                    && CanWalk(map.Objects[position.Point.X, position.Point.Y - 1])
                    && NotVerified(up))
                {
                    positiosToCheck.Enqueue(up);
                }

                Position down = new Position(position.Point.X, position.Point.Y + 1, steps, position);
                if (position.Point.Y < map.MapHeight - 1
                    && CanWalk(map.Objects[position.Point.X, position.Point.Y + 1])
                    && NotVerified(down))
                {
                    positiosToCheck.Enqueue(down);
                }

                Position left = new Position(position.Point.X - 1, position.Point.Y, steps, position);
                if (position.Point.X > 0
                    && CanWalk( map.Objects[position.Point.X - 1, position.Point.Y])
                    && NotVerified(left))
                {
                    positiosToCheck.Enqueue(left);
                }

                Position right = new Position(position.Point.X + 1, position.Point.Y, steps, position);
                if (position.Point.X < map.Width - 1
                    && CanWalk(map.Objects[position.Point.X + 1, position.Point.Y])
                    && NotVerified(right))
                {
                    positiosToCheck.Enqueue(right);
                }

                if (positiosToCheck.Count == 0)
                {
                    break;
                }

                position = positiosToCheck.Dequeue();
            }
        }

        private void AttackPositioins()
        {
            Position position = positiosToCheck.Dequeue();

            while (position.Steps <= range)
            {
                if (NotVerified(position))
                {
                    checkedPositions.Enqueue(position);
                    if (map.Objects[position.Point.X, position.Point.Y] is Character)
                    {
                        Character character = (Character)map.Objects[position.Point.X, position.Point.Y];
                        if (character.Owner.Name != Controller.GetInstance().GetLocalPlayer().Name)
                        {
                            attackPositions.Add(position.Point);
                        }
                    }
                }

                int steps = position.Steps + 1;

                Position up = new Position(position.Point.X, position.Point.Y - 1, steps, position);
                if (position.Point.Y > 0
                    && NotVerified(up))
                {
                    positiosToCheck.Enqueue(up);
                }

                Position down = new Position(position.Point.X, position.Point.Y + 1, steps, position);
                if (position.Point.Y < map.MapHeight - 1
                    && NotVerified(down))
                {
                    positiosToCheck.Enqueue(down);
                }

                Position left = new Position(position.Point.X - 1, position.Point.Y, steps, position);
                if (position.Point.X > 0
                    && NotVerified(left))
                {
                    positiosToCheck.Enqueue(left);
                }

                Position right = new Position(position.Point.X + 1, position.Point.Y, steps, position);
                if (position.Point.X < map.Width - 1
                    && NotVerified(right))
                {
                    positiosToCheck.Enqueue(right);
                }

                position = positiosToCheck.Dequeue();
            }
        }

        private bool NotVerified(Position position)
        {
            return !checkedPositions.Contains(position, positionComparer);
        }

        private bool Finalize(Position position)
        {
            if (searchType == SearchType.AvailablePositions)
            {
                if (position.Steps > range)
                {
                    return true;
                }
            }
            else
            {
                bool ret = position.Point.Equals(destiny.Point);

                if (ret)
                {
                    destiny = position;
                }

                return ret;
            }

            return false;
        }

        private bool IsInsideMap(Point point)
        {
            return (point.X >= 0 && point.X < map.MapWidth)                
                && (point.Y >= 0 && point.Y < map.MapHeight);
        }

        private List<Point> SplitRoute()
        {
            List<Point> route = new List<Point>();
            List<Point> splitedRoute = new List<Point>();
            Position position = destiny;
            Orientation orientation = Orientation.Horizontal;

            while (position.Parent != null)
            {
                route.Add(position.Point);
                position = position.Parent;
            }

            route.Add(position.Point);
            route.Reverse();

            for (int i = 0; i < route.Count; i++)
            {
                Point current = route[i];

                if (i == 0)
                {
                    if (route.Count > 1)
                    {
                        Point next = route[i + 1];
                        if (current.X != next.X)
                        {
                            orientation = Orientation.Horizontal;
                        }
                        else
                        {
                            orientation = Orientation.Vertical;
                        }
                    }
                    else
                    {
                        splitedRoute.Add(current);                    
                    }
                }
                else if (i == route.Count - 1)
                {
                    splitedRoute.Add(current);
                }
                else 
                {
                    Point next = route[i + 1];

                    if (current.X != next.X)
                    {
                        if (orientation != Orientation.Horizontal)
                        {
                            splitedRoute.Add(current);
                        }
                        orientation = Orientation.Horizontal;
                    }
                    else
                    {
                        if (orientation != Orientation.Vertical)
                        {
                            splitedRoute.Add(current);
                        }
                        orientation = Orientation.Vertical;                    
                    }
                }
            }

            return splitedRoute;
        }

        private Boolean CanWalk(Entity entity)
        {
            if (entity == null)
            {
                return true;
            }
            else
            {
                if (entity is Character)
                {
                    Character character = (Character)entity;
                    return character.Health.IsDead;
                }
                return false;
            }
        }

        private List<Point> RemoveInvalidPositions(List<Point> positions)
        {
            List<Point> ret = new List<Point>();

            foreach (Point position in positions)
            {
                if (map.Objects[position.X, position.Y] == null)
                {
                    ret.Add(position);   
                }
                else if (map.Objects[position.X, position.Y] is Character)
                {
                    Character character = (Character)map.Objects[position.X, position.Y];
                    if (!character.Health.IsDead)
                    {
                        ret.Add(position);
                    }
                }
            }

            return ret;
        }

    }
}
