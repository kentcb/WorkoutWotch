using System;

namespace WorkoutWotch.Models.Events
{
    public sealed class BeforeRepetitionEvent : NumberedEvent
    {
        public BeforeRepetitionEvent(ExecutionContext executionContext, int number)
            : base(executionContext, number)
        {
        }
    }
}

