using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Resources;
using System;
using System.Linq;

namespace SideGamePrototype
{
    internal class Camera2D
    {
        private Viewport viewport;

        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public float Zoom { get; set; }
        public Vector2 Origin { get; set; }

        public Camera2D(Viewport viewport)
        {
            this.viewport = viewport;

            Rotation = 0;
            Zoom = 1;
            Origin = new Vector2(viewport.Width / 2f, viewport.Height / 2f);
            Position = new Vector2(0, 0);
        }

        public BoundingFrustum GetBoundingFrustum()
        {
            var viewMatrix = GetVirtualViewMatrix();
            var projectionMatrix = GetProjectionMatrix(viewMatrix);
            return new BoundingFrustum(projectionMatrix);
        }

        public Rectangle GetBoundingRectangle()
        {
            var r = this.Rotation;
            this.Rotation = 0;
            var frustum = GetBoundingFrustum();
            var corners = frustum.GetCorners();
            var topLeft = corners[0];
            var bottomRight = corners[2];
            var width = bottomRight.X - topLeft.X;
            var height = bottomRight.Y - topLeft.Y;
            this.Rotation = r;
            return new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)width, (int)height);
        }

        public Matrix GetViewMatrix()
        {
            return
                Matrix.CreateTranslation(new Vector3(-this.PositionClamped, 0.0f)) *
                Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(Zoom, Zoom, 1) *
                Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
        }

        private Matrix GetVirtualViewMatrix(Vector2 parallaxFactor)
        {
            return
                Matrix.CreateTranslation(new Vector3(-this.PositionClamped * parallaxFactor, 0.0f)) *
                Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(Zoom, Zoom, 1) *
                Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
        }

        public void ZoomWidth(float width, float min, float max)
        {
            var z = (this.viewport.Width / (float)width);
            this.Zoom = Math.Max(min, Math.Min(z, max));
        }

        private Matrix GetVirtualViewMatrix()
        {
            return GetVirtualViewMatrix(Vector2.One);
        }

        private Matrix GetProjectionMatrix(Matrix viewMatrix)
        {
            var projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, -1, 0);
            Matrix.Multiply(ref viewMatrix, ref projection, out projection);
            return projection;
        }

        //Avoids tile tearing
        private Vector2 PositionClamped => new Vector2((float)Math.Floor(Position.X), (float)Math.Floor(Position.Y));

        public void MoveCamera(int x, int y, float gt)
        {
            this.Position += new Vector2(x, y) * gt;
        }
    }
}