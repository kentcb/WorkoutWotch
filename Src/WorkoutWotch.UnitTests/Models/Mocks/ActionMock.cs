namespace WorkoutWotch.UnitTests.Models.Mocks
{
    using System.Reactive;
    using System.Reactive.Linq;
    using PCLMock;
    using WorkoutWotch.Models;

    public sealed partial class ActionMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.Execute(It.IsAny<ExecutionContext>()))
                .Return(Observables.Unit);
        }
    }
}