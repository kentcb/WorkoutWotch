namespace WorkoutWotch.UnitTests.Services.Speech.Mocks
{
    using System.Threading;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;

    public sealed partial class SpeechServiceMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.SpeakAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Return(Task.FromResult(true));
        }
    }
}