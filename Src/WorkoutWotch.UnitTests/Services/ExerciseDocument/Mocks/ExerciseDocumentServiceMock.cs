namespace WorkoutWotch.UnitTests.Services.ExerciseDocument.Mocks
{
    using System.Reactive.Linq;

    public sealed partial class ExerciseDocumentServiceMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.ExerciseDocument)
                .Return(Observable.Empty<string>());
        }
    }
}