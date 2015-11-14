namespace WorkoutWotch.UnitTests.Services.Speech.Mocks
{
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using Kent.Boogaart.PCLMock;

    public sealed partial class SpeechServiceMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.SpeakAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Return(Observable.Return(Unit.Default));
        }
    }
}