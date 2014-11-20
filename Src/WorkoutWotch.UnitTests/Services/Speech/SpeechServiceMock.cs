namespace WorkoutWotch.UnitTests.Services.Speech
{
    using System.Threading;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Services.Contracts.Speech;

    public sealed class SpeechServiceMock : MockBase<ISpeechService>, ISpeechService
    {
        public SpeechServiceMock(MockBehavior behavior = MockBehavior.Strict)
            : base(behavior)
        {
            if (behavior == MockBehavior.Loose)
            {
                this.When(x => x.SpeakAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Return(Task.FromResult(true));
            }
        }

        public Task SpeakAsync(string speechString, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.Apply(x => x.SpeakAsync(speechString, cancellationToken));
        }
    }
}