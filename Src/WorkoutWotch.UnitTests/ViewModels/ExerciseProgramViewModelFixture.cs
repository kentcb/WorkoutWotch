namespace WorkoutWotch.UnitTests.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using Builders;
    using global::ReactiveUI;
    using Kent.Boogaart.PCLMock;
    using Models.Actions.Builders;
    using Models.Builders;
    using Services.Delay.Builders;
    using WorkoutWotch.Models;
    using WorkoutWotch.UnitTests.Models.Mocks;
    using WorkoutWotch.UnitTests.Reactive;
    using Xunit;

    public class ExerciseProgramViewModelFixture
    {
        [Fact]
        public void ctor_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgramViewModelBuilder().WithLoggerService(null).Build());
        }

        [Fact]
        public void ctor_throws_if_scheduler_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgramViewModelBuilder().WithSchedulerService(null).Build());
        }

        [Fact]
        public void ctor_throws_if_host_screen_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgramViewModelBuilder().WithHostScreen(null).Build());
        }

        [Fact]
        public void ctor_throws_if_model_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgramViewModelBuilder().WithModel(null).Build());
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
                        .WithBeforeExerciseAction(
                            new WaitActionBuilder()
                                .WithDelay(duration)
                                .Build())))
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
            var sut = new ExerciseProgramViewModelBuilder()
                .Build();

            Assert.False(sut.IsStarted);
        }

        [Fact]
        public void is_started_cycles_correctly_if_start_command_is_executed()
        {
            var scheduler = new TestSchedulerService();
            var sut = new ExerciseProgramViewModelBuilder()
                .WithSchedulerService(scheduler)
                .Build();
            scheduler.AdvanceMinimal();

            var isStarted = sut
                .WhenAnyValue(x => x.IsStarted)
                .CreateCollection();

            sut.StartCommand.Execute(null);
            scheduler.AdvanceMinimal();

            Assert.Equal(3, isStarted.Count);
            Assert.False(isStarted[0]);
            Assert.True(isStarted[1]);
            Assert.False(isStarted[2]);
        }

        [Fact]
        public void is_start_visible_is_true_by_default()
        {
            var scheduler = new TestSchedulerService();
            var sut = new ExerciseProgramViewModelBuilder()
                .WithSchedulerService(scheduler)
                .Build();
            scheduler.AdvanceMinimal();

            Assert.True(sut.IsStartVisible);
        }

        [Fact]
        public void is_start_visible_cycles_correctly_if_start_command_is_executed()
        {
            var scheduler = new TestSchedulerService();
            var sut = new ExerciseProgramViewModelBuilder()
                .WithSchedulerService(scheduler)
                .Build();
            scheduler.AdvanceMinimal();

            var isStartVisible = sut
                .WhenAnyValue(x => x.IsStartVisible)
                .CreateCollection();

            sut.StartCommand.Execute(null);
            scheduler.AdvanceMinimal();

            // TODO: the extra 2 values here appears to be due to ReactiveCommand's CanExecuteObservable implementation
            //       try this again once ReactiveCommand is re-written
            Assert.Equal(5, isStartVisible.Count);
            Assert.True(isStartVisible[0]);
            Assert.False(isStartVisible[1]);
            Assert.True(isStartVisible[2]);
            Assert.False(isStartVisible[3]);
            Assert.True(isStartVisible[4]);
        }

        [Fact]
        public void is_paused_is_false_by_default()
        {
            var sut = new ExerciseProgramViewModelBuilder()
                .Build();

            Assert.False(sut.IsPaused);
        }

        [Fact]
        public void is_paused_cycles_correctly_if_pause_command_is_executed()
        {
            var scheduler = new TestSchedulerService();
            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithBeforeExerciseAction(
                            new WaitActionBuilder()
                                .WithDelayService(new DelayServiceBuilder().Build())
                                .WithDelay(TimeSpan.FromMinutes(1))
                                .Build())))
                .WithSchedulerService(scheduler)
                .Build();

            Assert.False(sut.IsPaused);

            sut.StartCommand.Execute(null);
            scheduler.AdvanceMinimal();
            Assert.False(sut.IsPaused);

            sut.PauseCommand.Execute(null);
            scheduler.AdvanceMinimal();
            Assert.True(sut.IsPaused);
        }

        [Fact]
        public void is_pause_visible_is_false_by_default()
        {
            var sut = new ExerciseProgramViewModelBuilder()
                .Build();

            Assert.False(sut.IsPauseVisible);
        }

        [Fact]
        public void is_pause_visible_cycles_correctly_if_start_command_is_executed()
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
                        Observable
                            .Return(Unit.Default)
                            .Do(_ => { })
                            .Delay(TimeSpan.FromMinutes(1), scheduler.DefaultScheduler)
                            .Do(_ => ec.AddProgress(TimeSpan.FromMinutes(1))));
            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(
                    new ExerciseProgramBuilder()
                        .AddExercise(
                            new ExerciseBuilder()
                                .WithBeforeExerciseAction(action)))
                .WithSchedulerService(scheduler)
                .Build();

            Assert.False(sut.IsPauseVisible);

            sut.StartCommand.Execute(null);
            scheduler.AdvanceMinimal();
            Assert.True(sut.IsPauseVisible);

            scheduler.AdvanceBy(TimeSpan.FromMinutes(10));
            Assert.False(sut.IsPauseVisible);
        }

        [Fact]
        public void is_pause_visible_cycles_correctly_if_pause_command_is_executed()
        {
            var scheduler = new TestSchedulerService();
            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithBeforeExerciseAction(
                            new WaitActionBuilder()
                                .WithDelayService(new DelayServiceBuilder().Build())
                                .WithDelay(TimeSpan.FromMinutes(1))
                                .Build())))
                .WithSchedulerService(scheduler)
                .Build();

            var isPauseVisible = sut
                .WhenAnyValue(x => x.IsPauseVisible)
                .CreateCollection();

            sut.StartCommand.Execute(null);
            scheduler.AdvanceMinimal();

            sut.PauseCommand.Execute(null);
            scheduler.AdvanceMinimal();

            Assert.Equal(3, isPauseVisible.Count);
            Assert.False(isPauseVisible[0]);
            Assert.True(isPauseVisible[1]);
            Assert.False(isPauseVisible[2]);
        }

        [Fact]
        public void is_resume_visible_is_false_by_default()
        {
            var sut = new ExerciseProgramViewModelBuilder()
                .Build();

            Assert.False(sut.IsResumeVisible);
        }

        [Fact]
        public void is_resume_visible_cycles_correctly_if_start_command_is_executed_and_execution_is_paused()
        {
            var scheduler = new TestSchedulerService();
            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithBeforeExerciseAction(
                            new WaitActionBuilder()
                                .WithDelayService(new DelayServiceBuilder().Build())
                                .WithDelay(TimeSpan.FromMinutes(1))
                                .Build())))
                .WithSchedulerService(scheduler)
                .Build();

            var isResumeVisible = sut
                .WhenAnyValue(x => x.IsResumeVisible)
                .CreateCollection();

            sut.StartCommand.Execute(null);
            scheduler.AdvanceMinimal();

            sut.PauseCommand.Execute(null);
            scheduler.AdvanceMinimal();

            Assert.Equal(2, isResumeVisible.Count);
            Assert.False(isResumeVisible[0]);
            Assert.True(isResumeVisible[1]);
        }

        [Fact]
        public void progress_time_span_is_updated_throughout_execution()
        {
            var scheduler = new TestSchedulerService();
            var action = new ActionMock(MockBehavior.Loose);

            action
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Return<ExecutionContext>(
                    ec =>
                    {
                        ec.AddProgress(TimeSpan.FromSeconds(1));
                        ec.AddProgress(TimeSpan.FromSeconds(3));

                        ec.IsPaused = true;

                        return ec.WaitWhilePausedAsync();
                    });

            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithBeforeExerciseAction(action)))
                .WithSchedulerService(scheduler)
                .Build();

            var progressTimeSpan = sut
                .WhenAnyValue(x => x.ProgressTimeSpan)
                .CreateCollection();

            sut.StartCommand.Execute(null);
            scheduler.AdvanceMinimal();

            Assert.Equal(3, progressTimeSpan.Count);
            Assert.Equal(TimeSpan.Zero, progressTimeSpan[0]);
            Assert.Equal(TimeSpan.FromSeconds(1), progressTimeSpan[1]);
            Assert.Equal(TimeSpan.FromSeconds(4), progressTimeSpan[2]);
        }

        [Fact]
        public void progress_is_updated_throughout_execution()
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
                    {
                        ec.AddProgress(TimeSpan.FromSeconds(15));
                        ec.AddProgress(TimeSpan.FromSeconds(30));

                        ec.IsPaused = true;

                        return ec.WaitWhilePausedAsync();
                    });

            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithBeforeExerciseAction(action)))
                .WithSchedulerService(scheduler)
                .Build();

            var progress = sut
                .WhenAnyValue(x => x.Progress)
                .CreateCollection();

            sut.StartCommand.Execute(null);
            scheduler.AdvanceMinimal();

            Assert.Equal(3, progress.Count);
            Assert.Equal(0, progress[0]);
            Assert.Equal(0.25, progress[1]);
            Assert.Equal(0.75, progress[2]);
        }

        [Fact]
        public void skip_backwards_command_is_disabled_if_on_first_exercise()
        {
            var scheduler = new TestSchedulerService();
            var action = new ActionMock(MockBehavior.Loose);

            action
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Return<ExecutionContext>(
                    ec =>
                    {
                        ec.IsPaused = true;
                        return ec.WaitWhilePausedAsync();
                    });

            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithBeforeExerciseAction(action)))
                .WithSchedulerService(scheduler)
                .Build();

            sut.StartCommand.Execute(null);
            scheduler.AdvanceMinimal();

            Assert.False(sut.SkipBackwardsCommand.CanExecute(null));
        }

        [Fact]
        public void skip_backwards_command_is_enabled_if_sufficient_progress_has_been_made_through_first_exercise()
        {
            var scheduler = new TestSchedulerService();
            var action = new ActionMock(MockBehavior.Loose);

            action
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Return<ExecutionContext>(
                    ec =>
                    {
                        ec.AddProgress(TimeSpan.FromSeconds(1));
                        ec.IsPaused = true;
                        return ec.WaitWhilePausedAsync();
                    });

            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithBeforeExerciseAction(action)))
                .WithSchedulerService(scheduler)
                .Build();

            // TODO: technically, I should just check CanExecute(null) at the end, but without this subscription the RxCommand does not update CanExecute correctly
            //       try changing this once I'm using new RxCommand
            var canExecute = sut
                .SkipBackwardsCommand
                .CanExecuteObservable
                .CreateCollection();

            sut.StartCommand.Execute(null);
            scheduler.AdvanceMinimal();

            Assert.Equal(2, canExecute.Count);
            Assert.False(canExecute[0]);
            Assert.True(canExecute[1]);
        }

        [Fact]
        public void skip_backwards_command_restarts_the_execution_context_from_the_start_of_the_current_exercise_if_sufficient_progress_has_been_made()
        {
            var scheduler = new TestSchedulerService();
            var action = new ActionMock(MockBehavior.Loose);

            action
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Return<ExecutionContext>(
                    ec =>
                    {
                        ec.AddProgress(TimeSpan.FromSeconds(4));
                        ec.IsPaused = true;

                        return ec.WaitWhilePausedAsync();
                    });

            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithBeforeExerciseAction(action)))
                .WithSchedulerService(scheduler)
                .Build();

            var progress = sut
                .WhenAnyValue(x => x.ProgressTimeSpan)
                .CreateCollection();

            sut.StartCommand.Execute(null);
            scheduler.AdvanceMinimal();

            sut.SkipBackwardsCommand.Execute(null);
            scheduler.AdvanceMinimal();

            Assert.Equal(3, progress.Count);
            Assert.Equal(TimeSpan.Zero, progress[0]);
            Assert.Equal(TimeSpan.FromSeconds(4), progress[1]);
            Assert.Equal(TimeSpan.Zero, progress[2]);
        }

        [Fact]
        public void skip_backwards_command_restarts_the_execution_context_from_the_start_of_the_previous_exercise_if_the_current_exercise_if_only_recently_started()
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
                    {
                        ec.AddProgress(TimeSpan.FromSeconds(0.5));
                        ec.IsPaused = true;

                        return ec.WaitWhilePausedAsync();
                    });

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

            var progress = sut
                .WhenAnyValue(x => x.ProgressTimeSpan)
                .CreateCollection();

            // start from the second exercise
            sut.StartCommand.Execute(exercise1.Duration);
            scheduler.AdvanceMinimal();

            sut.SkipBackwardsCommand.Execute(null);
            scheduler.AdvanceMinimal();

            Assert.Equal(5, progress.Count);
            Assert.Equal(TimeSpan.Zero, progress[0]);
            Assert.Equal(TimeSpan.FromSeconds(10), progress[1]);
            Assert.Equal(TimeSpan.FromSeconds(10.5), progress[2]);
            Assert.Equal(TimeSpan.Zero, progress[3]);
            Assert.Equal(TimeSpan.FromSeconds(10), progress[4]);
        }

        [Fact]
        public void skip_forwards_command_is_disabled_if_on_last_exercise()
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
                    {
                        ec.IsPaused = true;

                        return ec.WaitWhilePausedAsync();
                    });

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

            var canExecute = sut
                .SkipForwardsCommand
                .CanExecuteObservable
                .CreateCollection();

            sut.StartCommand.Execute(null);
            scheduler.AdvanceMinimal();

            sut.SkipForwardsCommand.Execute(null);
            scheduler.AdvanceMinimal();

            Assert.Equal(5, canExecute.Count);
            Assert.False(canExecute[0]);
            Assert.True(canExecute[1]);
            Assert.False(canExecute[2]);
            Assert.True(canExecute[3]);
            Assert.False(canExecute[4]);
        }

        [Fact]
        public void skip_forwards_command_skips_to_the_next_exercise()
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
                    {
                        ec.IsPaused = true;

                        return ec.WaitWhilePausedAsync();
                    });

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

            var progress = sut
                .WhenAnyValue(x => x.ProgressTimeSpan)
                .CreateCollection();

            sut.StartCommand.Execute(null);
            scheduler.AdvanceMinimal();

            sut.SkipForwardsCommand.Execute(null);
            scheduler.AdvanceMinimal();

            Assert.Equal(2, progress.Count);
            Assert.Equal(TimeSpan.Zero, progress[0]);
            Assert.Equal(TimeSpan.FromSeconds(10), progress[1]);
        }

        [Fact]
        public void current_exercise_reflects_that_in_the_execution_context()
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
                    {
                        ec.IsPaused = true;
                        return ec.WaitWhilePausedAsync();
                    });

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

            sut.StartCommand.Execute(null);
            scheduler.AdvanceMinimal();
            Assert.Equal("Exercise 1", sut.CurrentExercise?.Name);

            sut.SkipForwardsCommand.Execute(null);
            scheduler.AdvanceMinimal();
            Assert.Equal("Exercise 2", sut.CurrentExercise?.Name);

            sut.SkipForwardsCommand.Execute(null);
            scheduler.AdvanceMinimal();
            Assert.Equal("Exercise 3", sut.CurrentExercise?.Name);

            sut.SkipBackwardsCommand.Execute(null);
            scheduler.AdvanceMinimal();
            Assert.Equal("Exercise 2", sut.CurrentExercise?.Name);
        }
        
        [Fact]
        public void execution_context_is_cancelled_if_user_navigates_away()
        {
            var scheduler = new TestSchedulerService();
            var sut = new ExerciseProgramViewModelBuilder()
                .WithModel(new ExerciseProgramBuilder()
                    .AddExercise(new ExerciseBuilder()
                        .WithBeforeExerciseAction(
                            new WaitActionBuilder()
                                .WithDelayService(new DelayServiceBuilder().Build())
                                .WithDelay(TimeSpan.FromMinutes(1))
                                .Build())))
                .WithSchedulerService(scheduler)
                .Build();
            sut
                .HostScreen
                .Router
                .NavigationStack
                .Add(sut);

            sut.StartCommand.Execute(null);
            scheduler.AdvanceMinimal();

            Assert.True(sut.IsStarted);

            sut
                .HostScreen
                .Router
                .NavigateBack
                .Execute(null);
            scheduler.AdvanceMinimal();

            Assert.False(sut.IsStarted);
        }
    }
}