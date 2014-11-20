namespace WorkoutWotch.UnitTests.Services.ExerciseDocument.Mocks
{
    using System;
    using System.Reactive.Linq;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;

    public sealed class ExerciseDocumentServiceMock : MockBase<IExerciseDocumentService>, IExerciseDocumentService
    {
        public ExerciseDocumentServiceMock(MockBehavior behavior = MockBehavior.Strict)
            : base(behavior)
        {
            if (behavior == MockBehavior.Loose)
            {
                this.When(x => x.ExerciseDocument).Return(Observable.Empty<string>());
            }
        }

        public IObservable<string> ExerciseDocument
        {
            get { return this.Apply(x => x.ExerciseDocument); }
        }
    }
}