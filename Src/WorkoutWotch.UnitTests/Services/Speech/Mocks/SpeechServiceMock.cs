namespace WorkoutWotch.UnitTests.Services.Speech.Mocks
{
    using System.Reactive;
    using System.Reactive.Linq;
    using PCLMock;

    public sealed partial class SpeechServiceMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.Speak(It.IsAny<string>()))
                .Return(Observable.Return(Unit.Default));
        }
    }
}