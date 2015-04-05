namespace WorkoutWotch.Models.Events
{
    using System.Globalization;

    public sealed class BeforeRepetitionEvent : NumberedEvent
    {
        public BeforeRepetitionEvent(ExecutionContext executionContext, int number)
            : base(executionContext, number)
        {
        }

        public override string ToString()
            => string.Format(CultureInfo.InvariantCulture, "Before Repetition {0}", this.Number);
    }
}