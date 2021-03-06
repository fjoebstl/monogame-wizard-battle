﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Resources;

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

            //init entities
            GameState.Entities = new EntityCollection();
            GameMapMirrorReader.CustomEvents.Add('x', (x, y) =>
            {
                var b = new Spike(new Vector2(x * 16, y * 16));
                GameState.Entities.Add(b);
            });
            GameMapMirrorReader.CustomEvents.Add('v', (x, y) =>
            {
                var b = new Spike(new Vector2(x * 16, y * 16), topSpike: true);
                GameState.Entities.Add(b);
            });

            //create game map
            var map = GameMapMirrorReader.FromFile(@"Content/Game.txt");
            this.mapRenderer = new GameMapRenderer(map);

            //init collision
            GameState.Collision = new Collision(map, GameState.Entities);

            GameState.Logic = new GameLogic();
            GameState.Logic.Init();
        }

        protected override void UnloadContent()
        {
            R.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;

            GameState.Update(dt);

            //DEBUG
            if (Keyboard.GetState().IsKeyDown(Keys.F1))
                GameState.DEBUG = true;
            if (Keyboard.GetState().IsKeyDown(Keys.F2))
                GameState.DEBUG = false;
            if (Keyboard.GetState().IsKeyDown(Keys.F3))
                GameState.PAUSE = true;
            if (Keyboard.GetState().IsKeyDown(Keys.F4))
                GameState.PAUSE = false;
            //DEBUG

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
            this.spriteBatch.Draw(R.Textures.Overlay, new Vector2(0, 0), Color.White);
            GameState.Entities.Draw(this.spriteBatch);
            this.spriteBatch.End();

            //UI
            this.spriteBatch.Begin();
            GameUI.Draw(this.spriteBatch);
            this.spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}