using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Resources;
using System.Linq;

namespace SideGamePrototype
{
    internal class Bullet : IEntity
    {
        public IRigidBody Body { get; private set; }
        public bool Dead { get; set; }

        public Bullet(Vector2 pos)
        {
            this.Body = new RigidBody(pos, GetCurrentShape);
        }

        public void Draw(SpriteBatch s)
        {
            var t = GetTile();
            t.Draw(s, this.Body.Positon);
        }

        public Tile GetTile()
            => R.Textures.Tiles.GetTileFromString(this.GetTileString());

        private string GetTileString()
            => "5,A";

        public void Update(float dt)
        {
            this.Body.Update(dt);

            if (this.Body.LastCollisionResult.WasCollision)
            {
                var w = this.Body.LastCollisionResult
                    .EntityCollisions.Where(e => e is Wizard).FirstOrDefault() as Wizard;
                if (w != null)
                    w.Hit();

                this.Dead = true;
            }
        }

        private Tile GetCurrentShape()
            => R.Textures.Tiles.GetTileFromString(GetTileString());
    }
}