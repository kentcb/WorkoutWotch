namespace WorkoutWotch.Models.Events
{
    using System.Globalization;

    public sealed class BeforeSetEvent : NumberedEvent
    {
        public BeforeSetEvent(ExecutionContext executionContext, int number)
            : base(executionContext, number)
        {
        }

        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "Before Set {0}", this.Number);
    }
}