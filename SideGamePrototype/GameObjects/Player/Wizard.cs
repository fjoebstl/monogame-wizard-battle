using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Resources;
using System;

namespace SideGamePrototype
{
    internal class Wizard : IEntity
    {
        private DrawableState currentState;
        private IRigidBody body;

        public Wizard(Vector2 pos, IInputHandler input, ICollision collision)
        {
            this.body = new RigidBody(pos, GetCurrentShape, collision);

            //States
            var walkingState = new WalkingState(body, input);
            var fallingState = new FallingState(body, input);
            var jumpingState = new JumpingState(body, input);
            var diveState = new DiveDownState(body, input);

            //Triggers
            var onGround = new BasicTrigger(() => collision.StandsOnGround(this.body.BoundingBox));
            var notOnGround = new BasicTrigger(() => !collision.StandsOnGround(this.body.BoundingBox));
            var jumpReady = CombinedTrigger.And(new DelayTrigger(0.2f), new BasicTrigger(() => input.JumpPressed));
            var notJumpPressed = new BasicTrigger(() => !input.JumpPressed);
            var downPressed = new BasicTrigger(() => input.CrouchPressed);
            var jumpEnds = CombinedTrigger.Or(new DelayTrigger(0.3f), notJumpPressed);

            walkingState.Add(notOnGround, () => fallingState);
            walkingState.Add(jumpReady, () => jumpingState);

            fallingState.Add(onGround, () => walkingState);
            fallingState.Add(downPressed, () => diveState);

            jumpingState.Add(jumpEnds, () => fallingState);

            diveState.Add(onGround, () => walkingState);

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
        public WalkingState(IRigidBody body, IInputHandler input)
            : base(body, input)
        {
        }

        public override State Update(float gt)
        {
            this.body.AddMoveComponent(this.input, velocity: 3.0f);

            return base.Update(gt);
        }

        protected override string GetTileString()
            => "3,A";
    }

    internal class FallingState : DrawableState
    {
        public FallingState(IRigidBody body, IInputHandler input)
            : base(body, input)
        {
        }

        public override State Update(float gt)
        {
            this.body.AddForce("g", new Vector2(0, 3.5f));
            this.body.AddMoveComponent(this.input, velocity: 1.0f);

            return base.Update(gt);
        }

        protected override string GetTileString()
            => "3,D";
    }

    internal class JumpingState : DrawableState
    {
        private float elapsed = 0.0f;

        public JumpingState(IRigidBody body, IInputHandler input)
            : base(body, input)
        {
        }

        public override void OnEnter()
        {
            this.elapsed = 0.0f;
            base.OnEnter();
        }

        public override State Update(float gt)
        {
            this.elapsed += gt;

            var jumpforce = Math.Max(5 - this.elapsed * 4, 0);

            this.body.AddForce("g", new Vector2(0, 3.5f));
            this.body.AddVelocityComponent("jump", new Vector2(0, -jumpforce));
            this.body.AddMoveComponent(this.input, velocity: 3.0f);

            return base.Update(gt);
        }

        protected override string GetTileString()
            => "3,C";
    }

    internal class DiveDownState : DrawableState
    {
        public DiveDownState(IRigidBody body, IInputHandler input)
            : base(body, input)
        {
        }

        public override State Update(float gt)
        {
            this.body.AddForce("g", new Vector2(0, 3.5f));
            this.body.AddVelocityComponent("downforce", new Vector2(0, 3));

            return base.Update(gt);
        }

        protected override string GetTileString()
            => "3,E";
    }

    internal abstract class DrawableState : State
    {
        protected readonly IRigidBody body;
        protected readonly IInputHandler input;

        protected abstract string GetTileString();

        public DrawableState(IRigidBody body, IInputHandler input)
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

    internal static class RigidBodyEx
    {
        public static void AddMoveComponent(this IRigidBody body, IInputHandler input, float velocity)
        {
            if (input.LeftPressed)
            {
                body.AddVelocityComponent("move", new Vector2(-velocity, 0));
                body.LookAt = new Vector2(-1, 0);
            }
            else if (input.RightPressed)
            {
                body.AddVelocityComponent("move", new Vector2(velocity, 0));
                body.LookAt = new Vector2(1, 0);
            }
        }
    }
}