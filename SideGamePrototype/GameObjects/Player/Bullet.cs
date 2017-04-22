using Microsoft.Xna.Framework.Graphics;
using Resources;

namespace SideGamePrototype.GameObjects.Player
{
    internal class Bullet : IEntity
    {
        public IRigidBody Body { get; private set; }

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
        }
    }
}