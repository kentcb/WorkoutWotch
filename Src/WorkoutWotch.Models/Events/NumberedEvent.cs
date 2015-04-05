namespace WorkoutWotch.Models.Events
{
    using System;

    public abstract class NumberedEvent : EventBase
    {
        private readonly int number;

        protected NumberedEvent(ExecutionContext executionContext, int number)
            : base(executionContext)
        {
            if (number < 0)
            {
                throw new ArgumentException("number must be greater than or equal to zero.", "number");
            }

            this.number = number;
        }

        public int Number => this.number;
    }
}