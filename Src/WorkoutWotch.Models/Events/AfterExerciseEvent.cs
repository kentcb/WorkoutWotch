namespace WorkoutWotch.Models.Events
{
    using Kent.Boogaart.HelperTrinity.Extensions;

    public sealed class AfterExerciseEvent : EventBase
    {
        private readonly Exercise exercise;

        public AfterExerciseEvent(ExecutionContext executionContext, Exercise exercise)
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