using System;

namespace SideGamePrototype
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

    internal class CombinedTrigger : ISignal
    {
        private readonly ISignal a;
        private readonly ISignal b;
        private readonly Func<bool, bool, bool> op;

        public static CombinedTrigger Or(ISignal a, ISignal b)
            => new CombinedTrigger(a, b, (j, k) => j || k);

        public static CombinedTrigger And(ISignal a, ISignal b)
           => new CombinedTrigger(a, b, (j, k) => j && k);

        public CombinedTrigger(ISignal a, ISignal b, Func<bool, bool, bool> op)
        {
            this.a = a;
            this.b = b;
            this.op = op;
        }

        public void Reset()
        {
            this.a.Reset();
            this.b.Reset();
        }

        public bool Triggers()
            => this.op(this.a.Triggers(), this.b.Triggers());

        public void Update(float gt)
        {
            this.a.Update(gt);
            this.b.Update(gt);
        }
    }
}