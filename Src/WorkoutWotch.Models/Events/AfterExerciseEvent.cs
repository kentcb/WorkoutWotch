namespace WorkoutWotch.Models.Events
{
    using System.Globalization;
    using Genesis.Ensure;

    public sealed class AfterExerciseEvent : EventBase
    {
        private readonly Exercise exercise;

        public AfterExerciseEvent(ExecutionContext executionContext, Exercise exercise)
            : base(executionContext)
        {
            Ensure.ArgumentNotNull(exercise, nameof(exercise));
            this.exercise = exercise;
        }

        public Exercise Exercise => this.exercise;

        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "After Exercise '{0}'", this.exercise.Name);
    }
}