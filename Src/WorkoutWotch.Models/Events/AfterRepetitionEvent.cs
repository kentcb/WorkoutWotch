using System;

namespace WorkoutWotch.Models.Events
{
    public sealed class AfterRepetitionEvent : NumberedEvent
    {
        public AfterRepetitionEvent(ExecutionContext executionContext, int number)
            : base(executionContext, number)
        {
        }
    }
}

