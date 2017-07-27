using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Resources;

namespace SideGamePrototype
{
    public enum CollisionType
    {
        None, Solid, YPassable, OneWay
    }

    public class Tile
    {
        private static readonly Color DEBUG_SOLID = Color.FromNonPremultiplied(0, 0, 255, 200);
        private static readonly Color DEBUG_YPASS = Color.FromNonPremultiplied(0, 255, 0, 200);

        private readonly TextureInfo t;
        private readonly Rectangle r;

        public Vector2 Size { get; private set; }
        public Rectangle CollisionBox { get; private set; }
        public CollisionType CollisionType { get; private set; }
        public bool IsEmpty { get; private set; }

        public string Name { get; set; }

        public Tile(TextureInfo t, Rectangle r)
        {
            this.t = t;
            this.r = r;

            this.Size = new Vector2(r.Width, r.Height);
            this.SetCollisionAttributes();
        }

        public void DrawInTileCoordinates(SpriteBatch s, Vector2 tileCoordinates, SpriteEffects eff = SpriteEffects.None)
        {
            var dest = new Rectangle(
                   (int)tileCoordinates.X * r.Width,
                   (int)tileCoordinates.Y * r.Height,
                   r.Width,
                   r.Height);
            this.Draw(s, dest, eff);
        }

        public void Draw(SpriteBatch s, Vector2 d, SpriteEffects eff = SpriteEffects.None)
        {
            var dest = new Rectangle((int)d.X, (int)d.Y, r.Width, r.Height);
            this.Draw(s, dest, eff);
        }

        public void Draw(SpriteBatch s, Rectangle destination, SpriteEffects eff = SpriteEffects.None)
        {
            s.Draw(
                  texture: this.t.Texture,
                  destinationRectangle: destination,
                  sourceRectangle: r,
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
                    collBox = MathUtil.FlipHorizontal(collBox, this.r.Size.X);
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
                 color: this.CollisionType == CollisionType.Solid ? DEBUG_SOLID : Color.Transparent,
                 rotation: 0.0f,
                 origin: new Vector2(),
                 effects: eff,
                 layerDepth: 1.0f);
            }
            //DEBUG
        }

        private void SetCollisionAttributes()
        {
            Color shapeColor = Color.Transparent;

            int l, t, r, b;
            l = t = r = b = (int)this.Size.X / 2;

            var raw = this.t.Raw;
            var width = this.t.Texture.Width;

            var sourceRect = new Rectangle(this.r.X, this.r.Y + this.r.Height, this.r.Width, this.r.Height);

            this.IsEmpty = true;
            for (int x = 0; x < sourceRect.Width; x++)
            {
                for (int y = 0; y < sourceRect.Height; y++)
                {
                    var pixel = raw.GetPixel(sourceRect.X + x, sourceRect.Y + y, width);
                    if (pixel.A > 0.0f)
                    {
                        shapeColor = pixel;
                        l = x < l ? x : l;
                        t = y < t ? y : t;
                        r = x > r ? x : r;
                        b = y > b ? y : b;

                        this.IsEmpty = false;
                    }
                }
            }

            if (shapeColor == Color.Transparent)
            {
                this.CollisionType = CollisionType.None;
                this.CollisionBox = new Rectangle();
                return;
            }

            this.CollisionBox = new Rectangle(l, t, r - l + 1, b - t + 1);

            if (shapeColor.B > shapeColor.G)
            {
                this.CollisionType = shapeColor.R > shapeColor.B ? CollisionType.OneWay : CollisionType.Solid;
            }
            else
            {
                this.CollisionType = shapeColor.R > shapeColor.G ? CollisionType.OneWay : CollisionType.YPassable;
            }
        }
    }
}