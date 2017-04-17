using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideGamePrototype.GameObjects.Player
{
    internal class BasicTrigger : ISignal
    {
        private readonly Func<bool> trigger;

        public BasicTrigger(Func<bool> trigger)
        {
            this.trigger = trigger;
        }

        public void Update(float gt)
        {
        }

        public bool Triggers()
            => this.trigger();

        public void Reset()
        {
        }
    }

    internal class DelayTrigger : ISignal
    {
        private readonly float targetTime;
        private float time = 0.0f;

        public DelayTrigger(float time)
        {
            this.targetTime = time;
        }

        public void Reset()
        {
            this.time = 0.0f;
        }

        public bool Triggers()
        {
            return this.time >= this.targetTime;
        }

        public void Update(float gt)
        {
            this.time += gt;
        }
    }
}