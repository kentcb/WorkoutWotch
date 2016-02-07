namespace WorkoutWotch.UnitTests.Services.Audio.Mocks
{
    using System.Reactive;
    using System.Reactive.Linq;
    using PCLMock;

    public sealed partial class AudioServiceMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.Play(It.IsAny<string>()))
                .Return(Observable.Return(Unit.Default));
        }
    }
}