namespace WorkoutWotch.Models.Events
{
    using System.Globalization;

    public sealed class AfterSetEvent : NumberedEvent
    {
        public AfterSetEvent(ExecutionContext executionContext, int number)
            : base(executionContext, number)
        {
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "After Set {0}", this.Number);
        }
    }
}