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
        private bool wasHit = false;

        public Wizard(Vector2 pos, IInputHandler input)
        {
            this.Body = new RigidBody(pos, GetCurrentShape);

            //States
            var walkingState = new WalkingState(this, input);
            var fallingState = new FallingState(this, input);
            var jumpingState = new JumpingState(this, input);
            var diveState = new DiveDownState(this, input);
            var fireingState = new FireingState(this, input);
            var dyingState = new DyingState(this, input);

            //Triggers
            var onGround = Trigger.From(() => this.Body.LastCollisionResult.StandsOnGround);
            var notOnGround = Trigger.From(() => !this.Body.LastCollisionResult.StandsOnGround);
            var jumpReady = Trigger.And(
                Trigger.Delay(0.2f),
                Trigger.From(() => input.JumpPressed));

            var notJumpPressed = Trigger.From(() => !input.JumpPressed);
            var downPressed = Trigger.From(() => input.CrouchPressed);
            var firePressed = Trigger.From(() => input.FirePressed);
            var wasHit = Trigger.From(() => this.wasHit);

            var jumpEnds = Trigger.Or(
                Trigger.Delay(JumpingState.Duration),
                notJumpPressed,
                Trigger.And(
                    Trigger.Delay(0.2f),
                    Trigger.From(() => this.Body.LastCollisionResult.WasCollision)));

            //Transitions
            walkingState.Add(wasHit, () => dyingState);
            walkingState.Add(notOnGround, () => fallingState);
            walkingState.Add(jumpReady, () => jumpingState);
            walkingState.AddPush(firePressed, () => fireingState);

            fallingState.Add(wasHit, () => dyingState);
            fallingState.Add(onGround, () => walkingState);
            fallingState.Add(downPressed, () => diveState);
            fallingState.AddPush(firePressed, () => fireingState);

            jumpingState.Add(wasHit, () => dyingState);
            jumpingState.Add(jumpEnds, () => fallingState);
            jumpingState.AddPush(firePressed, () => fireingState);

            diveState.Add(wasHit, () => dyingState);
            diveState.Add(onGround, () => walkingState);

            fireingState.Add(wasHit, () => dyingState);
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
            this.wasHit = true;
        }

        public CollisionType CollisionType => ((DrawableState)this.currentStates.Peek()).GetTile().CollisionType;

        private ITile GetCurrentShape()
            => ((DrawableState)this.currentStates.Peek()).GetTile();
    }

    internal class WalkingState : DrawableState
    {
        private bool isMoving = false;

        public WalkingState(IEntity entity, IInputHandler input)
            : base(nameof(WalkingState), entity, input)
        {
        }

        public override Stack<State> Update(float gt)
        {
            this.isMoving = this.entity.Body.AddMoveComponent(this.input, velocity: 3.0f);

            return base.Update(gt);
        }

        protected override string GetTileString()
            => this.isMoving ? "3,F" : "3,A";
    }

    internal class FallingState : DrawableState
    {
        public FallingState(IEntity entity, IInputHandler input)
            : base(nameof(FallingState), entity, input)
        {
        }

        public override Stack<State> Update(float gt)
        {
            var body = this.entity.Body;
            body.AddForce("g", new Vector2(0, 3.5f));
            body.AddMoveComponent(this.input, velocity: 1.0f);

            return base.Update(gt);
        }

        protected override string GetTileString()
            => "3,D";
    }

    internal class JumpingState : DrawableState
    {
        public static float Duration = 1.5f;
        public static float Force = 4.0f;
        public static float Mul => Force / Duration;

        private float elapsed = 0.0f;

        public JumpingState(IEntity entity, IInputHandler input)
            : base(nameof(JumpingState), entity, input)
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

            var body = this.entity.Body;
            body.AddForce("g", new Vector2(0, 3.5f));
            body.AddVelocityComponent("jump", new Vector2(0, -jumpforce));
            body.AddMoveComponent(this.input, velocity: 2.0f);

            return base.Update(gt);
        }

        protected override string GetTileString()
            => "3,C";
    }

    internal class DiveDownState : DrawableState
    {
        private float chargeTime = 0.0f;
        private readonly float maxCharge = 0.2f;

        public DiveDownState(IEntity entity, IInputHandler input)
            : base(nameof(DiveDownState), entity, input)
        {
        }

        public override Stack<State> Update(float gt)
        {
            this.chargeTime += gt;

            this.entity.Body.AddForce("g", new Vector2(0, 3.5f));
            this.entity.Body.AddVelocityComponent("downforce", new Vector2(0, 3));

            return base.Update(gt);
        }

        public override void OnEnter()
        {
            this.chargeTime = 0.0f;
            base.OnEnter();
        }

        public override void OnExit()
        {
            if (this.chargeTime >= maxCharge)
            {
                var body = this.entity.Body;
                this.CreateBullet(body.LookAt, 4.0f);
                this.CreateBullet(-body.LookAt, 4.0f);
                this.CreateBullet(new Vector2(-1, -1f), 2.0f);
                this.CreateBullet(new Vector2(1, -1f), 2.0f);
                this.CreateBullet(new Vector2(0, -1.0f), 3.0f);
            }

            this.chargeTime = 0.0f;
            base.OnExit();
        }

        private void CreateBullet(Vector2 dir, float speed)
        {
            var body = this.entity.Body;
            var pos = body.BoundingBox.Center.ToVector2() + dir * 20.0f;
            var b = new Bullet(pos - body.BoundingBox.Size.ToVector2() / 2.0f);
            b.Body.AddVelocityComponent("b", dir * speed, isConstant: true);
            GameState.Entities.Add(b);
        }

        protected override string GetTileString()
            => this.chargeTime >= this.maxCharge ? "3,E" : "3,D";
    }

    internal class DyingState : DrawableState
    {
        private float delay = 0.0f;

        public DyingState(IEntity entity, IInputHandler input)
            : base(nameof(DiveDownState), entity, input)
        {
        }

        public override Stack<State> Update(float gt)
        {
            this.entity.Body.AddForce("g", new Vector2(0, 3.5f));

            this.entity.Body.LookAt *= delay % 0.3f > 0.15f ? -1.0f : 1.0f;

            delay += gt;
            if (delay > 1.0f)
                this.entity.Dead = true;

            return base.Update(gt);
        }

        protected override string GetTileString()
            => "3,G";
    }

    internal class FireingState : DrawableState
    {
        public FireingState(IEntity entity, IInputHandler input)
            : base(nameof(FireingState), entity, input)
        {
        }

        public override void OnEnter()
        {
            var body = this.entity.Body;

            var pos = body.BoundingBox.Center.ToVector2() + body.LookAt * 20.0f;
            var b = new Bullet(pos - body.BoundingBox.Size.ToVector2() / 2.0f);
            b.Body.AddVelocityComponent("b", body.LookAt * 4.0f, isConstant: true);

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

        protected readonly IEntity entity;
        protected readonly IInputHandler input;

        protected abstract string GetTileString();

        public DrawableState(string name, IEntity entity, IInputHandler input)
        {
            this.entity = entity;
            this.input = input;
            this.name = name;
        }

        public void Draw(SpriteBatch s)
        {
            var t = GetTile();
            t.Draw(s, this.entity.Body.Positon, this.entity.Body.LookAt.X > 0
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None);
        }

        public override Stack<State> Update(float gt)
        {
            return base.Update(gt);
        }

        public ITile GetTile()
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