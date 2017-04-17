using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace SideGamePrototype
{
    public interface ICollision
    {
        bool Move(ref Vector2 pos, Vector2 newPos, Tile t);
        bool StandsOnGround(Rectangle boundingBox);
    }

    public class Collision : ICollision
    {
        private GameMap map;

        public Collision(GameMap map)
        {
            this.map = map;
        }

        public bool StandsOnGround(Rectangle boundingBox)
        {
            //Check X--X--X bottom points of rectangle

            var bottomCenter = new Vector2(boundingBox.Center.X, boundingBox.Bottom);
            var bottomLeft = new Vector2(boundingBox.X, boundingBox.Bottom);
            var bottomRight = new Vector2(boundingBox.Right, boundingBox.Bottom);

            return IsNotPassable(bottomCenter) || IsNotPassable(bottomLeft) || IsNotPassable(bottomRight);
        }

        private bool IsNotPassable(Vector2 p)
            => GetTileAt(p) != ' ';

        private char GetTileAt(Vector2 p)
            => this.map.Data[(int)(p.X / 16), (int)(p.Y / 16)];

        public bool Collides(IEnumerable<Vector2> solidPixels)
            => solidPixels.Any(p => IsNotPassable(p));

        public bool Collides(IEnumerable<Vector2> solidPixels, out Vector2 ct)
        {
            ct = solidPixels.FirstOrDefault(p => IsNotPassable(p));
            return ct != default(Vector2);
        }

        public bool Move(ref Vector2 pos, Vector2 newPos, Tile t)
        {
            var pixels = t.GetSolidPoints();
            bool collided = false;

            //Free if stuck:
            //if pos is a collition before moving try to "push" objectCenter
            //away from collition
            Vector2 collisionPoint;
            if (Collides(Translate(pixels, pos), out collisionPoint))
            {
                var objectCenter = pos + t.Size / 2;
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

        private IEnumerable<Vector2> Translate(List<Vector2> o, Vector2 off)
            => o.Select(i => i + off);
    }
}