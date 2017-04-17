using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SideGamePrototype;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Resources
{
    public static class R
    {
        public static T Textures;
        public static S System;

        internal static void Init(GraphicsDeviceManager d, ContentManager content, IntPtr window)
        {
            Textures = new T(d.GraphicsDevice, content);
            System = new S(d, content, window);
        }

        internal static void Unload()
        {
            Textures.Dispose();
        }

        public class T : IDisposable
        {
            public readonly Texture2D Background;
            public readonly TileMap Tiles;

            public Texture2D Red;

            private Texture2D tileTexture;
            private Dictionary<Texture2D, Color[]> cachedPixelData = new Dictionary<Texture2D, Color[]>();

            public T(GraphicsDevice d, ContentManager c)
            {
                c.RootDirectory = "./Content";
                this.Background = c.Load<Texture2D>("Background.png");
                this.tileTexture = c.Load<Texture2D>("Resources.png");
                this.Red = Texture2dHelper.CreateTexture(d, 10, 10, (_) => Color.Red);
                this.Tiles = new TileMap(tileTexture);
            }

            public Color[] GetPixels(Texture2D t)
            {
                if (!this.cachedPixelData.ContainsKey(t))
                {
                    this.cachedPixelData[t] = t.GetPixels();
                }

                return this.cachedPixelData[t];
            }

            public void Dispose()
            {
                this.Background.Dispose();
                this.tileTexture.Dispose();
            }
        }

        public class S
        {
            public Rectangle Window => getWindow();
            public string ContentDirectory;
            public GraphicsDevice GraphicsDevice;

            private Func<Rectangle> getWindow;
            private GraphicsDeviceManager deviceManager;
            private IntPtr window;

            internal S(GraphicsDeviceManager deviceManager, ContentManager content, IntPtr window)
            {
                this.GraphicsDevice = deviceManager.GraphicsDevice;
                this.ContentDirectory = content.RootDirectory;
                this.deviceManager = deviceManager;
                this.window = window;
                this.getWindow = () => new Rectangle(
                    0, 0,
                    deviceManager.PreferredBackBufferWidth, deviceManager.PreferredBackBufferHeight);
            }

            public void ToWindow()
            {
                this.deviceManager.PreferredBackBufferWidth = 1440 - 16;
                this.deviceManager.PreferredBackBufferHeight = 800;
                this.deviceManager.IsFullScreen = false;
                this.deviceManager.ApplyChanges();

                this.CenterWindow();
            }

            public void ToFullscreen()
            {
                this.deviceManager.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
                this.deviceManager.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
                this.deviceManager.IsFullScreen = true;
                this.deviceManager.ApplyChanges();
            }

            private void CenterWindow()
            {
                var r = Screen.FromControl(Control.FromHandle(this.window)).Bounds;
                var w = this.deviceManager.PreferredBackBufferWidth;
                var h = this.deviceManager.PreferredBackBufferHeight;
                var x = r.Width / 2 - w / 2;
                var y = r.Height / 2 - h / 2;

                this.SetWindowPosition(x, y);
            }

            private void SetWindowPosition(int x, int y)
            {
                var form = Control.FromHandle(this.window);
                form.Location = new System.Drawing.Point(x, y);
            }
        }
    }
}