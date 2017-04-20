using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Resources;
using System.Collections.Generic;

namespace SideGamePrototype
{
    public class SideGameProto : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private GameMapRenderer mapRenderer;
        private List<IEntity> entityList = new List<IEntity>();
        private Camera2D cam;

        public SideGameProto()
        {
            graphics = new GraphicsDeviceManager(this);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //init game resources
            R.Init(graphics, Content, this.Window.Handle);
            R.System.ToWindow();

            //init camera
            this.cam = new Camera2D(graphics.GraphicsDevice.Viewport);

            //create game map + player
            var map = GameMapMirrorReader.FromFile(@"Content/Game.txt");
            this.mapRenderer = new GameMapRenderer(map);
            var collision = new Collision(map, this.entityList);

            this.entityList.Add(new Wizard(
                pos: new Vector2(100, 500),
                input: new KeyboardLayout1InputHandler(),
                collision: collision));

            this.entityList.Add(new Wizard(
                pos: new Vector2(200, 500),
                input: new KeyboardLayout2InputHandler(),
                collision: collision));
        }

        protected override void UnloadContent()
        {
            R.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;

            this.entityList.ForEach(e => e.Update(dt));

            //Camera TEST
            var w1 = this.entityList[0];
            var w2 = this.entityList[1];

            var origin = w1.Body.Positon - (w1.Body.Positon - w2.Body.Positon) / 2.0f;

            this.cam.Position = origin - new Vector2(
                this.graphics.GraphicsDevice.Viewport.Width,
                this.graphics.GraphicsDevice.Viewport.Height) / 2.0f;

            var r = MathUtil.Union(w1.Body.BoundingBox, w2.Body.BoundingBox);
            r.Inflate(200, 100);

            this.cam.ZoomWidth(r.Width, 1.0f, 3.0f);
            //Camera TEST

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Background
            this.spriteBatch.Begin();
            this.spriteBatch.Draw(R.Textures.Background, new Vector2(0, 0), Color.White);
            this.spriteBatch.End();

            //Foreground
            var viewMatrix = this.cam.GetViewMatrix();

            this.spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: viewMatrix);
            this.mapRenderer.Draw(this.spriteBatch);
            this.entityList.ForEach(e => e.Draw(this.spriteBatch));

            this.spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}