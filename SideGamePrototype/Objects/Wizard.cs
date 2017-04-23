using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SideGamePrototype
{
    internal class Wizard : IEntity
    {
        public IRigidBody Body { get; private set; }
        public bool Dead { get; set; } = false;

        private Stack<State> currentStates;

        public Wizard(Vector2 pos, IInputHandler input)
        {
            this.Body = new RigidBody(pos, GetCurrentShape);

            //States
            var walkingState = new WalkingState(Body, input);
            var fallingState = new FallingState(Body, input);
            var jumpingState = new JumpingState(Body, input);
            var diveState = new DiveDownState(Body, input);
            var fireingState = new FireingState(Body, input);

            //Triggers
            var onGround = Trigger.From(() => this.Body.LastCollisionResult.StandsOnGround);
            var notOnGround = Trigger.From(() => !this.Body.LastCollisionResult.StandsOnGround);
            var jumpReady = Trigger.And(
                Trigger.Delay(0.2f),
                Trigger.From(() => input.JumpPressed));

            var notJumpPressed = Trigger.From(() => !input.JumpPressed);
            var downPressed = Trigger.From(() => input.CrouchPressed);
            var firePressed = Trigger.From(() => input.FirePressed);

            var jumpEnds = Trigger.Or(
                Trigger.Delay(JumpingState.Duration),
                notJumpPressed,
                Trigger.And(
                    Trigger.Delay(0.2f),
                    Trigger.From(() => this.Body.LastCollisionResult.WasCollision)));

            //Transitions
            walkingState.Add(notOnGround, () => fallingState);
            walkingState.Add(jumpReady, () => jumpingState);
            walkingState.AddPush(firePressed, () => fireingState);

            fallingState.Add(onGround, () => walkingState);
            fallingState.Add(downPressed, () => diveState);
            fallingState.AddPush(firePressed, () => fireingState);

            jumpingState.Add(jumpEnds, () => fallingState);
            jumpingState.AddPush(firePressed, () => fireingState);

            diveState.Add(onGround, () => walkingState);

            fireingState.AddPop(Trigger.Delay(0.2f));

            this.currentStates = new Stack<State>();
            this.currentStates.Push(walkingState);
        }

        public void Draw(SpriteBatch s)
        {
            //DEBUG
            if (GameState.DEBUG && this.Body.LastCollisionResult.WasCollision)
            {
                var b = this.Body.BoundingBox;
                s.Draw(R.Textures.Red, b, Color.White);
            }
            //DEBUG

            ((DrawableState)this.currentStates.Peek()).Draw(s);
        }

        public void Update(float dt)
        {
            //Update all pushed down states (but no transitions are made; see -> State.Update)
            var passive = this.currentStates.Reverse().Take(this.currentStates.Count - 1);
            passive.ToList().ForEach(p => p.Update(dt));

            //Get new states
            this.currentStates = this.currentStates.Peek().Update(dt);

            this.Body.Update(dt);
        }

        public void Hit()
        {
            this.Dead = true;
        }

        private Tile GetCurrentShape()
            => ((DrawableState)this.currentStates.Peek()).GetTile();
    }

    internal class WalkingState : DrawableState
    {
        private bool isMoving = false;

        public WalkingState(IRigidBody body, IInputHandler input)
            : base(nameof(WalkingState), body, input)
        {
        }

        public override Stack<State> Update(float gt)
        {
            this.isMoving = this.body.AddMoveComponent(this.input, velocity: 3.0f);

            return base.Update(gt);
        }

        protected override string GetTileString()
            => this.isMoving ? "3,F" : "3,A";
    }

    internal class FallingState : DrawableState
    {
        public FallingState(IRigidBody body, IInputHandler input)
            : base(nameof(FallingState), body, input)
        {
        }

        public override Stack<State> Update(float gt)
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
        public static float Duration = 1.5f;
        public static float Force = 5.0f;
        public static float Mul => Force / Duration;

        private float elapsed = 0.0f;

        public JumpingState(IRigidBody body, IInputHandler input)
            : base(nameof(JumpingState), body, input)
        {
        }

        public override void OnEnter()
        {
            this.elapsed = 0.0f;
            base.OnEnter();
        }

        public override Stack<State> Update(float gt)
        {
            this.elapsed += gt;

            var jumpforce = Math.Max(Force - this.elapsed * Mul, 0);

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
            : base(nameof(DiveDownState), body, input)
        {
        }

        public override Stack<State> Update(float gt)
        {
            this.body.AddForce("g", new Vector2(0, 3.5f));
            this.body.AddVelocityComponent("downforce", new Vector2(0, 3));

            return base.Update(gt);
        }

        protected override string GetTileString()
            => "3,E";
    }

    internal class FireingState : DrawableState
    {
        public FireingState(IRigidBody body, IInputHandler input)
            : base(nameof(FireingState), body, input)
        {
        }

        public override void OnEnter()
        {
            var pos = this.body.BoundingBox.Center.ToVector2() + this.body.LookAt * 16.0f;
            var b = new Bullet(pos - this.body.BoundingBox.Size.ToVector2() / 2.0f);
            b.Body.AddVelocityComponent("b", this.body.LookAt * 3.0f, isConstant: true);

            GameState.Entities.Add(b);
            base.OnEnter();
        }

        public override Stack<State> Update(float gt)
        {
            return base.Update(gt);
        }

        protected override string GetTileString()
            => "3,B";
    }

    [DebuggerDisplay("{name}")]
    internal abstract class DrawableState : State
    {
        private string name;

        protected readonly IRigidBody body;
        protected readonly IInputHandler input;

        protected abstract string GetTileString();

        public DrawableState(string name, IRigidBody body, IInputHandler input)
        {
            this.body = body;
            this.input = input;
            this.name = name;
        }

        public void Draw(SpriteBatch s)
        {
            var t = GetTile();
            t.Draw(s, this.body.Positon, this.body.LookAt.X > 0
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None);
        }

        public override Stack<State> Update(float gt)
        {
            return base.Update(gt);
        }

        public Tile GetTile()
            => R.Textures.Tiles.GetTileFromString(this.GetTileString());
    }

    internal static class RigidBodyEx
    {
        public static bool AddMoveComponent(this IRigidBody body, IInputHandler input, float velocity)
        {
            if (input.LeftPressed)
            {
                body.AddVelocityComponent("move", new Vector2(-velocity, 0));
                body.LookAt = new Vector2(-1, 0);
                return true;
            }
            else if (input.RightPressed)
            {
                body.AddVelocityComponent("move", new Vector2(velocity, 0));
                body.LookAt = new Vector2(1, 0);
                return true;
            }

            return false;
        }
    }
}