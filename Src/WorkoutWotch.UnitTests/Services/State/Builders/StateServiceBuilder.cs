namespace WorkoutWotch.UnitTests.Services.State.Builders
{
    using Akavache;
    using PCLMock;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.State;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.State.Mocks;

    public sealed class StateServiceBuilder : IBuilder
    {
        private IBlobCache blobCache;
        private ILoggerService loggerService;

        public StateServiceBuilder()
        {
            this.blobCache = new BlobCacheMock(MockBehavior.Loose);
            this.loggerService = new LoggerServiceMock(MockBehavior.Loose);
        }

        public StateServiceBuilder WithBlobCache(IBlobCache blobCache) =>
            this.With(ref this.blobCache, blobCache);

        public StateServiceBuilder WithLoggerService(ILoggerService loggerService) =>
            this.With(ref this.loggerService, loggerService);

        public StateService Build() =>
            new StateService(
                this.blobCache,
                this.loggerService);

        public static implicit operator StateService(StateServiceBuilder builder) =>
            builder.Build();
    }
}