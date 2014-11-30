namespace WorkoutWotch.Models.Events
{
    public sealed class BeforeSetEvent : NumberedEvent
    {
        public BeforeSetEvent(ExecutionContext executionContext, int number)
            : base(executionContext, number)
        {
        }
    }
}