using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        private enum Operation
        {
            Transition, Pop, Push
        }

        private readonly Dictionary<ISignal, Tuple<Operation, Func<State>>> signals
            = new Dictionary<ISignal, Tuple<Operation, Func<State>>>();

        protected Stack<State> stateStack = new Stack<State>();

        private bool enterCalled = false;

        public State()
        {
            this.stateStack.Push(this);
        }

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
            this.signals.Add(signal, ToTuple(Operation.Transition, creator));
        }

        public void AddPush(ISignal signal, Func<State> creator)
        {
            this.signals.Add(signal, ToTuple(Operation.Push, creator));
        }

        public void AddPop(ISignal signal)
        {
            this.signals.Add(signal, ToTuple(Operation.Pop, () => null));
        }

        public virtual Stack<State> Update(float gt)
        {
            if (!this.enterCalled)
                this.OnEnter();

            if (this.stateStack.Peek() != this)
                return null;

            foreach (var s in this.signals.Keys)
                s.Update(gt);

            foreach (var kv in this.signals)
            {
                if (kv.Key.Triggers())
                {
                    this.OnExit();

                    var op = kv.Value.Item1;
                    var creator = kv.Value.Item2;

                    if (op != Operation.Push)
                        this.stateStack.Pop();
                    if (op == Operation.Pop)
                        return this.stateStack;

                    var newState = creator();
                    newState.stateStack = this.stateStack;
                    this.stateStack.Push(newState);

                    return this.stateStack;
                }
            }

            return this.stateStack;
        }

        private static Tuple<Operation, Func<State>> ToTuple(Operation o, Func<State> f)
            => new Tuple<Operation, Func<State>>(o, f);
    }

    #region Test

    [TestFixture]
    public class StateTest
    {
        [Test]
        public void NoTrigger_DoesNotTransition()
        {
            var w = new TestState("w");
            var j = new TestState("j");

            w.Add(new TestSignal(), () => j);

            var n = w.Update(0.0f);

            Assert.AreEqual(w, n.Peek());
            Assert.AreEqual(w.enterCalled, 1);
            Assert.AreEqual(w.exitCalled, 0);
        }

        [Test]
        public void Trigger_DoesTransition()
        {
            var w = new TestState("w");
            var j = new TestState("j");
            var signal = new TestSignal() { Value = true };
            var signal2 = new TestSignal() { Value = false };

            w.Add(signal, () => j);
            j.Add(signal2, () => w);

            var n = w.Update(0.0f);

            Assert.AreEqual(j, n.Peek());
            Assert.AreEqual(w.enterCalled, 1);
            Assert.AreEqual(w.exitCalled, 1);
            Assert.AreEqual(signal.ResetCalled, 1);
            Assert.AreEqual(signal.UpdateCalled, 1);
            Assert.AreEqual(signal2.UpdateCalled, 0);

            var n2 = n.Peek().Update(0.0f);

            Assert.AreEqual(n.Peek(), n2.Peek());
            Assert.AreEqual(j.enterCalled, 1);
            Assert.AreEqual(j.exitCalled, 0);
            Assert.AreEqual(signal2.ResetCalled, 1);
            Assert.AreEqual(signal.ResetCalled, 1);
            Assert.AreEqual(signal.UpdateCalled, 1);
            Assert.AreEqual(signal2.UpdateCalled, 1);
        }

        [Test]
        public void Trigger_DoesTransitionX()
        {
            var w = new TestState("w");
            var j = new TestState("j");
            var signal = new TestSignal() { Value = true };
            var signal2 = new TestSignal() { Value = false };

            w.AddPush(signal, () => j);

            var n = w.Update(0.0f);
            var n2 = n.Peek().Update(0.0f);

            Assert.AreEqual(2, n.Count);
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

        [DebuggerDisplay("{name}")]
        internal class TestState : State
        {
            public int enterCalled = 0;
            public int exitCalled = 0;

            public string name;

            public TestState(string n)
            {
                this.name = n;
            }

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