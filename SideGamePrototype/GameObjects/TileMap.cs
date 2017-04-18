using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Resources;
using System;
using System.Collections.Generic;

namespace SideGamePrototype
{
    public class Tile
    {
        private readonly Texture2D t;
        private readonly Rectangle r;

        public Vector2 Size => new Vector2(r.Width, r.Height);

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

        public IEnumerable<Vector2> GetSolidPoints()
        {
            //Get all pixels of image which are not fully transparent

            var l = new List<Vector2>();
            var raw = R.Textures.GetPixels(this.t);

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

            return l.ToArray();
        }
    }

    public class TileMap
    {
        private static readonly int tileSize = 16;

        private readonly Texture2D t;

        private Dictionary<char, string> tileMapping = new Dictionary<char, string>()
        {
            //See Content\Game.txt for map characters
            //See Content\Resources.pdn (Paint.net) for texture coordinates
            { '#', "1,B" },
            { '(', "1,A" },
            { ')', "1,C" },
            { '§', "1,D" },
        };

        private Dictionary<Rectangle, Tile> cache = new Dictionary<Rectangle, Tile>();

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

        public Tile GetCollisionTileFromChar(char ch)
        {
            if (!tileMapping.ContainsKey(ch))
                return null;

            return GetCollisionTileFromString(tileMapping[ch]);
        }

        public Tile GetTileFromString(string resKey)
            => this.GetTileFromString(resKey, t => t);

        public Tile GetCollisionTileFromString(string resKey)
            => this.GetTileFromString(resKey, t => t + new Vector2(0, 1));

        private Tile GetTileFromString(string resKey, Func<Vector2, Vector2> trans)
        {
            var r = VecToRec(trans(StrToVec(resKey)));
            if (!this.cache.ContainsKey(r))
            {
                this.cache.Add(r, new Tile(this.t, r));
            }
            return this.cache[r];
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