using System;
using WorkoutWotch.Models;
using Kent.Boogaart.PCLMock;

namespace WorkoutWotch.UnitTests.Models.Mocks
{
    public sealed class EventMock: MockBase<IEvent>, IEvent
    {
        public EventMock(MockBehavior behavior = MockBehavior.Strict)
            : base(behavior)
        {
        }

        public ExecutionContext ExecutionContext
        {
            get { return this.Apply(x => x.ExecutionContext); }
        }
    }
}

