﻿using Microsoft.Xna.Framework.Graphics;

namespace SideGamePrototype
{
    public interface IUpdate
    {
        void Update(float dt);
    }

    public interface IDraw
    {
        void Draw(SpriteBatch s);
    }

    internal interface IEntity : IUpdate, IDraw
    {
    }
}