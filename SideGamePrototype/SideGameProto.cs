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
            GameState.Camera = new Camera2D(graphics.GraphicsDevice.Viewport);

            //create game map
            var map = GameMapMirrorReader.FromFile(@"Content/Game.txt");
            this.mapRenderer = new GameMapRenderer(map);

            //init entities
            GameState.Entities = new EntityCollection();

            var collision = new Collision(map, GameState.Entities);

            GameState.Entities.Add(new Wizard(
                pos: new Vector2(100, 500),
                input: new KeyboardLayout1InputHandler(),
                collision: collision));

            GameState.Entities.Add(new Wizard(
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

            GameState.Entities.Update(dt);

            //Camera TEST
            var w1 = GameState.Entities.All[0];
            var w2 = GameState.Entities.All[1];

            var origin = w1.Body.Positon - (w1.Body.Positon - w2.Body.Positon) / 2.0f;

            GameState.Camera.Position = origin - new Vector2(
                this.graphics.GraphicsDevice.Viewport.Width,
                this.graphics.GraphicsDevice.Viewport.Height) / 2.0f;

            var r = MathUtil.Union(w1.Body.BoundingBox, w2.Body.BoundingBox);
            r.Inflate(200, 100);

            GameState.Camera.ZoomWidth(r.Width, 1.0f, 3.0f);
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
            var viewMatrix = GameState.Camera.GetViewMatrix();

            this.spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: viewMatrix);
            this.mapRenderer.Draw(this.spriteBatch);
            GameState.Entities.Draw(this.spriteBatch);

            this.spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}