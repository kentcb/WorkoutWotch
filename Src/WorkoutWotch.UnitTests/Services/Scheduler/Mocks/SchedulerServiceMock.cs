namespace WorkoutWotch.UnitTests.Services.Scheduler.Mocks
{
    using PCLMock;

    public sealed partial class SchedulerServiceMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.DefaultScheduler)
                .Return(new SchedulerMock(MockBehavior.Loose));
            this
                .When(x => x.CurrentThreadScheduler)
                .Return(new SchedulerMock(MockBehavior.Loose));
            this
                .When(x => x.ImmediateScheduler)
                .Return(new SchedulerMock(MockBehavior.Loose));
            this
                .When(x => x.MainScheduler)
                .Return(new SchedulerMock(MockBehavior.Loose));
            this
                .When(x => x.TaskPoolScheduler)
                .Return(new SchedulerMock(MockBehavior.Loose));
        }
    }
}