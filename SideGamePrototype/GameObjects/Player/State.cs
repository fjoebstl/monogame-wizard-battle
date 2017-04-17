using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace SideGamePrototype
{
    internal interface ISignal
    {
        bool Triggers();
        void Update(float gt);
        void Reset();
    }

    internal abstract class State
    {
        private readonly Dictionary<ISignal, Func<State>> signals
            = new Dictionary<ISignal, Func<State>>();

        private bool enterCalled = false;

        public virtual void OnEnter()
        {
            this.enterCalled = true;

            foreach (var s in this.signals.Keys)
                s.Reset();
        }

        public virtual void OnExit()
        {
            this.enterCalled = false;
        }

        public void Add(ISignal signal, Func<State> creator)
        {
            this.signals.Add(signal, creator);
        }

        public virtual State Update(float gt)
        {
            if (!this.enterCalled)
                this.OnEnter();

            foreach (var s in this.signals.Keys)
                s.Update(gt);

            foreach (var kv in this.signals)
            {
                if (kv.Key.Triggers())
                {
                    this.OnExit();
                    return kv.Value();
                }
            }

            return this;
        }
    }

    #region Test

    [TestFixture]
    public class StateTest
    {
        [Test]
        public void NoTrigger_DoesNotTransition()
        {
            var w = new TestState();
            var j = new TestState();

            w.Add(new TestSignal(), () => j);

            var n = w.Update(0.0f);

            Assert.AreEqual(w, n);
            Assert.AreEqual(w.enterCalled, 1);
            Assert.AreEqual(w.exitCalled, 0);
        }

        [Test]
        public void Trigger_DoesTransition()
        {
            var w = new TestState();
            var j = new TestState();
            var signal = new TestSignal() { Value = true };
            var signal2 = new TestSignal() { Value = false };

            w.Add(signal, () => j);
            j.Add(signal2, () => w);

            var n = w.Update(0.0f);

            Assert.AreEqual(j, n);
            Assert.AreEqual(w.enterCalled, 1);
            Assert.AreEqual(w.exitCalled, 1);
            Assert.AreEqual(signal.ResetCalled, 1);
            Assert.AreEqual(signal.UpdateCalled, 1);
            Assert.AreEqual(signal2.UpdateCalled, 0);

            var n2 = n.Update(0.0f);

            Assert.AreEqual(n, n2);
            Assert.AreEqual(j.enterCalled, 1);
            Assert.AreEqual(j.exitCalled, 0);
            Assert.AreEqual(signal2.ResetCalled, 1);
            Assert.AreEqual(signal.ResetCalled, 1);
            Assert.AreEqual(signal.UpdateCalled, 1);
            Assert.AreEqual(signal2.UpdateCalled, 1);
        }

        internal class TestSignal : ISignal
        {
            public bool Value = false;
            public int ResetCalled = 0;
            public int UpdateCalled = 0;

            public void Reset()
            {
                this.ResetCalled++;
            }

            public bool Triggers()
                => Value;

            public void Update(float gt)
            {
                this.UpdateCalled++;
            }
        }

        internal class TestState : State
        {
            public int enterCalled = 0;
            public int exitCalled = 0;

            public override void OnEnter()
            {
                this.enterCalled++;
                base.OnEnter();
            }

            public override void OnExit()
            {
                this.exitCalled++;
                base.OnExit();
            }
        }
    }

    #endregion Test
}