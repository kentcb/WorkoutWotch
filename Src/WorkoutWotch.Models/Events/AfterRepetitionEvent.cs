namespace WorkoutWotch.Models.Events
{
    using System.Globalization;

    public sealed class AfterRepetitionEvent : NumberedEvent
    {
        public AfterRepetitionEvent(ExecutionContext executionContext, int number)
            : base(executionContext, number)
        {
        }

        public override string ToString()
            => string.Format(CultureInfo.InvariantCulture, "After Repetition {0}", this.Number);
    }
}