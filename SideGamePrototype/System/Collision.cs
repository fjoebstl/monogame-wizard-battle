using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Resources;
using System;

namespace SideGamePrototype
{
    internal interface ICollision
    {
        CollisionResult Move(IRigidBody body, Vector2 targetPosition);
    }

    internal class CollisionResult
    {
        public List<Point> CollisionPoints { get; set; } = new List<Point>();
        public bool WasCollision => this.CollisionPoints.Any();
        public bool StandsOnGround { get; set; } = false;

        public void Add(CollisionResult collResult)
        {
            this.CollisionPoints.AddRange(collResult.CollisionPoints);
            this.StandsOnGround = collResult.StandsOnGround;
        }
    }

    internal class Collision : ICollision
    {
        private readonly GameMap map;
        private readonly IEntityCollection entities;

        public Collision(GameMap map, IEntityCollection entities)
        {
            this.map = map;
            this.entities = entities;
        }

        public CollisionResult Move(IRigidBody body, Vector2 targetPosition)
        {
            var tt = new Point((int)Math.Round(targetPosition.X), (int)Math.Round(targetPosition.Y));

            if (body.Positon.ToPoint().Equals(tt))
                return null;

            var oldPosition = body.Positon;

            body.Positon = targetPosition;
            var r = Collides(body);
            body.Positon = oldPosition;

            return r;
        }

        private CollisionResult Collides(IRigidBody body)
        {
            //Points to test
            var test = Translate(body.Shape.SolidPixels, body.Positon);

            //Collect all tile and entity points
            var tiles = GetEdgePoints(body.BoundingBox)
                .SelectMany(p => TileCharToPoints(GetTileCharAt(p), p));

            var entities = this.entities.All
                .Where(e => e.Body != body && e.Body.BoundingBox.Intersects(body.BoundingBox))
                .SelectMany(e => Translate(e.Body.Shape.SolidPixels, e.Body.Positon.ToPoint()));

            var all = tiles.Union(entities);

            //Test collision
            var r = all.Intersect(test);

            //Test if body on ground
            var origin = body.BoundingBox.Center;
            var rOnGround = all.Intersect(Translate(test, new Point(0, 1)));
            var onGround = rOnGround.Any() && rOnGround.Any(p => p.Y > origin.Y);

            return new CollisionResult() { CollisionPoints = r.ToList(), StandsOnGround = onGround };
        }

        private IEnumerable<Point> TileCharToPoints(char tileChar, Vector2 p)
        {
            if (tileChar == ' ')
                return new List<Point>();

            var solidPoints = R.Textures.Tiles.GetCollisionTileFromChar(tileChar).GetSolidPoints();
            var tileTopLeft = new Point((int)(p.X / 16) * 16, (int)(p.Y / 16) * 16);
            var trans = Translate(solidPoints, tileTopLeft);

            return trans;
        }

        private char GetTileCharAt(Vector2 p)
            => this.map.Data[(int)(p.X / 16), (int)(p.Y / 16)];

        private IEnumerable<Vector2> Translate(IEnumerable<Vector2> o, Vector2 off)
            => o.Select(i => i + off);

        private IEnumerable<Point> Translate(IEnumerable<Point> o, Vector2 off)
            => o.Select(i => (i.ToVector2() + off).ToPoint());

        private IEnumerable<Point> Translate(IEnumerable<Point> o, Point off)
            => o.Select(i => i + off);

        private IEnumerable<Vector2> GetEdgePoints(Rectangle r)
        {
            return new Vector2[4]
            {
                new Vector2(r.Left, r.Top),
                new Vector2(r.Right, r.Top),
                new Vector2(r.Left, r.Bottom),
                new Vector2(r.Right, r.Bottom),
            };
        }
    }
}