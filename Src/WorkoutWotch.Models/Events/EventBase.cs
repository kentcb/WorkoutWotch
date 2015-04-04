namespace WorkoutWotch.Models.Events
{
    using Kent.Boogaart.HelperTrinity.Extensions;

    public abstract class EventBase : IEvent
    {
        private readonly ExecutionContext executionContext;

        protected EventBase(ExecutionContext executionContext)
        {
            executionContext.AssertNotNull(nameof(executionContext));
            this.executionContext = executionContext;
        }

        public ExecutionContext ExecutionContext
        {
            get { return this.executionContext; }
        }
    }
}