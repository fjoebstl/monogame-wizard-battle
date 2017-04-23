﻿using Microsoft.Xna.Framework;
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
        public Vector2 LookAt { get; set; } = new Vector2();
        public CollisionResult LastCollisionResult { get; private set; } = new CollisionResult();

        public Rectangle BoundingBox { get; private set; } = new Rectangle();

        public PixelShape Shape { get; private set; }

        private readonly Func<PixelShape> getCurrentShape;
        private readonly Dictionary<string, ForceComponent> forces = new Dictionary<string, ForceComponent>();

        public RigidBody(Vector2 position, Func<PixelShape> getCurrentShape)
        {
            this.Positon = position;
            this.getCurrentShape = getCurrentShape;
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
            this.Shape = this.getCurrentShape();
            this.BoundingBox = new Rectangle(
           (int)this.Positon.X,
           (int)this.Positon.Y,
           (int)this.Shape.Size.X,
           (int)this.Shape.Size.Y);

            var pos = this.Positon;
            foreach (var force in this.forces.Values)
            {
                force.vel += force.acc * dt;
                pos += force.vel;
            }

            this.Velocity = pos - this.Positon;

            CollisionResult collResult = new CollisionResult();

            do
            {
                collResult = GameState.Collision.Move(this, pos);
                if (collResult == null)
                {
                    collResult = this.LastCollisionResult;
                    break;
                }

                if (collResult.WasCollision)
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

            this.LastCollisionResult = collResult;

            //ToDo: should not be calculated twice
            //this.LastCollisionResult.StandsOnGround = collResult.StandsOnGround;//GameState.Collision.Move(this, this.Positon).StandsOnGround;
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

    public class PixelShape
    {
        public IEnumerable<Point> SolidPixels { get; set; }
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