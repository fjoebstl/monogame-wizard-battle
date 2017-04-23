using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Resources;

namespace SideGamePrototype
{
    internal interface ICollision
    {
        CollisionResult Move(IRigidBody body, Vector2 targetPosition);
    }

    internal class CollisionResult
    {
        public bool WasCollision { get; set; }
        public bool StandsOnGround { get; set; } = false;
        public Vector2 AvailablePosition { get; set; }
        public List<IEntity> EntityCollisions { get; set; } = new List<IEntity>();
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
            //Should be re-implemented according to
            //http://higherorderfun.com/blog/2012/05/20/the-guide-to-implementing-2d-platformers/

            int yoff = 0;
            int xoff = 0;

            var entities = new HashSet<IEntity>();
            IEntity e;

            //Vertical
            var offV = new Point(0, (int)targetPosition.Y - (int)body.Positon.Y);

            var hitV = Collides(body, offV, out e);
            if (e != null)
                entities.Add(e);

            var selfV = Translate(body.BoundingBox, offV);
            if (hitV != Rectangle.Empty)
            {
                yoff = selfV.Y < hitV.Y ? selfV.Bottom - hitV.Top : selfV.Top - hitV.Bottom;
            }

            //Horizontal
            var offH = new Point((int)targetPosition.X - (int)body.Positon.X, 0 - yoff);
            var hitH = Collides(body, offH, out e);
            if (e != null)
                entities.Add(e);

            var selfH = Translate(body.BoundingBox, offH);
            if (hitH != Rectangle.Empty)
            {
                xoff = selfH.X < hitH.X ? selfH.Right - hitH.Left : selfH.Left - selfH.Right;
            }

            var onGround = Collides(body, offV + new Point(xoff, yoff + 1), out e) != Rectangle.Empty;

            var r = new CollisionResult();
            r.AvailablePosition = targetPosition - new Vector2(xoff, yoff);
            r.WasCollision = hitV != Rectangle.Empty || hitH != Rectangle.Empty;
            r.StandsOnGround = onGround;
            r.EntityCollisions = entities.ToList();

            return r;
        }

        private Rectangle Collides(IRigidBody body, Point off, out IEntity collEntity)
        {
            var hitRect = Translate(body.BoundingBox, off);
            collEntity = null;

            foreach (var p in GetEdgePoints(hitRect))
            {
                var tileChar = GetTileCharAt(p);
                if (tileChar != ' ')
                {
                    var tile = R.Textures.Tiles.GetTileFromChar(tileChar);
                    var tileTopLeft = new Point((int)(p.X / 16) * 16, (int)(p.Y / 16) * 16);
                    if (tile.CollisionType == CollisionType.Solid)
                    {
                        var translatedOther = Translate(tile.CollisionBox, tileTopLeft);

                        if (hitRect.Intersects(translatedOther))
                        {
                            return translatedOther;
                        }
                    }
                }
            }

            foreach (var otherEntity in this.entities.All.Where(e => e.Body != body))
            {
                if (hitRect.Intersects(otherEntity.Body.BoundingBox))
                {
                    collEntity = otherEntity;
                    return otherEntity.Body.BoundingBox;
                }
            }

            return Rectangle.Empty;
        }

        private char GetTileCharAt(Vector2 p)
            => this.map.Data[(int)(p.X / 16), (int)(p.Y / 16)];

        private Rectangle Translate(Rectangle r, Point off)
            => new Rectangle(r.X + off.X, r.Y + off.Y, r.Width, r.Height);

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