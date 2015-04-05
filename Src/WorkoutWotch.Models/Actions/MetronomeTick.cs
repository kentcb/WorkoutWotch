namespace WorkoutWotch.Models.Actions
{
    using System;

    public struct MetronomeTick
    {
        private readonly TimeSpan periodBefore;
        private readonly MetronomeTickType type;

        public MetronomeTick(TimeSpan periodBefore, MetronomeTickType type = MetronomeTickType.Click)
        {
            this.periodBefore = periodBefore;
            this.type = type;
        }

        public TimeSpan PeriodBefore => this.periodBefore;

        public MetronomeTickType Type => this.type;
    }
}