namespace WorkoutWotch.UnitTests.ReactiveUI.Mocks
{
    using Kent.Boogaart.PCLMock;
    using global::ReactiveUI;

    public sealed class ScreenMock : MockBase<IScreen>, IScreen
    {
        public ScreenMock(MockBehavior behavior = MockBehavior.Strict)
            : base(behavior)
        {
            if (behavior == MockBehavior.Loose)
            {
                this.ConfigureLooseBehavior();
            }
        }

        private void ConfigureLooseBehavior()
        {
            this
                .When(x => x.Router)
                .Return(new RoutingState());
        }

        public RoutingState Router => this.Apply(x => x.Router);
    }
}