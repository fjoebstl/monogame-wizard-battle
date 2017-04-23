using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Resources;
using System;
using System.Collections.Generic;

namespace SideGamePrototype
{
    public interface ITileMap
    {
        Tile GetTileFromChar(char ch);
        Tile GetTileFromString(string resKey);

        void Load(Texture2D texture, int tileSize);
    }

    public class TextureInfo
    {
        public Texture2D Texture { get; set; }
        public Color[] Raw { get; set; }
    }

    public class TileMap : ITileMap
    {
        private Dictionary<char, string> tileMapping = new Dictionary<char, string>()
        {
            //See Content\Game.txt for map characters
            //See Content\Resources.pdn (Paint.net) for texture coordinates
            { '#', "1,B" },
            { '(', "1,A" },
            { ')', "1,C" },
            { '§', "1,D" },
        };

        private Dictionary<string, Tile> cache = new Dictionary<string, Tile>();

        public void Load(Texture2D texture, int tileSize)
        {
            var maxX = texture.Width / tileSize;
            var maxY = texture.Height / tileSize - 1;

            var info = new TextureInfo()
            {
                Texture = texture,
                Raw = texture.GetPixels(),
            };

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
                    var tile = new Tile(info, new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize));
                    tile.Name = $"{y},{(char)('A' - 1 + x)}";

                    if (!tile.IsEmpty)
                    {
                        this.cache.Add(tile.Name, tile);
                    }
                }
            }
        }

        public Tile GetTileFromChar(char ch)
        {
            if (!tileMapping.ContainsKey(ch))
                return null;

            return GetTileFromString(tileMapping[ch]);
        }

        public Tile GetTileFromString(string resKey)
        {
            return this.cache[resKey];
        }
    }
}