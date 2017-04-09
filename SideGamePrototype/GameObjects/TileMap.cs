using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SideGamePrototype
{
    public class Tile
    {
        private readonly Texture2D t;
        private readonly Rectangle r;

        public Tile(Texture2D t, Rectangle r)
        {
            this.t = t;
            this.r = r;
        }

        public void DrawInTileCoordinates(SpriteBatch s, Vector2 tileCoordinates, SpriteEffects eff = SpriteEffects.None)
        {
            var dest = new Rectangle(
                   (int)tileCoordinates.X * r.Width,
                   (int)tileCoordinates.Y * r.Height,
                   r.Width,
                   r.Height);
            this.Draw(s, dest, eff);
        }

        public void Draw(SpriteBatch s, Vector2 d, SpriteEffects eff = SpriteEffects.None)
        {
            var dest = new Rectangle((int)d.X, (int)d.Y, r.Width, r.Height);
            this.Draw(s, dest, eff);
        }

        public void Draw(SpriteBatch s, Rectangle destination, SpriteEffects eff = SpriteEffects.None)
            => s.Draw(
                texture: this.t,
                destinationRectangle: destination,
                sourceRectangle: r,
                color: Color.White,
                rotation: 0.0f,
                origin: new Vector2(),
                effects: eff,
                layerDepth: 1.0f);

        public List<Vector2> GetSolidPoints()
        {
            var l = new List<Vector2>();

            //Very inefficient :( should be cached somewhere
            var raw = t.GetPixels();

            for (int x = 0; x < this.r.Width; x++)
            {
                for (int y = 0; y < this.r.Y; y++)
                {
                    var pixel = raw.GetPixel(this.r.X + x, this.r.Y + y, this.t.Width);
                    if (pixel.A > 0.0f)
                    {
                        l.Add(new Vector2(x, y));
                    }
                }
            }

            return l;
        }
    }

    public static class Texture2dHelper
    {
        public static Color GetPixel(this Color[] colors, int x, int y, int width)
        {
            return colors[x + (y * width)];
        }

        public static Color[] GetPixels(this Texture2D texture)
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(colors1D);
            return colors1D;
        }
    }

    public class TileMap
    {
        private static readonly int tileSize = 16;

        private readonly Texture2D t;

        private Dictionary<char, string> tileMapping = new Dictionary<char, string>()
        {
            { '#', "1,B" },
            { '(', "1,A" },
            { ')', "1,C" },
        };

        private Dictionary<string, Tile> cache = new Dictionary<string, Tile>();

        public TileMap(Texture2D t)
        {
            this.t = t;
        }

        public Tile GetTileFromChar(char ch)
        {
            if (!tileMapping.ContainsKey(ch))
                return null;

            return GetTileFromString(tileMapping[ch]);
        }

        public Tile GetTileFromString(string resKey)
        {
            if (!this.cache.ContainsKey(resKey))
            {
                this.cache.Add(resKey, new Tile(this.t, VecToRec(StrToVec(resKey))));
            }
            return this.cache[resKey];
        }

        private static Vector2 StrToVec(string s)
        {
            var y = int.Parse(s.Split(',')[0]);
            var x = char.Parse(s.Split(',')[1]) - 'A' + 1;
            return new Vector2(x, y);
        }

        private static Rectangle VecToRec(Vector2 v)
            => new Rectangle((int)v.X * tileSize, (int)v.Y * tileSize, tileSize, tileSize);
    }
}