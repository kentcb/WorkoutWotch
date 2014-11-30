using System;

namespace WorkoutWotch.Models.Events
{
    public sealed class DuringRepetitionEvent : NumberedEvent
    {
        public DuringRepetitionEvent(ExecutionContext executionContext, int number)
            : base(executionContext, number)
        {
        }
    }
}

