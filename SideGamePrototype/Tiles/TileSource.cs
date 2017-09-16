using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SideGamePrototype
{
    public class TileSource
    {
        public Texture2D Texture { get; private set; }
        public Color[] Raw { get; private set; }

        public TileSource(Texture2D t)
        {
            this.Texture = t;
            this.Raw = t.GetPixels();
        }

        public Dictionary<string, ITile> Load(int tileSize)
        {
            var r = new Dictionary<string, ITile>();

            var maxX = this.Texture.Width / tileSize;
            var maxY = this.Texture.Height / tileSize - 1;

            //Range:
            //starts at 1,1 because  first row and coll  contains grid descritions in image
            //see: Resources.pdn layers
            //Increment:
            //Only scan every second row because row below tiles contain collision information
            //read by tile.
            for (int y = 1; y < maxY; y += 2)
            {
                for (int x = 1; x < maxX; x++)
                {
                    var tile = this.CreateTile(
                            name: $"{y},{(char)('A' - 1 + x)}",
                            sourceRect: new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize));

                    if (tile != null)
                    {
                        r.Add(tile.Name, tile);
                    }
                }
            }

            return r;
        }

        private Tile CreateTile(string name, Rectangle sourceRect)
        {
            var boundingBox = this.GetBoundingBoxOfNonTransparentPixels(sourceRect);
            if (boundingBox.Height * boundingBox.Width <= 1)
                return null;

            var colSourceRect = new Rectangle(sourceRect.X, sourceRect.Y + sourceRect.Height, sourceRect.Width, sourceRect.Height);
            var colBoundingBox = this.GetBoundingBoxOfNonTransparentPixels(colSourceRect);

            Color shapeColor = this.GetPixel(colSourceRect, colBoundingBox.X, colBoundingBox.Y);
            CollisionType type = GetTypeFromColor(shapeColor);

            return new Tile(name, this, sourceRect, type == CollisionType.None ? new Rectangle() : colBoundingBox, type);
        }

        private static CollisionType GetTypeFromColor(Color c)
        {
            if (c == Color.Transparent)
            {
                return CollisionType.None;
            }

            switch (c.GetDominatComponent())
            {
                case 0: return CollisionType.OneWay;
                case 1: return CollisionType.YPassable;
                default: return CollisionType.Solid;
            }
        }

        private Rectangle GetBoundingBoxOfNonTransparentPixels(Rectangle inArea)
        {
            int l, t, r, b;
            l = r = (int)inArea.Width / 2;
            t = b = (int)inArea.Height / 2;

            for (int x = 0; x < inArea.Width; x++)
            {
                for (int y = 0; y < inArea.Height; y++)
                {
                    var pixel = this.Raw.GetPixel(inArea.X + x, inArea.Y + y, this.Texture.Width);
                    if (pixel.A > 0.0f)
                    {
                        l = x < l ? x : l;
                        t = y < t ? y : t;
                        r = x > r ? x : r;
                        b = y > b ? y : b;
                    }
                }
            }

            return new Rectangle(l, t, r - l, b - t);
        }

        private Color GetPixel(Rectangle inArea, int x, int y)
        {
            return this.Raw.GetPixel(inArea.X + x, inArea.Y + y, this.Texture.Width);
        }
    }
}