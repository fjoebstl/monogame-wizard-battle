using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SideGamePrototype
{
    public interface ITileCollection
    {
        ITile GetTileFromChar(char ch);
        ITile GetTileFromString(string resKey);

        void Load(Texture2D texture, int tileSize);
    }

    public class TileCollection : ITileCollection
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

        private Dictionary<string, ITile> cache = new Dictionary<string, ITile>();

        public void Load(Texture2D texture, int tileSize)
        {
            var maxX = texture.Width / tileSize;
            var maxY = texture.Height / tileSize - 1;

            var info = new TileSource(texture);
            this.cache = info.Load(tileSize);
        }

        public ITile GetTileFromChar(char ch)
        {
            if (!tileMapping.ContainsKey(ch))
                return null;

            return GetTileFromString(tileMapping[ch]);
        }

        public ITile GetTileFromString(string resKey)
        {
            return this.cache[resKey];
        }
    }
}