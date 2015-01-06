namespace WorkoutWotch.UnitTests.Services.State
{
    using Akavache;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.State;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.State.Mocks;

    internal sealed class StateServiceBuilder
    {
        private IBlobCache blobCache;
        private ILoggerService loggerService;

        public StateServiceBuilder()
        {
            this.blobCache = new BlobCacheMock(MockBehavior.Loose);
            this.loggerService = new LoggerServiceMock(MockBehavior.Loose);
        }

        public StateService Build()
        {
            return new StateService(
                this.blobCache,
                this.loggerService);
        }

        public StateServiceBuilder WithBlobCache(IBlobCache blobCache)
        {
            this.blobCache = blobCache;
            return this;
        }

        public StateServiceBuilder WithLoggerService(ILoggerService loggerService)
        {
            this.loggerService = loggerService;
            return this;
        }
    }
}