namespace WorkoutWotch.UnitTests.Models.Actions.Builders
{
    using Genesis.TestUtil;
    using PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.UnitTests.Models.Mocks;

    public sealed class DoNotAwaitActionBuilder : IBuilder
    {
        private IAction innerAction;

        public DoNotAwaitActionBuilder()
        {
            this.innerAction = new ActionMock(MockBehavior.Loose);
        }

        public DoNotAwaitActionBuilder WithInnerAction(IAction innerAction) =>
            this.With(ref this.innerAction, innerAction);

        public DoNotAwaitAction Build() =>
            new DoNotAwaitAction(
                this.innerAction);

        public static implicit operator DoNotAwaitAction(DoNotAwaitActionBuilder builder) =>
            builder.Build();
    }
}