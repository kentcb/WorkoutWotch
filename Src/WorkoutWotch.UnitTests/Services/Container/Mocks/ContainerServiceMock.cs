namespace WorkoutWotch.UnitTests.Services.Container.Mocks
{
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.Container;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using WorkoutWotch.Services.Contracts.Speech;
    using WorkoutWotch.Services.Contracts.State;
    using WorkoutWotch.UnitTests.Services.Audio.Mocks;
    using WorkoutWotch.UnitTests.Services.Delay.Mocks;
    using WorkoutWotch.UnitTests.Services.ExerciseDocument.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.Scheduler.Mocks;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;
    using WorkoutWotch.UnitTests.Services.State.Mocks;

    public sealed class ContainerServiceMock : MockBase<IContainerService>, IContainerService
    {
        public ContainerServiceMock(MockBehavior behavior = MockBehavior.Strict)
            : base(behavior)
        {
            if (behavior == MockBehavior.Loose)
            {
                this.When(x => x.Resolve<IAudioService>()).Return(new AudioServiceMock(MockBehavior.Loose));
                this.When(x => x.Resolve<IContainerService>()).Return(this);
                this.When(x => x.Resolve<IDelayService>()).Return(new DelayServiceMock(MockBehavior.Loose));
                this.When(x => x.Resolve<IExerciseDocumentService>()).Return(new ExerciseDocumentServiceMock(MockBehavior.Loose));
                this.When(x => x.Resolve<ILoggerService>()).Return(new LoggerServiceMock(MockBehavior.Loose));
                this.When(x => x.Resolve<ISchedulerService>()).Return(new SchedulerServiceMock(MockBehavior.Loose));
                this.When(x => x.Resolve<ISpeechService>()).Return(new SpeechServiceMock(MockBehavior.Loose));
                this.When(x => x.Resolve<IStateService>()).Return(new StateServiceMock(MockBehavior.Loose));
            }
        }

        public T Resolve<T>()
            where T : class
        {
            return this.Apply(x => x.Resolve<T>());
        }
    }
}