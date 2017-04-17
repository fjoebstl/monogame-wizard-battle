using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Resources;

namespace SideGamePrototype
{
    internal class Wizard : IEntity
    {
        private DrawableState currentState;
        private RigidBody body;

        public Wizard(Vector2 pos, IInputHandler input, ICollision collision)
        {
            this.body = new RigidBody(pos, GetCurrentShape, collision);

            //States
            var walkingState = new WalkingState(body, input);
            var fallingState = new FallingState(body, input);
            var jumpingState = new JumpingState(body, input);

            //Triggers
            var onGround = new BasicTrigger(() => collision.StandsOnGround(this.body.BoundingBox));
            var notOnGround = new BasicTrigger(() => !collision.StandsOnGround(this.body.BoundingBox));
            var jumpReady = CombinedTrigger.And(new DelayTrigger(0.2f), new BasicTrigger(() => input.JumpPressed));
            var notJumpPressed = new BasicTrigger(() => !input.JumpPressed);
            var jumpEnds = CombinedTrigger.Or(new DelayTrigger(0.3f), notJumpPressed);

            walkingState.Add(notOnGround, () => fallingState);
            walkingState.Add(jumpReady, () => jumpingState);

            fallingState.Add(onGround, () => walkingState);
            jumpingState.Add(jumpEnds, () => fallingState);

            this.currentState = walkingState;
        }

        public void Draw(SpriteBatch s)
        {
            //DEBUG
            //if (this.body.WasCollision)
            //{
            //    var b = this.body.BoundingBox;
            //    s.Draw(R.Textures.Red, b, Color.White);
            //}
            //DEBUG

            this.currentState.Draw(s);
        }

        public void Update(float dt)
        {
            this.currentState = (DrawableState)this.currentState.Update(dt);
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

    internal class FallingState : DrawableState
    {
        public FallingState(RigidBody body, IInputHandler input)
            : base(body, input)
        {
        }

        public override State Update(float gt)
        {
            this.body.AddForce("g", new Vector2(0, 3.5f));

            if (this.input.LeftPressed)
            {
                this.body.AddVelocityComponent("move", new Vector2(-1, 0));
                this.body.LookAt = new Vector2(-1, 0);
            }
            else if (this.input.RightPressed)
            {
                this.body.AddVelocityComponent("move", new Vector2(1, 0));
                this.body.LookAt = new Vector2(1, 0);
            }

            return base.Update(gt);
        }

        protected override string GetTileString()
            => "3,D";
    }

    internal class JumpingState : DrawableState
    {
        public JumpingState(RigidBody body, IInputHandler input)
            : base(body, input)
        {
        }

        public override State Update(float gt)
        {
            this.body.AddForce("g", new Vector2(0, 3.5f));
            this.body.AddVelocityComponent("jump", new Vector2(0, -5));

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
            => "3,C";
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