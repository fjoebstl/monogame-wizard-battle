using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Resources;

namespace SideGamePrototype
{
    internal class Bullet : IEntity
    {
        public IRigidBody Body { get; private set; }

        public bool Dead { get; private set; }

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
                this.Dead = true;
        }

        private PixelShape GetCurrentShape()
            => PixelShape.FromTile(R.Textures.Tiles.GetCollisionTileFromString(GetTileString()));
    }
}