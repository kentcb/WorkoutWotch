namespace WorkoutWotch.UnitTests.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using WorkoutWotch.Models;
    using WorkoutWotch.UnitTests.Models;
    using WorkoutWotch.UnitTests.Models.Mocks;
    using WorkoutWotch.UnitTests.Reactive;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;
    using WorkoutWotch.ViewModels;

    [TestFixture]
    public class ExerciseViewModelFixture
    {
        [Test]
        public void ctor_throws_if_scheduler_service_is_null()
        {
            var model = new ExerciseBuilder().Build();
            Assert.Throws<ArgumentNullException>(() => new ExerciseViewModel(null, model, Observable.Never<ExecutionContext>()));
        }

        [Test]
        public void ctor_throws_if_model_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseViewModel(new TestSchedulerService(), null, Observable.Never<ExecutionContext>()));
        }

        [Test]
        public void ctor_throws_if_execution_context_is_null()
        {
            var model = new ExerciseBuilder().Build();
            Assert.Throws<ArgumentNullException>(() => new ExerciseViewModel(new TestSchedulerService(), model, null));
        }

        [TestCase("Name")]
        [TestCase("Another name")]
        [TestCase("Yet another wacky &*(&!^^@9  \t823 name")]
        public void name_returns_name_in_model(string name)
        {
            var sut = new ExerciseViewModelBuilder()
                .WithModel(new ExerciseBuilder()
                    .WithName(name))
                .Build();

            Assert.AreEqual(name, sut.Name);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(1000)]
        [TestCase(34871)]
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

            Assert.AreEqual(TimeSpan.FromMilliseconds(durationInMs), sut.Duration);
        }

        [Test]
        public void progress_time_span_is_zero_if_there_is_no_execution_context()
        {
            var sut = new ExerciseViewModelBuilder().Build();

            Assert.AreEqual(TimeSpan.Zero, sut.ProgressTimeSpan);
        }

        [Test]
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

            Assert.AreEqual(TimeSpan.Zero, sut.ProgressTimeSpan);
        }

        [Test]
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
            Assert.AreEqual(TimeSpan.Zero, sut.ProgressTimeSpan);

            executionContext.AddProgress(TimeSpan.FromSeconds(3));
            scheduler.Start();
            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.ProgressTimeSpan);

            executionContext.AddProgress(TimeSpan.FromSeconds(2));
            scheduler.Start();
            Assert.AreEqual(TimeSpan.FromSeconds(5), sut.ProgressTimeSpan);
        }

        [Test]
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
            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.ProgressTimeSpan);

            executionContext.SetCurrentExercise(model2);
            scheduler.Start();
            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.ProgressTimeSpan);

            executionContext.AddProgress(TimeSpan.FromSeconds(3));
            scheduler.Start();
            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.ProgressTimeSpan);
        }

        [Test]
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
            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.ProgressTimeSpan);

            executionContextSubject.OnNext(new ExecutionContext());
            scheduler.Start();
            Assert.AreEqual(TimeSpan.Zero, sut.ProgressTimeSpan);
        }

        [Test]
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
            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.ProgressTimeSpan);

            executionContextSubject.OnNext(null);
            scheduler.Start();
            Assert.AreEqual(TimeSpan.Zero, sut.ProgressTimeSpan);
        }

        [TestCase(0, 0, 0d)]
        [TestCase(0, 100, 0d)]
        [TestCase(100, -100, 0d)]
        [TestCase(1000, 0, 0d)]
        [TestCase(1000, 100, 0.1d)]
        [TestCase(1000, 500, 0.5d)]
        [TestCase(10000, 500, 0.05d)]
        [TestCase(10000, 9000, 0.9d)]
        [TestCase(10000, 9500, 0.95d)]
        [TestCase(10000, 10000, 1d)]
        [TestCase(10000, 11000, 1d)]
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

            Assert.AreEqual(expectedProgress, sut.Progress);
        }

        [Test]
        public void is_active_is_false_if_there_is_no_execution_context()
        {
            var scheduler = new TestSchedulerService();
            var sut = new ExerciseViewModelBuilder()
                .WithSchedulerService(scheduler)
                .Build();

            scheduler.Start();
            Assert.False(sut.IsActive);
        }

        [Test]
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

        [Test]
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