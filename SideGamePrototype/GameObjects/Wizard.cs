using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideGamePrototype
{
    public class Wizard : IEntity
    {
        private enum State
        {
            Standing, Jumping, Falling, Shooting
        }

        private static readonly Dictionary<State, string> stateMap = new Dictionary<State, string>
        {
            { State.Standing, "3,A" },
            { State.Shooting, "3,B" },
            { State.Jumping, "3,C" },
            { State.Falling, "3,D" },
        };

        private readonly IInputHandler input;

        private Vector2 pos;
        private Vector2 vel = new Vector2();
        private Vector2 acc = new Vector2();
        private bool lastDirLeft = false;
        private State state = State.Standing;

        private ICollision collision;

        private Rectangle BoundingBox => new Rectangle((int)this.pos.X, (int)this.pos.Y, 16, 16);

        public Wizard(Vector2 pos, IInputHandler input, ICollision collision)
        {
            this.input = input;
            this.pos = pos;
            this.collision = collision;
        }

        public void Update(float dt)
        {
            //this.acc = new Vector2();
            var horizontalVel = new Vector2();

            var onground = this.collision.StandsOnGround(this.BoundingBox);
            if (onground)
            {
                this.state = State.Standing;
            }
            else
            {
                //gravity
                this.acc += new Vector2(0, 3) * dt;
            }

            if (this.input.LeftPressed)
            {
                horizontalVel = new Vector2(-3, 0);
                this.lastDirLeft = true;
            }
            else if (this.input.RightPressed)
            {
                horizontalVel = new Vector2(3, 0);
                this.lastDirLeft = false;
            }

            if (this.input.JumpPressed && onground)
            {
                this.acc = new Vector2(0, -0.65f);
                this.state = State.Jumping;
            }

            this.vel += Limit(this.acc, 3);
            this.vel = Limit(this.vel, 5);

            Debug.WriteLine(acc);
            Debug.WriteLine(vel);

            var totalVel = this.vel + horizontalVel;
            if (this.collision.Move(ref this.pos, this.pos + totalVel, this.GetTile()))
            {
                this.acc = new Vector2();
                this.vel = new Vector2();
            }
        }

        private Vector2 Limit(Vector2 vel, int limit)
        {
            vel.X = vel.X < -limit || vel.X > limit ? limit * Math.Sign(vel.X) : vel.X;
            vel.Y = vel.Y < -limit || vel.Y > limit ? limit * Math.Sign(vel.Y) : vel.Y;

            return vel;
        }

        public void Draw(SpriteBatch s)
        {
            var t = GetTile();
            t.Draw(s, this.pos, !lastDirLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        }

        private Tile GetTile()
        {
            return R.Textures.Tiles.GetTileFromString(stateMap[this.state]);
        }
    }
}