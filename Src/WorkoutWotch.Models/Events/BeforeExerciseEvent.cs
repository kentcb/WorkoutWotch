namespace WorkoutWotch.Models.Events
{
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
	}
}