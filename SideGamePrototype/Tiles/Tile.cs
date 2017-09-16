using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Resources;

namespace SideGamePrototype
{
    public enum CollisionType
    {
        None, Solid, YPassable, OneWay
    }

    public interface ITile
    {
        string Name { get; }
        Vector2 Size { get; }

        Rectangle CollisionBox { get; }
        CollisionType CollisionType { get; }

        void Draw(SpriteBatch s, Vector2 d, SpriteEffects eff = SpriteEffects.None);
        void DrawInTileCoordinates(SpriteBatch s, Vector2 tileCoordinates, SpriteEffects eff = SpriteEffects.None);
    }

    public class Tile : ITile
    {
        private static readonly Color DEBUG_SOLID = Color.FromNonPremultiplied(0, 0, 255, 180);
        private static readonly Color DEBUG_ONEWAY = Color.FromNonPremultiplied(255, 0, 0, 180);
        private static readonly Color DEBUG_YPASS = Color.FromNonPremultiplied(0, 255, 0, 180);

        private readonly TileSource tileSource;
        private readonly Rectangle sourceRect;

        public Vector2 Size { get; private set; }
        public Rectangle CollisionBox { get; private set; }
        public CollisionType CollisionType { get; private set; }

        public string Name { get; }

        public Tile(string name, TileSource tileSource, Rectangle sourceRect, Rectangle collisionBox, CollisionType collisionType)
        {
            this.Name = name;
            this.tileSource = tileSource;
            this.sourceRect = sourceRect;

            this.Size = new Vector2(sourceRect.Width, sourceRect.Height);
            this.CollisionBox = collisionBox;
            this.CollisionType = collisionType;
        }

        public void DrawInTileCoordinates(SpriteBatch s, Vector2 tileCoordinates, SpriteEffects eff = SpriteEffects.None)
        {
            var dest = new Rectangle(
                   (int)tileCoordinates.X * sourceRect.Width,
                   (int)tileCoordinates.Y * sourceRect.Height,
                   sourceRect.Width,
                   sourceRect.Height);
            this.Draw(s, dest, eff);
        }

        public void Draw(SpriteBatch s, Vector2 d, SpriteEffects eff = SpriteEffects.None)
        {
            var dest = new Rectangle((int)d.X, (int)d.Y, sourceRect.Width, sourceRect.Height);
            this.Draw(s, dest, eff);
        }

        private void Draw(SpriteBatch s, Rectangle destination, SpriteEffects eff = SpriteEffects.None)
        {
            s.Draw(
                  texture: this.tileSource.Texture,
                  destinationRectangle: destination,
                  sourceRectangle: sourceRect,
                  color: Color.White,
                  rotation: 0.0f,
                  origin: new Vector2(),
                  effects: eff,
                  layerDepth: 1.0f);

            //DEBUG
            if (GameState.DEBUG)
            {
                var collBox = this.CollisionBox;

                if (eff == SpriteEffects.FlipHorizontally)
                {
                    collBox = MathUtil.FlipHorizontal(collBox, this.sourceRect.Size.X);
                }

                var box = new Rectangle(
                     destination.X + collBox.X,
                     destination.Y + collBox.Y,
                     collBox.Width,
                     collBox.Height);

                s.Draw(
                 texture: R.Textures.White,
                 destinationRectangle: box,
                 sourceRectangle: new Rectangle(),
                 color: this.CollisionType == CollisionType.Solid ? DEBUG_SOLID :
                        this.CollisionType == CollisionType.OneWay ? DEBUG_ONEWAY :
                        Color.Transparent,
                 rotation: 0.0f,
                 origin: new Vector2(),
                 effects: eff,
                 layerDepth: 1.0f);
            }
            //DEBUG
        }
    }
}