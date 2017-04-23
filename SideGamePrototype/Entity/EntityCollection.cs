using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace SideGamePrototype
{
    internal interface IEntityCollection : IDraw, IUpdate
    {
        List<IEntity> All { get; }
        void Add(IEntity e);
    }

    internal class EntityCollection : IEntityCollection
    {
        public List<IEntity> All { get; private set; } = new List<IEntity>();
        private List<IEntity> toAdd = new List<IEntity>();

        public void Add(IEntity e)
        {
            this.toAdd.Add(e);
        }

        public void Update(float dt)
        {
            this.All.AddRange(this.toAdd);
            this.toAdd.Clear();

            this.All = this.All.Where(e => !e.Dead).ToList();

            this.All.ForEach(e => e.Update(dt));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            this.All.ForEach(e => e.Draw(spriteBatch));
        }
    }
}