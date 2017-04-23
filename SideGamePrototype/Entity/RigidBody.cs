using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SideGamePrototype
{
    internal interface IRigidBody
    {
        Rectangle BoundingBox { get; }

        Vector2 LookAt { get; set; }
        Vector2 Positon { get; set; }
        CollisionResult LastCollisionResult { get; }

        Vector2 Velocity { get; }

        void AddForce(string name, Vector2 f, bool isConstant = false);
        void AddVelocityComponent(string name, Vector2 f, bool isConstant = false);
        void Update(float dt);
    }

    internal class RigidBody : IRigidBody
    {
        private class ForceComponent
        {
            public Vector2 acc = new Vector2();
            public Vector2 vel = new Vector2();
            public bool isVelocityComponent = false;
            public bool isConstant = false;
        }

        public Vector2 Velocity { get; private set; } = new Vector2();
        public Vector2 Positon { get; set; }
        public Vector2 LookAt { get; set; } = new Vector2(1, 0);
        public CollisionResult LastCollisionResult { get; private set; } = new CollisionResult();

        public Rectangle BoundingBox { get; private set; } = new Rectangle();

        private Tile tile { get; set; }
        private readonly Func<Tile> getCurrentTile;
        private readonly Dictionary<string, ForceComponent> forces = new Dictionary<string, ForceComponent>();

        public RigidBody(Vector2 position, Func<Tile> getCurrentTile)
        {
            this.Positon = position;
            this.getCurrentTile = getCurrentTile;
        }

        public void AddForce(string name, Vector2 f, bool isConstant = false)
        {
            this.EnsureExists(name);
            this.forces[name].acc = f;
            this.forces[name].isConstant = isConstant;
        }

        public void AddVelocityComponent(string name, Vector2 f, bool isConstant = false)
        {
            this.EnsureExists(name);
            this.forces[name].vel = f;
            this.forces[name].isVelocityComponent = true;
            this.forces[name].isConstant = isConstant;
        }

        public void Update(float dt)
        {
            this.tile = this.getCurrentTile();

            var bb = this.tile.CollisionBox;
            if (this.LookAt.X > 0)
                bb = MathUtil.FlipHorizontal(bb, (int)this.tile.Size.X);

            this.BoundingBox = new Rectangle(
                (int)this.Positon.X + bb.X,
                (int)this.Positon.Y + bb.Y,
                bb.Width,
                bb.Height);

            var pos = this.Positon;
            foreach (var force in this.forces.Values)
            {
                force.vel += force.acc * dt;
                pos += force.vel;
            }

            this.Velocity = pos - this.Positon;

            var collResult = GameState.Collision.Move(this, pos);

            if (collResult != null)
            {
                this.Positon = collResult.AvailablePosition;
                this.LastCollisionResult = collResult;
            }

            this.ResetForces(this.LastCollisionResult);
        }

        private void ResetForces(CollisionResult collResult)
        {
            foreach (var force in this.forces.Values)
            {
                force.acc = new Vector2();
                if (collResult.WasCollision || (force.isVelocityComponent && !force.isConstant))
                {
                    force.vel = new Vector2();
                }
            }
        }

        private void EnsureExists(string name)
        {
            if (!this.forces.ContainsKey(name))
            {
                this.forces.Add(name, new ForceComponent());
            }
        }
    }
}