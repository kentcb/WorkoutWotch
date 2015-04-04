namespace WorkoutWotch.UnitTests.ViewModels
{
    using System;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.UnitTests.Models;
    using WorkoutWotch.UnitTests.Models.Mocks;
    using WorkoutWotch.UnitTests.Reactive;
    using WorkoutWotch.ViewModels;
    using Xunit;

    public class ExerciseViewModelFixture
    {
        [Fact]
        public void ctor_throws_if_scheduler_service_is_null()
        {
            var model = new ExerciseBuilder().Build();
            Assert.Throws<ArgumentNullException>(() => new ExerciseViewModel(null, model, Observable.Never<ExecutionContext>()));
        }

        [Fact]
        public void ctor_throws_if_model_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseViewModel(new TestSchedulerService(), null, Observable.Never<ExecutionContext>()));
        }

        [Fact]
        public void ctor_throws_if_execution_context_is_null()
        {
            var model = new ExerciseBuilder().Build();
            Assert.Throws<ArgumentNullException>(() => new ExerciseViewModel(new TestSchedulerService(), model, null));
        }

        [Theory]
        [InlineData("Name")]
        [InlineData("Another name")]
        [InlineData("Yet another wacky &*(&!^^@9  \t823 name")]
        public void name_returns_name_in_model(string name)
        {
            var sut = new ExerciseViewModelBuilder()
                .WithModel(new ExerciseBuilder()
                    .WithName(name))
                .Build();

            Assert.Equal(name, sut.Name);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(1000)]
        [InlineData(34871)]
        public void duration_returns_duration_in_model(int durationInMs)
        {
            var action = new ActionMock(MockBehavior.Loose);

            action
                .When(x => x.Duration)
                .Return(TimeSpan.FromMilliseconds(durationInMs));

            var sut = new ExerciseViewModelBuilder()
                .WithModel(new ExerciseBuilder()
                    .WithBeforeExerciseAction(action))
                .Build();

            Assert.Equal(TimeSpan.FromMilliseconds(durationInMs), sut.Duration);
        }

        [Fact]
        public void progress_time_span_is_zero_if_there_is_no_execution_context()
        {
            var sut = new ExerciseViewModelBuilder().Build();

            Assert.Equal(TimeSpan.Zero, sut.ProgressTimeSpan);
        }

        [Fact]
        public void progress_time_span_is_zero_if_no_progress_has_been_made_through_this_exercise()
        {
            var model1 = new ExerciseBuilder().Build();
            var model2 = new ExerciseBuilder().Build();
            var executionContext = new ExecutionContext();
            var sut = new ExerciseViewModelBuilder()
                .WithModel(model2)
                .WithExecutionContext(executionContext)
                .Build();

            executionContext.SetCurrentExercise(model1);
            executionContext.AddProgress(TimeSpan.FromSeconds(3));

            Assert.Equal(TimeSpan.Zero, sut.ProgressTimeSpan);
        }

        [Fact]
        public void progress_time_span_reflects_any_progression_through_the_exercise()
        {
            var scheduler = new TestSchedulerService();
            var model = new ExerciseBuilder().Build();
            var executionContext = new ExecutionContext();
            var sut = new ExerciseViewModelBuilder()
                .WithSchedulerService(scheduler)
                .WithExecutionContext(executionContext)
                .WithModel(model)
                .Build();

            executionContext.SetCurrentExercise(model);

            scheduler.Start();
            Assert.Equal(TimeSpan.Zero, sut.ProgressTimeSpan);

            executionContext.AddProgress(TimeSpan.FromSeconds(3));
            scheduler.Start();
            Assert.Equal(TimeSpan.FromSeconds(3), sut.ProgressTimeSpan);

            executionContext.AddProgress(TimeSpan.FromSeconds(2));
            scheduler.Start();
            Assert.Equal(TimeSpan.FromSeconds(5), sut.ProgressTimeSpan);
        }

        [Fact]
        public void progress_time_span_is_not_reset_to_zero_if_another_exercise_is_started()
        {
            var scheduler = new TestSchedulerService();
            var model1 = new ExerciseBuilder().Build();
            var model2 = new ExerciseBuilder().Build();
            var executionContext = new ExecutionContext();
            var sut = new ExerciseViewModelBuilder()
                .WithSchedulerService(scheduler)
                .WithExecutionContext(executionContext)
                .WithModel(model1)
                .Build();

            executionContext.SetCurrentExercise(model1);
            executionContext.AddProgress(TimeSpan.FromSeconds(3));

            scheduler.Start();
            Assert.Equal(TimeSpan.FromSeconds(3), sut.ProgressTimeSpan);

            executionContext.SetCurrentExercise(model2);
            scheduler.Start();
            Assert.Equal(TimeSpan.FromSeconds(3), sut.ProgressTimeSpan);

            executionContext.AddProgress(TimeSpan.FromSeconds(3));
            scheduler.Start();
            Assert.Equal(TimeSpan.FromSeconds(3), sut.ProgressTimeSpan);
        }

        [Fact]
        public void progress_time_span_is_reset_to_zero_if_the_execution_context_changes()
        {
            var scheduler = new TestSchedulerService();
            var model = new ExerciseBuilder().Build();
            var executionContext = new ExecutionContext();
            var executionContextSubject = new Subject<ExecutionContext>();
            var sut = new ExerciseViewModelBuilder()
                .WithSchedulerService(scheduler)
                .WithExecutionContext(executionContextSubject)
                .WithModel(model)
                .Build();

            executionContextSubject.OnNext(executionContext);
            executionContext.SetCurrentExercise(model);

            executionContext.AddProgress(TimeSpan.FromSeconds(3));
            scheduler.Start();
            Assert.Equal(TimeSpan.FromSeconds(3), sut.ProgressTimeSpan);

            executionContextSubject.OnNext(new ExecutionContext());
            scheduler.Start();
            Assert.Equal(TimeSpan.Zero, sut.ProgressTimeSpan);
        }

        [Fact]
        public void progress_time_span_is_reset_to_zero_if_the_execution_context_changes_to_null()
        {
            var scheduler = new TestSchedulerService();
            var model = new ExerciseBuilder().Build();
            var executionContext = new ExecutionContext();
            var executionContextSubject = new Subject<ExecutionContext>();
            var sut = new ExerciseViewModelBuilder()
                .WithSchedulerService(scheduler)
                .WithExecutionContext(executionContextSubject)
                .WithModel(model)
                .Build();

            executionContextSubject.OnNext(executionContext);
            executionContext.SetCurrentExercise(model);

            executionContext.AddProgress(TimeSpan.FromSeconds(3));
            scheduler.Start();
            Assert.Equal(TimeSpan.FromSeconds(3), sut.ProgressTimeSpan);

            executionContextSubject.OnNext(null);
            scheduler.Start();
            Assert.Equal(TimeSpan.Zero, sut.ProgressTimeSpan);
        }

        [Theory]
        [InlineData(0, 0, 0d)]
        [InlineData(0, 100, 0d)]
        [InlineData(100, -100, 0d)]
        [InlineData(1000, 0, 0d)]
        [InlineData(1000, 100, 0.1d)]
        [InlineData(1000, 500, 0.5d)]
        [InlineData(10000, 500, 0.05d)]
        [InlineData(10000, 9000, 0.9d)]
        [InlineData(10000, 9500, 0.95d)]
        [InlineData(10000, 10000, 1d)]
        [InlineData(10000, 11000, 1d)]
        public void progress_is_calculated_based_on_duration_and_progress_time_span(int durationInMs, int progressInMs, double expectedProgress)
        {
            var scheduler = new TestSchedulerService();
            var action = new ActionMock(MockBehavior.Loose);

            action
                .When(x => x.Duration)
                .Return(TimeSpan.FromMilliseconds(durationInMs));

            var model = new ExerciseBuilder()
                .WithBeforeExerciseAction(action)
                .Build();

            var executionContext = new ExecutionContext();
            var sut = new ExerciseViewModelBuilder()
                .WithSchedulerService(scheduler)
                .WithExecutionContext(executionContext)
                .WithModel(model)
                .Build();

            executionContext.SetCurrentExercise(model);
            executionContext.AddProgress(TimeSpan.FromMilliseconds(progressInMs));

            scheduler.Start();

            Assert.Equal(expectedProgress, sut.Progress);
        }

        [Fact]
        public void is_active_is_false_if_there_is_no_execution_context()
        {
            var scheduler = new TestSchedulerService();
            var sut = new ExerciseViewModelBuilder()
                .WithSchedulerService(scheduler)
                .Build();

            scheduler.Start();
            Assert.False(sut.IsActive);
        }

        [Fact]
        public void is_active_is_true_if_this_exercise_is_the_current_exercise()
        {
            var scheduler = new TestSchedulerService();
            var model = new ExerciseBuilder().Build();
            var executionContext = new ExecutionContext();
            var sut = new ExerciseViewModelBuilder()
                .WithSchedulerService(scheduler)
                .WithExecutionContext(executionContext)
                .WithModel(model)
                .Build();

            scheduler.Start();
            Assert.False(sut.IsActive);

            executionContext.SetCurrentExercise(model);
            scheduler.Start();
            Assert.True(sut.IsActive);
        }

        [Fact]
        public void is_active_is_false_if_this_exercise_is_not_the_current_exercise()
        {
            var scheduler = new TestSchedulerService();
            var model1 = new ExerciseBuilder().Build();
            var model2 = new ExerciseBuilder().Build();
            var executionContext = new ExecutionContext();
            var sut = new ExerciseViewModelBuilder()
                .WithSchedulerService(scheduler)
                .WithExecutionContext(executionContext)
                .WithModel(model1)
                .Build();

            scheduler.Start();
            Assert.False(sut.IsActive);

            executionContext.SetCurrentExercise(model2);
            scheduler.Start();
            Assert.False(sut.IsActive);
        }
    }
}