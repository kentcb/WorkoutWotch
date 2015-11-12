namespace WorkoutWotch.UnitTests.Services.Audio.Mocks
{
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;

    public sealed partial class AudioServiceMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.PlayAsync(It.IsAny<string>()))
                .Return(Task.FromResult(true));
        }
    }
}