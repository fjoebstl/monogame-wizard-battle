using Microsoft.Xna.Framework;
using System;

namespace SideGamePrototype
{
    internal static class MathUtil
    {
        public static Rectangle Union(Rectangle a, Rectangle b)
        {
            int x1 = Math.Min(a.X, b.X);
            int x2 = Math.Max(a.X + a.Width, b.X + b.Width);
            int y1 = Math.Min(a.Y, b.Y);
            int y2 = Math.Max(a.Y + a.Height, b.Y + b.Height);

            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }

        public static Rectangle FlipHorizontal(Rectangle r, int size)
           => new Rectangle(size - r.X - r.Width, r.Y, r.Width, r.Height);
    }
}