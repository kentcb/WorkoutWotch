namespace WorkoutWotch.Models.Events
{
    using System.Globalization;
    using Genesis.Ensure;

    public sealed class BeforeExerciseEvent : EventBase
    {
        private readonly Exercise exercise;

        public BeforeExerciseEvent(ExecutionContext executionContext, Exercise exercise)
            : base(executionContext)
        {
            Ensure.ArgumentNotNull(exercise, nameof(exercise));
            this.exercise = exercise;
        }

        public Exercise Exercise => this.exercise;

        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "Before Exercise '{0}'", this.exercise.Name);
    }
}