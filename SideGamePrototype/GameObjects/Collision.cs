using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SideGamePrototype
{
    public interface ICollision
    {
        bool Move(ref Vector2 pos, ref Vector2 vel, ref Vector2 acc, Tile t);

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
            var p = new Vector2(boundingBox.Center.X, boundingBox.Bottom);
            var p2 = new Vector2(boundingBox.X, boundingBox.Bottom);
            var p3 = new Vector2(boundingBox.Right, boundingBox.Bottom);
            return IsNotPassable(p) || IsNotPassable(p2) || IsNotPassable(p3);
        }

        private bool IsNotPassable(Vector2 p)
        {
            var t = GetTileAt(p);
            return t != ' ';
        }

        private char GetTileAt(Vector2 p)
        {
            return this.map.Data[(int)(p.X / 16), (int)(p.Y / 16)];
        }

        public bool Collides(IEnumerable<Vector2> solidPixels)
        {
            return solidPixels.Any(p => IsNotPassable(p));
        }

        public bool Collides(IEnumerable<Vector2> solidPixels, out Vector2 ct)
        {
            ct = solidPixels.FirstOrDefault(p => IsNotPassable(p));
            return ct != default(Vector2);
        }

        public bool Move(ref Vector2 pos, ref Vector2 vel, ref Vector2 acc, Tile t)
        {
            var newPos = pos + vel;
            var pixels = t.GetSolidPoints();
            bool collided = false;

            //Free if stuck
            Vector2 ct;
            while (Collides(Translate(pixels, pos), out ct))
            {
                pos += (pos + new Vector2(8, 8)) - ct;
                return true;
            }

            //Move iteration
            int iterations = 0;
            while (Collides(Translate(pixels, newPos)))
            {
                collided = true;
                newPos = (pos + newPos) / 2;
                iterations++;

                if (iterations > 4)
                {
                    return true;
                }
            }

            /* if (collided)
             {
                 vel.X *= -1;
                 vel.Y *= -1;
             }*/

            pos = newPos;

            return collided;
        }

        private IEnumerable<Vector2> Translate(List<Vector2> o, Vector2 off)
            => o.Select(i => i + off);
    }
}