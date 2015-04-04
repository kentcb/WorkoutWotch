namespace WorkoutWotch.Models.Events
{
    using System.Globalization;
    using Kent.Boogaart.HelperTrinity.Extensions;

    public sealed class AfterExerciseEvent : EventBase
    {
        private readonly Exercise exercise;

        public AfterExerciseEvent(ExecutionContext executionContext, Exercise exercise)
            : base(executionContext)
        {
            exercise.AssertNotNull(nameof(exercise));
            this.exercise = exercise;
        }

        public Exercise Exercise
        {
            get { return this.exercise; }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "After Exercise '{0}'", this.exercise.Name);
        }
    }
}