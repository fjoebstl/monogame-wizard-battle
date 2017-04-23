using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SideGamePrototype;
using System;
using System.Windows.Forms;

namespace Resources
{
    public static class R
    {
        public static T Textures;
        public static S System;
        public static F Fonts;

        internal static void Init(GraphicsDeviceManager d, ContentManager content, IntPtr window)
        {
            Textures = new T(d.GraphicsDevice, content);
            System = new S(d, content, window);
            Fonts = new F(content);
        }

        internal static void Unload()
        {
            Textures.Dispose();
        }

        public class F
        {
            public SpriteFont Default { get; private set; }

            public F(ContentManager content)
            {
                this.Default = content.Load<SpriteFont>("Segoe_UI_10.5_Regular");
            }
        }

        public class T : IDisposable
        {
            public readonly Texture2D Background;
            public readonly Texture2D Overlay;
            public readonly ITileCollection Tiles;

            public Texture2D Red;
            public Texture2D White;

            private Texture2D tileTexture;

            public T(GraphicsDevice d, ContentManager c)
            {
                c.RootDirectory = "./Content";
                this.Background = c.Load<Texture2D>("Background.png");
                this.Overlay = c.Load<Texture2D>("Overlay.png");
                this.tileTexture = c.Load<Texture2D>("Resources.png");
                this.Red = Texture2dHelper.CreateTexture(d, 10, 10, (_) => Color.Red);
                this.White = Texture2dHelper.CreateTexture(d, 10, 10, (_) => Color.White);

                this.Tiles = new TileCollection();
                this.Tiles.Load(texture: this.tileTexture, tileSize: 16);
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