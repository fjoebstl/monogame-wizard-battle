using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Resources;
using System.IO;
using System.Linq;

namespace SideGamePrototype
{
    public class GameMap
    {
        public int Width { get; }
        public int Height { get; }
        public char[,] Data { get; }

        public GameMap(int width, int height, char[,] data)
        {
            this.Width = width;
            this.Height = height;
            this.Data = data;
        }
    }

    public class GameMapRenderer
    {
        private readonly GameMap map;

        public GameMapRenderer(GameMap map)
        {
            this.map = map;
        }

        public void Draw(SpriteBatch s)
        {
            for (int x = 0; x < this.map.Width; x++)
            {
                for (int y = 0; y < this.map.Height; y++)
                {
                    var t = R.Textures.Tiles.GetTileFromChar(this.map.Data[x, y]);

                    if (t != null)
                        t.DrawInTileCoordinates(s, new Vector2(x, y));
                }
            }
        }
    }

    public class GameMapReader
    {
        public static GameMap FromFile(string file)
        {
            var lines = File.ReadAllLines(file);
            int width = lines[0].Length;
            int height = lines.Count();

            char[,] data = new char[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    data[x, y] = lines[y][x];
                }
            }

            return new GameMap(width, height, data);
        }
    }

    public class GameMapMirrorReader
    {
        public static GameMap FromFile(string file)
        {
            var lines = File.ReadAllLines(file);
            int width = lines[0].Length;
            int height = lines.Count();

            char[,] data = new char[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var rx = x;
                    if (x > width / 2)
                    {
                        rx = width - x - 1;
                    }

                    var c = lines[y][rx];

                    if (x > width / 2)
                    {
                        if (c == '(')
                            c = ')';
                        else if (c == ')')
                            c = '(';
                    }

                    data[x, y] = c;
                }
            }

            return new GameMap(width, height, data);
        }
    }
}