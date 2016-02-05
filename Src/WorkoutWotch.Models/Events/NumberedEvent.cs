namespace WorkoutWotch.Models.Events
{
    using Utility;

    public abstract class NumberedEvent : EventBase
    {
        private readonly int number;

        protected NumberedEvent(ExecutionContext executionContext, int number)
            : base(executionContext)
        {
            Ensure.ArgumentCondition(number >= 0, "number must be greater than or equal to zero.", "number");

            this.number = number;
        }

        public int Number => this.number;
    }
}