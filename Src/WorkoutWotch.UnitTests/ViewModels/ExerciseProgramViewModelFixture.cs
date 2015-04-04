namespace WorkoutWotch.UnitTests.ViewModels
{
    using System;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using ReactiveUI;
    using WorkoutWotch.Models;
    using WorkoutWotch.Services.Delay;
    using WorkoutWotch.UnitTests.Models;
    using WorkoutWotch.UnitTests.Models.Actions;
    using WorkoutWotch.UnitTests.Models.Mocks;
    using WorkoutWotch.UnitTests.Reactive;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.ViewModels;
    using Xunit;

    public class ExerciseProgramViewModelFixture
    {
        [Fact]
        public void ctor_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgramViewModel(null, new TestSchedulerService(), new ExerciseProgramBuilder().Build()));
        }

        [Fact]
        public void ctor_throws_if_scheduler_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgramViewModel(new LoggerServiceMock(), null, new ExerciseProgramBuilder().Build()));
        }

        [Fact]
        public void ctor_throws_if_model_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgramViewModel(new LoggerServiceMock(), new TestSchedulerService(), null));
        }

        [Theory]
        [InlineData("Name")]
        [InlineData("Some name")]
        [InlineData("Another name")]
        public void name_returns_name_in_model(string name)
        {
            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .WithName(name))
                .Build();

            Assert.Equal(name, sut.Name);
        }

        [Theory]
        [InlineData(239)]
        [InlineData(1)]
        [InlineData(232389)]
        public void duration_returns_duration_in_model(int durationInMs)
        {
            var duration = TimeSpan.FromMilliseconds(durationInMs);
            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithBeforeExerciseAction(new WaitActionBuilder()
                            .WithDelay(duration))))
                .Build();

            Assert.Equal(duration, sut.Duration);
        }

        [Fact]
        public void exercises_returns_exercises_in_model()
        {
            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithName("Exercise 1"))
                    .AddExercise(new ExerciseBuilder()
                        .WithName("Exercise 2")))
                .Build();

            Assert.NotNull(sut.Exercises);
            Assert.Equal(2, sut.Exercises.Count);
            Assert.Equal("Exercise 1", sut.Exercises[0].Name);
            Assert.Equal("Exercise 2", sut.Exercises[1].Name);
        }

        [Fact]
        public void is_started_is_false_by_default()
        {
            var sut = new ExerciseProgramViewModelBuilder().Build();

            Assert.False(sut.IsStarted);
        }

        [Fact]
        public async Task is_started_cycles_correctly_if_start_command_is_executed()
        {
            var scheduler = new TestSchedulerService();
            var sut = new ExerciseProgramViewModelBuilder()
                .WithSchedulerService(scheduler)
                .Build();

            var isStartedTask = sut
                .WhenAnyValue(x => x.IsStarted)
                .Take(3)
                .ToListAsync()
                .ToTask();

            using (scheduler.Pump())
            {
                sut.StartCommand.Execute(null);

                var isStarted = await isStartedTask;

                Assert.False(isStarted[0]);
                Assert.True(isStarted[1]);
                Assert.False(isStarted[2]);
            }
        }

        [Fact]
        public void is_start_visible_is_true_by_default()
        {
            var sut = new ExerciseProgramViewModelBuilder().Build();

            Assert.True(sut.IsStartVisible);
        }

        [Fact]
        public async Task is_start_visible_cycles_correctly_if_start_command_is_executed()
        {
            var scheduler = new TestSchedulerService();
            var sut = new ExerciseProgramViewModelBuilder()
                .WithSchedulerService(scheduler)
                .Build();

            var isStartVisibleTask = sut
                .WhenAnyValue(x => x.IsStartVisible)
                .Take(3)
                .ToListAsync()
                .ToTask();

            using (scheduler.Pump())
            {
                sut.StartCommand.Execute(null);

                var isStartVisible = await isStartVisibleTask;

                Assert.True(isStartVisible[0]);
                Assert.False(isStartVisible[1]);
                Assert.True(isStartVisible[2]);
            }
        }

        [Fact]
        public void is_paused_is_false_by_default()
        {
            var sut = new ExerciseProgramViewModelBuilder().Build();

            Assert.False(sut.IsPaused);
        }

        [Fact]
        public async Task is_paused_cycles_correctly_if_pause_command_is_executed()
        {
            var scheduler = new TestSchedulerService();
            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithBeforeExerciseAction(new WaitActionBuilder()
                            .WithDelayService(new DelayService())
                            .WithDelay(TimeSpan.FromMinutes(1)))))
                .WithSchedulerService(scheduler)
                .Build();

            using (scheduler.Pump())
            {
                await sut.WhenAnyValue(x => x.IsPaused)
                    .Where(x => !x)
                    .FirstAsync()
                    .Timeout(TimeSpan.FromSeconds(3));

                sut.StartCommand.Execute(null);

                await sut.WhenAnyValue(x => x.IsPaused)
                    .Where(x => !x)
                    .FirstAsync()
                    .Timeout(TimeSpan.FromSeconds(3));

                sut.PauseCommand.Execute(null);

                await sut.WhenAnyValue(x => x.IsPaused)
                    .Where(x => x)
                    .FirstAsync()
                    .Timeout(TimeSpan.FromSeconds(3));
            }
        }

        [Fact]
        public void is_pause_visible_is_false_by_default()
        {
            var sut = new ExerciseProgramViewModelBuilder().Build();

            Assert.False(sut.IsPauseVisible);
        }

        [Fact]
        public async Task is_pause_visible_cycles_correctly_if_start_command_is_executed()
        {
            var scheduler = new TestSchedulerService();
            var sut = new ExerciseProgramViewModelBuilder()
                .WithSchedulerService(scheduler)
                .Build();

            var isPauseVisibleTask = sut
                .WhenAnyValue(x => x.IsPauseVisible)
                .Take(3)
                .ToListAsync()
                .ToTask();

            using (scheduler.Pump())
            {
                sut.StartCommand.Execute(null);

                var isPauseVisible = await isPauseVisibleTask;

                Assert.False(isPauseVisible[0]);
                Assert.True(isPauseVisible[1]);
                Assert.False(isPauseVisible[2]);
            }
        }

        [Fact]
        public async Task is_pause_visible_cycles_correctly_if_pause_command_is_executed()
        {
            var scheduler = new TestSchedulerService();
            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithBeforeExerciseAction(new WaitActionBuilder()
                            .WithDelayService(new DelayService())
                            .WithDelay(TimeSpan.FromMinutes(1)))))
                .WithSchedulerService(scheduler)
                .Build();

            using (scheduler.Pump())
            {
                await sut.WhenAnyValue(x => x.IsPauseVisible)
                    .Where(x => !x)
                    .FirstAsync()
                    .Timeout(TimeSpan.FromSeconds(3));

                sut.StartCommand.Execute(null);

                await sut.WhenAnyValue(x => x.IsPauseVisible)
                    .Where(x => x)
                    .FirstAsync()
                    .Timeout(TimeSpan.FromSeconds(3));

                sut.PauseCommand.Execute(null);

                await sut.WhenAnyValue(x => x.IsPauseVisible)
                    .Where(x => !x)
                    .FirstAsync()
                    .Timeout(TimeSpan.FromSeconds(3));
            }
        }

        [Fact]
        public void is_resume_visible_is_false_by_default()
        {
            var sut = new ExerciseProgramViewModelBuilder().Build();

            Assert.False(sut.IsResumeVisible);
        }

        [Fact]
        public async Task is_resume_visible_cycles_correctly_if_start_command_is_executed_and_execution_is_paused()
        {
            var scheduler = new TestSchedulerService();
            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithBeforeExerciseAction(new WaitActionBuilder()
                            .WithDelayService(new DelayService())
                            .WithDelay(TimeSpan.FromMinutes(1)))))
                .WithSchedulerService(scheduler)
                .Build();

            using (scheduler.Pump())
            {
                await sut.WhenAnyValue(x => x.IsResumeVisible)
                    .Where(x => !x)
                    .FirstAsync()
                    .Timeout(TimeSpan.FromSeconds(3));

                sut.StartCommand.Execute(null);

                await sut.WhenAnyValue(x => x.IsResumeVisible)
                    .Where(x => !x)
                    .FirstAsync()
                    .Timeout(TimeSpan.FromSeconds(3));

                sut.PauseCommand.Execute(null);

                await sut.WhenAnyValue(x => x.IsResumeVisible)
                    .Where(x => x)
                    .FirstAsync()
                    .Timeout(TimeSpan.FromSeconds(3));
            }
        }

        [Fact]
        public async Task progress_time_span_is_updated_throughout_execution()
        {
            var scheduler = new TestSchedulerService();
            var action = new ActionMock(MockBehavior.Loose);

            action
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Return<ExecutionContext>(
                    ec =>
                        Task.Run(async () =>
                        {
                            ec.AddProgress(TimeSpan.FromSeconds(1));
                            ec.AddProgress(TimeSpan.FromSeconds(3));

                            ec.IsPaused = true;
                            await ec.WaitWhilePausedAsync();
                        }));

            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithBeforeExerciseAction(action)))
                .WithSchedulerService(scheduler)
                .Build();

            var progressTimeSpanTask = sut
                .WhenAnyValue(x => x.ProgressTimeSpan)
                .Take(3)
                .ToListAsync()
                .ToTask();

            using (scheduler.Pump())
            {
                sut.StartCommand.Execute(null);

                var progressTimeSpan = await progressTimeSpanTask;

                Assert.Equal(TimeSpan.Zero, progressTimeSpan[0]);
                Assert.Equal(TimeSpan.FromSeconds(1), progressTimeSpan[1]);
                Assert.Equal(TimeSpan.FromSeconds(4), progressTimeSpan[2]);
            }
        }

        [Fact]
        public async Task progress_is_updated_throughout_execution()
        {
            var scheduler = new TestSchedulerService();
            var action = new ActionMock(MockBehavior.Loose);

            action
                .When(x => x.Duration)
                .Return(TimeSpan.FromMinutes(1));

            action
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Return<ExecutionContext>(
                    ec =>
                        Task.Run(async () =>
                        {
                            ec.AddProgress(TimeSpan.FromSeconds(15));
                            ec.AddProgress(TimeSpan.FromSeconds(30));

                            ec.IsPaused = true;
                            await ec.WaitWhilePausedAsync();
                        }));

            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithBeforeExerciseAction(action)))
                .WithSchedulerService(scheduler)
                .Build();

            var progressTask = sut
                .WhenAnyValue(x => x.Progress)
                .Take(3)
                .ToListAsync()
                .ToTask();

            using (scheduler.Pump())
            {
                sut.StartCommand.Execute(null);

                var progress = await progressTask;

                Assert.Equal(0, progress[0]);
                Assert.Equal(0.25, progress[1]);
                Assert.Equal(0.75, progress[2]);
            }
        }

        [Fact]
        public async Task skip_backwards_command_is_disabled_if_on_first_exercise()
        {
            var scheduler = new TestSchedulerService();
            var action = new ActionMock(MockBehavior.Loose);

            action
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Return<ExecutionContext>(
                    ec =>
                        Task.Run(async () =>
                        {
                            ec.IsPaused = true;
                            await ec.WaitWhilePausedAsync();
                        }));

            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithBeforeExerciseAction(action)))
                .WithSchedulerService(scheduler)
                .Build();

            using (scheduler.Pump())
            {
                sut.StartCommand.Execute(null);

                await sut.WhenAnyValue(x => x.IsPaused)
                    .Where(x => x)
                    .Timeout(TimeSpan.FromSeconds(3))
                    .FirstAsync();

                Assert.False(sut.SkipBackwardsCommand.CanExecute(null));
            }
        }

        [Fact]
        public async Task skip_backwards_command_is_enabled_if_sufficient_progress_has_been_made_through_first_exercise()
        {
            var scheduler = new TestSchedulerService();
            var action = new ActionMock(MockBehavior.Loose);

            action
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Return<ExecutionContext>(
                    ec =>
                        Task.Run(async () =>
                        {
                            ec.AddProgress(TimeSpan.FromSeconds(1));
                            ec.IsPaused = true;
                            await ec.WaitWhilePausedAsync();
                        }));

            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithBeforeExerciseAction(action)))
                .WithSchedulerService(scheduler)
                .Build();

            using (scheduler.Pump())
            {
                sut.StartCommand.Execute(null);

                await sut.SkipBackwardsCommand.CanExecuteObservable
                    .Where(x => x)
                    .Timeout(TimeSpan.FromSeconds(3))
                    .FirstAsync();
            }
        }

        [Fact]
        public async Task skip_backwards_command_restarts_the_execution_context_from_the_start_of_the_current_exercise_if_sufficient_progress_has_been_made()
        {
            var scheduler = new TestSchedulerService();
            var action = new ActionMock(MockBehavior.Loose);

            action
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Return<ExecutionContext>(
                    ec =>
                        Task.Run(async () =>
                        {
                            ec.AddProgress(TimeSpan.FromSeconds(4));
                            ec.IsPaused = true;
                            await ec.WaitWhilePausedAsync();
                        }));

            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithBeforeExerciseAction(action)))
                .WithSchedulerService(scheduler)
                .Build();

            var progressTask = sut.WhenAnyValue(x => x.ProgressTimeSpan)
                .Take(3)
                .ToListAsync()
                .ToTask();

            using (scheduler.Pump())
            {
                sut.StartCommand.Execute(null);

                await sut.WhenAnyValue(x => x.IsPaused)
                    .Where(x => x)
                    .Timeout(TimeSpan.FromSeconds(3))
                    .FirstAsync();

                sut.SkipBackwardsCommand.Execute(null);

                var progress = await progressTask;

                Assert.Equal(TimeSpan.Zero, progress[0]);
                Assert.Equal(TimeSpan.FromSeconds(4), progress[1]);
                Assert.Equal(TimeSpan.Zero, progress[2]);
            }
        }

        [Fact]
        public async Task skip_backwards_command_restarts_the_execution_context_from_the_start_of_the_previous_exercise_if_the_current_exercise_if_only_recently_started()
        {
            var scheduler = new TestSchedulerService();
            var action = new ActionMock();

            action
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(10));

            action
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Return<ExecutionContext>(
                    ec =>
                        Task.Run(async () =>
                        {
                            ec.AddProgress(TimeSpan.FromSeconds(0.5));
                            ec.IsPaused = true;
                            await ec.WaitWhilePausedAsync();
                        }));

            var exercise1 = new ExerciseBuilder()
                .WithName("Exercise 1")
                .WithBeforeExerciseAction(action)
                .Build();

            var exercise2 = new ExerciseBuilder()
                .WithName("Exercise 2")
                .WithBeforeExerciseAction(action)
                .Build();

            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(exercise1)
                    .AddExercise(exercise2))
                .WithSchedulerService(scheduler)
                .Build();

            var progressTask = sut.WhenAnyValue(x => x.ProgressTimeSpan)
                .Take(4)
                .ToListAsync()
                .ToTask();

            using (scheduler.Pump())
            {
                // start from the second exercise
                sut.StartCommand.Execute(exercise1.Duration);

                await sut.WhenAnyValue(x => x.IsPaused)
                    .Where(x => x)
                    .Timeout(TimeSpan.FromSeconds(3))
                    .FirstAsync();

                sut.SkipBackwardsCommand.Execute(null);

                var progress = await progressTask;

                Assert.Equal(TimeSpan.Zero, progress[0]);
                Assert.Equal(TimeSpan.FromSeconds(10), progress[1]);
                Assert.Equal(TimeSpan.FromSeconds(10.5), progress[2]);
                Assert.Equal(TimeSpan.Zero, progress[3]);
            }
        }

        [Fact]
        public async Task skip_forwards_command_is_disabled_if_on_last_exercise()
        {
            var scheduler = new TestSchedulerService();
            var action = new ActionMock();

            action
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(10));

            action
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Return<ExecutionContext>(
                    ec =>
                        Task.Run(async () =>
                        {
                            ec.IsPaused = true;
                            await ec.WaitWhilePausedAsync();
                        }));

            var exercise1 = new ExerciseBuilder()
                .WithName("Exercise 1")
                .WithBeforeExerciseAction(action)
                .Build();

            var exercise2 = new ExerciseBuilder()
                .WithName("Exercise 2")
                .WithBeforeExerciseAction(action)
                .Build();

            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(exercise1)
                    .AddExercise(exercise2))
                .WithSchedulerService(scheduler)
                .Build();

            using (scheduler.Pump())
            {
                sut.StartCommand.Execute(null);

                await sut.WhenAnyValue(x => x.IsPaused)
                    .Where(x => x)
                    .Timeout(TimeSpan.FromSeconds(3))
                    .FirstAsync();

                sut.SkipForwardsCommand.Execute(null);

                await sut.SkipForwardsCommand.CanExecuteObservable
                    .Where(x => !x)
                    .Timeout(TimeSpan.FromSeconds(3))
                    .FirstAsync();
            }
        }

        [Fact]
        public async Task skip_forwards_command_skips_to_the_next_exercise()
        {
            var scheduler = new TestSchedulerService();
            var action = new ActionMock();

            action
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(10));

            action
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Return<ExecutionContext>(
                    ec =>
                        Task.Run(async () =>
                        {
                            ec.IsPaused = true;
                            await ec.WaitWhilePausedAsync();
                        }));

            var exercise1 = new ExerciseBuilder()
                .WithName("Exercise 1")
                .WithBeforeExerciseAction(action)
                .Build();

            var exercise2 = new ExerciseBuilder()
                .WithName("Exercise 2")
                .WithBeforeExerciseAction(action)
                .Build();

            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(exercise1)
                    .AddExercise(exercise2))
                .WithSchedulerService(scheduler)
                .Build();

            var progressTask = sut.WhenAnyValue(x => x.ProgressTimeSpan)
                .Take(2)
                .ToListAsync()
                .ToTask();

            using (scheduler.Pump())
            {
                sut.StartCommand.Execute(null);

                await sut.WhenAnyValue(x => x.IsPaused)
                    .Where(x => x)
                    .Timeout(TimeSpan.FromSeconds(3))
                    .FirstAsync();

                sut.SkipForwardsCommand.Execute(null);

                var progress = await progressTask;

                Assert.Equal(TimeSpan.Zero, progress[0]);
                Assert.Equal(TimeSpan.FromSeconds(10), progress[1]);
            }
        }

        [Fact]
        public async Task current_exercise_reflects_that_in_the_execution_context()
        {
            var scheduler = new TestSchedulerService();
            var action = new ActionMock();

            action
                .When(x => x.Duration)
                .Return(TimeSpan.FromSeconds(10));

            action
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Return<ExecutionContext>(
                    ec =>
                        Task.Run(async () =>
                        {
                            ec.IsPaused = true;
                            await ec.WaitWhilePausedAsync();
                        }));

            var exercise1 = new ExerciseBuilder()
                .WithName("Exercise 1")
                .WithBeforeExerciseAction(action)
                .Build();

            var exercise2 = new ExerciseBuilder()
                .WithName("Exercise 2")
                .WithBeforeExerciseAction(action)
                .Build();

            var exercise3 = new ExerciseBuilder()
                .WithName("Exercise 3")
                .WithBeforeExerciseAction(action)
                .Build();

            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(exercise1)
                    .AddExercise(exercise2)
                    .AddExercise(exercise3))
                .WithSchedulerService(scheduler)
                .Build();

            var currentExercisesTask = sut.WhenAnyValue(x => x.CurrentExercise)
                .Where(x => x != null)
                .DistinctUntilChanged()
                .Take(4)
                .ToListAsync()
                .ToTask();

            using (scheduler.Pump())
            {
                sut.StartCommand.Execute(null);

                await sut.WhenAnyValue(x => x.IsPaused)
                    .Where(x => x)
                    .Timeout(TimeSpan.FromSeconds(3))
                    .FirstAsync();

                sut.SkipForwardsCommand.Execute(null);

                await sut.SkipForwardsCommand.IsExecuting
                    .Where(x => !x)
                    .Timeout(TimeSpan.FromSeconds(3))
                    .FirstAsync();

                sut.SkipForwardsCommand.Execute(null);

                await sut.SkipForwardsCommand.IsExecuting
                    .Where(x => !x)
                    .Timeout(TimeSpan.FromSeconds(3))
                    .FirstAsync();

                sut.SkipBackwardsCommand.Execute(null);

                var currentExercises = await currentExercisesTask;

                Assert.Equal("Exercise 1", currentExercises[0].Name);
                Assert.Equal("Exercise 2", currentExercises[1].Name);
                Assert.Equal("Exercise 3", currentExercises[2].Name);
                Assert.Equal("Exercise 2", currentExercises[3].Name);
            }
        }
    }
}