using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Resources;

namespace SideGamePrototype
{
    internal class Wizard2 : IEntity
    {
        private DrawableState currentState;
        private RigidBody body;

        public Wizard2(Vector2 pos, IInputHandler input, ICollision collision)
        {
            this.body = new RigidBody(pos, GetCurrentShape, collision);
            this.currentState = new WalkingState(body, input);
        }

        public void Draw(SpriteBatch s)
        {
            if (this.body.WasCollision)
            {
                var b = this.body.BoundingBox;
                //b.Inflate(-4, -4);
                s.Draw(R.Textures.Red, b, Color.White);
            }

            this.currentState.Draw(s);
        }

        public void Update(float dt)
        {
            this.currentState.Update(dt);
            this.body.Update(dt);
        }

        private PixelShape GetCurrentShape()
            => PixelShape.FromTile(this.currentState.GetTile());
    }

    internal class WalkingState : DrawableState
    {
        public WalkingState(RigidBody body, IInputHandler input)
            : base(body, input)
        {
        }

        public override State Update(float gt)
        {
            this.body.AddForce("g", new Vector2(0, 3.5f));

            if (this.input.LeftPressed)
            {
                this.body.AddVelocityComponent("move", new Vector2(-3, 0));
                this.body.LookAt = new Vector2(-1, 0);
            }
            else if (this.input.RightPressed)
            {
                this.body.AddVelocityComponent("move", new Vector2(3, 0));
                this.body.LookAt = new Vector2(1, 0);
            }

            return base.Update(gt);
        }

        protected override string GetTileString()
            => "3,A";
    }

    internal abstract class DrawableState : State
    {
        protected readonly RigidBody body;
        protected readonly IInputHandler input;

        protected abstract string GetTileString();

        public DrawableState(RigidBody body, IInputHandler input)
        {
            this.body = body;
            this.input = input;
        }

        public void Draw(SpriteBatch s)
        {
            var t = GetTile();
            t.Draw(s, this.body.Positon, this.body.LookAt.X > 0
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None);
        }

        public Tile GetTile()
            => R.Textures.Tiles.GetTileFromString(this.GetTileString());
    }
}