namespace WorkoutWotch.UnitTests.Services.Audio.Mocks
{
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Services.Contracts.Audio;

    public sealed class AudioServiceMock : MockBase<IAudioService>, IAudioService
    {
        public AudioServiceMock(MockBehavior behavior = MockBehavior.Strict)
            : base(behavior)
        {
            if (behavior == MockBehavior.Loose)
            {
                this.When(x => x.PlayAsync(It.IsAny<string>())).Return(Task.FromResult(true));
            }
        }

        public Task PlayAsync(string resourceUri)
        {
            return this.Apply(x => x.PlayAsync(resourceUri));
        }
    }
}