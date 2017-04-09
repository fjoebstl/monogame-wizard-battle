using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

        public SideGameProto()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            R.Init(graphics, Content, this.Window.Handle);
            R.System.ToWindow();

            var map = GameMapReader.FromFile(@"Content/Game.txt");
            this.mapRenderer = new GameMapRenderer(map);
            var collision = new Collision(map);

            this.entityList.Add(new Wizard(
                pos: new Vector2(100, 100),
                input: new KeyboardLayout1InputHandler(),
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

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            this.spriteBatch.Begin();
            this.spriteBatch.Draw(R.Textures.Background, new Vector2(0, 0), Color.White);
            this.spriteBatch.End();

            this.spriteBatch.Begin();
            this.mapRenderer.Draw(this.spriteBatch);
            this.entityList.ForEach(e => e.Draw(this.spriteBatch));
            this.spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}