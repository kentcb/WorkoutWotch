namespace WorkoutWotch.UnitTests.Models.Mocks
{
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Models;

    public sealed partial class ActionMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Return(Task.FromResult(true));
        }
    }
}