namespace WorkoutWotch.Models.Events
{
    using System.Globalization;

    public sealed class DuringRepetitionEvent : NumberedEvent
    {
        public DuringRepetitionEvent(ExecutionContext executionContext, int number)
            : base(executionContext, number)
        {
        }

        public override string ToString()
            => string.Format(CultureInfo.InvariantCulture, "During Repetition {0}", this.Number);
    }
}