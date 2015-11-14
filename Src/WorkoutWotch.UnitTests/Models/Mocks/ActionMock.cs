namespace WorkoutWotch.UnitTests.Models.Mocks
{
    using System.Reactive;
    using System.Reactive.Linq;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Models;

    public sealed partial class ActionMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Return(Observable.Return(Unit.Default));
        }
    }
}