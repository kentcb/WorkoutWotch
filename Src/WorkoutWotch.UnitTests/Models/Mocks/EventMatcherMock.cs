namespace WorkoutWotch.UnitTests.Models.Mocks
{
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Models;

    public sealed class EventMatcherMock : MockBase<IEventMatcher>, IEventMatcher
    {
        public EventMatcherMock(MockBehavior behavior = MockBehavior.Strict)
            : base(behavior)
        {
        }

        public bool Matches(IEvent @event)
        {
            return this.Apply(x => x.Matches(@event));
        }
    }
}