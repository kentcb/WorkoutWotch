using System;
using WorkoutWotch.Models;
using Kent.Boogaart.PCLMock;
using System.Threading.Tasks;

namespace WorkoutWotch.UnitTests.Models.Mocks
{
    public sealed class ActionMock : MockBase<IAction>, IAction
    {
        public ActionMock(MockBehavior behavior = MockBehavior.Strict)
            : base(behavior)
        {
            if (behavior == MockBehavior.Loose)
            {
                this.When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>())).Return(Task.FromResult(true));
            }
        }

        public TimeSpan Duration
        {
            get { return this.Apply(x => x.Duration); }
        }

        public Task ExecuteAsync(ExecutionContext context)
        {
            return this.Apply(x => x.ExecuteAsync(context));
        }
    }
}

