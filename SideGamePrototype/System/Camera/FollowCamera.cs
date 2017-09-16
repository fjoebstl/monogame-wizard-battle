using Microsoft.Xna.Framework;
using Resources;
using System;
using System.Linq;

namespace SideGamePrototype
{
    public static class FollowCamera
    {
        public static void UpdateCamera(float dt)
        {
            //Camera TEST
            var wizards = GameState.Entities.All.OfType<Wizard>().ToList();

            if (wizards.Count >= 1)
            {
                var w1 = wizards[0];
                var viewport = R.System.GraphicsDevice.Viewport;

                var origin = w1.Body.Positon;

                var test = w1.Body.BoundingBox;
                test.Inflate(100, 100);

                if (!test.Contains(GameState.Camera.Position + new Vector2(viewport.Width, viewport.Height) / 2.0f))
                {
                    GameState.Camera.Position -= (GameState.Camera.Position - (origin - new Vector2(viewport.Width, viewport.Height) / 2.0f)) * 0.01f;
                }

                var r = w1.Body.BoundingBox;
                r.Inflate(200, 100);

                GameState.Camera.ZoomWidth(r.Width, 1.0f, 2.0f);
            }
            else
            {
                float x = GameState.Camera.Position.X;
                float y = GameState.Camera.Position.Y;
                float speed = 2.0f;

                if (Math.Abs(x) <= speed)
                    x = 0;
                else
                    x += x > 0 ? -speed : speed;

                if (Math.Abs(y) <= speed)
                    y = 0;
                else
                    y += y > 0 ? -speed : speed;

                GameState.Camera.Position = new Vector2(x, y);

                if (GameState.Camera.Zoom > 1.0f)
                    GameState.Camera.Zoom -= 0.01f;
                else
                    GameState.Camera.Zoom = 1.0f;
            }
            //Camera TEST
        }
    }
}