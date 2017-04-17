using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Resources;

namespace SideGamePrototype
{
    internal interface ICollision
    {
        bool Move(ref Vector2 pos, Vector2 newPos, PixelShape t);
        bool StandsOnGround(IRigidBody body);
    }

    internal class Collision : ICollision
    {
        private GameMap map;

        public Collision(GameMap map)
        {
            this.map = map;
        }

        public bool StandsOnGround(IRigidBody body)
        {
            var p = body.Shape.SolidPixels;
            var min = p.Max(y => y.Y);

            var bottomPixelOfShape = p.Where(y => y.Y == min);
            var collisionLine = Translate(Translate(bottomPixelOfShape, body.Positon), new Vector2(0, 1));

            return Collides(collisionLine);
        }

        private bool IsSolid(Vector2 p)
        {
            char tileChar = GetTileAt(p);

            if (tileChar == ' ')
                return false;

            var solidPoints = R.Textures.Tiles.GetCollisionTileFromChar(tileChar).GetSolidPoints();

            var tileTopLeft = new Vector2((int)(p.X / 16) * 16, (int)(p.Y / 16) * 16);
            var trans = Translate(solidPoints, tileTopLeft);

            if (trans.Any(aa => (int)aa.X == (int)p.X && (int)aa.Y == (int)p.Y))
                return true;

            return false;
        }

        private char GetTileAt(Vector2 p)
            => this.map.Data[(int)(p.X / 16), (int)(p.Y / 16)];

        public bool Collides(IEnumerable<Vector2> solidPixels)
            => solidPixels.Any(p => IsSolid(p));

        public bool Collides(IEnumerable<Vector2> solidPixels, out Vector2 ct)
        {
            ct = solidPixels.FirstOrDefault(p => IsSolid(p));
            return ct != default(Vector2);
        }

        public bool Move(ref Vector2 pos, Vector2 newPos, PixelShape shape)
        {
            var pixels = shape.SolidPixels.ToList();
            bool collided = false;

            //Free if stuck:
            //if pos is a collition before moving try to "push" objectCenter
            //away from collition
            Vector2 collisionPoint;
            if (Collides(Translate(pixels, pos), out collisionPoint))
            {
                var objectCenter = pos + shape.Size / 2;
                pos += objectCenter - collisionPoint;
                return true;
            }

            //Move iteration:
            //If newPos would result in an collision binary search for position
            //between pos <--> newPos.
            //break binary search after maxIterations
            int iterations = 0;
            int maxIterations = 4;
            while (Collides(Translate(pixels, newPos)))
            {
                collided = true;
                newPos = (pos + newPos) / 2;
                iterations++;

                if (iterations > maxIterations)
                {
                    return true;
                }
            }

            pos = newPos;

            return collided;
        }

        private IEnumerable<Vector2> Translate(IEnumerable<Vector2> o, Vector2 off)
            => o.Select(i => i + off);
    }
}