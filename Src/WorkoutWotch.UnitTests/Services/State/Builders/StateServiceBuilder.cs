namespace WorkoutWotch.UnitTests.Services.State.Builders
{
    using Akavache;
    using PCLMock;
    using WorkoutWotch.Services.State;
    using WorkoutWotch.UnitTests.Services.State.Mocks;

    public sealed class StateServiceBuilder : IBuilder
    {
        private IBlobCache blobCache;

        public StateServiceBuilder()
        {
            this.blobCache = new BlobCacheMock(MockBehavior.Loose);
        }

        public StateServiceBuilder WithBlobCache(IBlobCache blobCache) =>
            this.With(ref this.blobCache, blobCache);

        public StateService Build() =>
            new StateService(
                this.blobCache);

        public static implicit operator StateService(StateServiceBuilder builder) =>
            builder.Build();
    }
}