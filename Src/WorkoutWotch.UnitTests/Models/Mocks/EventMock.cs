namespace WorkoutWotch.UnitTests.Models.Mocks
{
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Models;

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