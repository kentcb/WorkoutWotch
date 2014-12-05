namespace WorkoutWotch.Models.Events
{
    using System.Globalization;
    using Kent.Boogaart.HelperTrinity.Extensions;

	public sealed class BeforeExerciseEvent : EventBase
	{
        private readonly Exercise exercise;

        public BeforeExerciseEvent(ExecutionContext executionContext, Exercise exercise)
            : base(executionContext)
        {
            exercise.AssertNotNull("exercise");
            this.exercise = exercise;
        }

        public Exercise Exercise
        {
            get { return this.exercise; }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Before Exercise '{0}'", this.exercise.Name);
        }
	}
}