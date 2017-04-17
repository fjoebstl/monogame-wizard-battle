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
        bool WasCollision { get; }

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
        public bool WasCollision { get; private set; }

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
            var wasCollition = false;
            foreach (var force in this.forces.Values)
            {
                force.vel += force.acc * dt;

                var pos = this.Positon;

                var coll = this.collision.Move(ref pos, pos + force.vel, this.getCurrentShape());

                force.acc = new Vector2();
                if (coll || force.isVelocityComponent)
                {
                    force.vel = new Vector2();
                }

                wasCollition = wasCollition || coll;

                this.Positon = pos;
            }

            this.WasCollision = wasCollition;
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