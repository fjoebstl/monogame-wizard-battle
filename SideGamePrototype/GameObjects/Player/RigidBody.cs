using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SideGamePrototype
{
    internal interface IRigidBody
    {
        Rectangle BoundingBox { get; }
        PixelShape Shape { get; }

        Vector2 LookAt { get; set; }
        Vector2 Positon { get; set; }
        CollisionResult LastCollisionResult { get; }

        void AddForce(string name, Vector2 f);
        void AddVelocityComponent(string name, Vector2 f);
        void Update(float dt);
    }

    internal class RigidBody : IRigidBody
    {
        private class ForceComponent
        {
            public Vector2 acc = new Vector2();
            public Vector2 vel = new Vector2();
            public bool isVelocityComponent = false;
        }

        public Vector2 Positon { get; set; }
        public Vector2 LookAt { get; set; } = new Vector2();
        public CollisionResult LastCollisionResult { get; private set; } = new CollisionResult();

        public Rectangle BoundingBox => new Rectangle(
            (int)this.Positon.X,
            (int)this.Positon.Y,
            (int)this.getCurrentShape().Size.X,
            (int)this.getCurrentShape().Size.Y);

        public PixelShape Shape => this.getCurrentShape();

        private readonly ICollision collision;
        private readonly Func<PixelShape> getCurrentShape;
        private readonly Dictionary<string, ForceComponent> forces = new Dictionary<string, ForceComponent>();

        public RigidBody(Vector2 position, Func<PixelShape> getCurrentShape, ICollision collision)
        {
            this.Positon = position;
            this.collision = collision;
            this.getCurrentShape = getCurrentShape;
        }

        public void AddForce(string name, Vector2 f)
        {
            this.EnsureExists(name);
            this.forces[name].acc = f;
        }

        public void AddVelocityComponent(string name, Vector2 f)
        {
            this.EnsureExists(name);
            this.forces[name].vel = f;
            this.forces[name].isVelocityComponent = true;
        }

        public void Update(float dt)
        {
            var pos = this.Positon;
            foreach (var force in this.forces.Values)
            {
                force.vel += force.acc * dt;
                pos += force.vel;
            }

            CollisionResult collResult = new CollisionResult();

            do
            {
                collResult = this.collision.Move(this, pos);
                pos = this.Positon + (pos - this.Positon) / 2.0f;
            } while (collResult.WasCollision && Math.Abs((pos - this.Positon).Length()) > 0.5f);

            //collResult = this.collision.Move(this, pos);
            //if (collResult.WasCollision)
            //{
            //    pos = this.Positon - (pos - this.Positon) / 2.0f;
            //    ResetForces(collResult);
            //    collResult = this.collision.Move(this, pos);
            //}

            if (!collResult.WasCollision)
            {
                this.Positon = pos;
            }

            ResetForces(collResult);

            this.LastCollisionResult.CollisionPoints = collResult.CollisionPoints;
            this.LastCollisionResult.StandsOnGround = this.collision.Move(this, this.Positon).StandsOnGround;
        }

        private void ResetForces(CollisionResult collResult)
        {
            foreach (var force in this.forces.Values)
            {
                force.acc = new Vector2();
                if (collResult.WasCollision || force.isVelocityComponent)
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

    public class PixelShape
    {
        public IEnumerable<Vector2> SolidPixels { get; set; }
        public Vector2 Size { get; set; }

        public static PixelShape FromTile(Tile t)
        {
            return new PixelShape()
            {
                Size = t.Size,
                SolidPixels = t.GetSolidPoints(),
            };
        }
    }
}