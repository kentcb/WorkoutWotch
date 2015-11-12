namespace WorkoutWotch.UnitTests.Services.State.Mocks
{
    using System;
    using System.Reactive.Disposables;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Services.Contracts.State;

    public sealed partial class StateServiceMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.SaveAsync())
                .Return(Task.FromResult(true));
            this
                .When(x => x.RegisterSaveCallback(It.IsAny<Func<IStateService, Task>>()))
                .Return(Disposable.Empty);
        }
    }
}