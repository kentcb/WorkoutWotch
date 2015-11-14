namespace WorkoutWotch.UnitTests.Services.State.Mocks
{
    using System.Reactive;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Services.Contracts.State;

    public sealed partial class StateServiceMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.SaveAsync())
                .Return(Observable.Return(Unit.Default));
            this
                .When(x => x.RegisterSaveCallback(It.IsAny<SaveCallback>()))
                .Return(Disposable.Empty);
        }
    }
}