using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Resources;
using System.Linq;

namespace SideGamePrototype
{
    internal class Spike : IEntity
    {
        public IRigidBody Body { get; private set; }
        public bool Dead { get; set; }

        private bool topSpike = false;

        public Spike(Vector2 pos, bool topSpike = false)
        {
            this.Body = new RigidBody(pos, GetCurrentShape);
            this.Body.IsStatic = true;
            this.topSpike = topSpike;
        }

        public void Draw(SpriteBatch s)
        {
            var t = GetTile();
            t.Draw(s, this.Body.Positon);
        }

        public ITile GetTile()
            => R.Textures.Tiles.GetTileFromString(this.GetTileString());

        private string GetTileString()
            => this.topSpike ? "7,B" : "7,A";

        public void Update(float dt)
        {
            this.Body.Update(dt);

            if (this.Body.LastCollisionResult.WasCollision)
            {
                var w = this.Body.LastCollisionResult
                    .EntityCollisions.Where(e => e is Wizard).FirstOrDefault() as Wizard;
                if (w != null)
                    w.Hit();
            }
        }

        public CollisionType CollisionType => GetTile().CollisionType;

        private ITile GetCurrentShape()
            => R.Textures.Tiles.GetTileFromString(GetTileString());
    }
}