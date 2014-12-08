using System;

namespace WorkoutWotch.Models.Actions
{
    public struct MetronomeTick
    {
        private readonly TimeSpan periodBefore;
        private readonly MetronomeTickType type;

        public MetronomeTick(TimeSpan periodBefore, MetronomeTickType type = MetronomeTickType.Click)
        {
            this.periodBefore = periodBefore;
            this.type = type;
        }

        public TimeSpan PeriodBefore
        {
            get{return this.periodBefore;}
        }

        public MetronomeTickType Type
        {
            get{return this.type;}
        }
    }
}

