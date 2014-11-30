using System;

namespace WorkoutWotch.Models.Events
{
    public sealed class AfterSetEvent : NumberedEvent
    {
        public AfterSetEvent(ExecutionContext executionContext, int number)
            : base(executionContext, number)
        {
        }
    }
}

