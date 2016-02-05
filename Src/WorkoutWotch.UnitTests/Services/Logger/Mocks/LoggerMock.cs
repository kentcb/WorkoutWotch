namespace WorkoutWotch.UnitTests.Services.Logger.Mocks
{
    public sealed partial class LoggerMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.IsDebugEnabled)
                .Return(true);
            this
                .When(x => x.IsErrorEnabled)
                .Return(true);
            this
                .When(x => x.IsInfoEnabled)
                .Return(true);
            this
                .When(x => x.IsPerfEnabled)
                .Return(true);
            this
                .When(x => x.IsWarnEnabled)
                .Return(true);
        }
    }
}