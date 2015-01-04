using WorkoutWotch.UnitTests.Reactive;
using System.Threading.Tasks;
using ReactiveUI;

namespace WorkoutWotch.UnitTests.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.EventMatchers;
    using WorkoutWotch.Models.Events;
    using WorkoutWotch.UnitTests.Models.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;
    using WorkoutWotch.ViewModels;

    [TestFixture]
    public class ExerciseViewModelFixture
    {
        [Test]
        public void ctor_throws_if_scheduler_service_is_null()
        {
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
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
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            Assert.Throws<ArgumentNullException>(() => new ExerciseViewModel(new TestSchedulerService(), model, null));
        }

        [TestCase("Name")]
        [TestCase("Another name")]
        [TestCase("Yet another wacky &*(&!^^@9  \t823 name")]
        public void name_returns_name_in_model(string name)
        {
            var scheduler = new TestSchedulerService();
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), name, 1, 1, Enumerable.Empty<MatcherWithAction>());
            var sut = new ExerciseViewModel(scheduler, model, Observable.Never<ExecutionContext>());

            Assert.AreEqual(name, sut.Name);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(1000)]
        [TestCase(34871)]
        public void duration_returns_duration_in_model(int durationInMs)
        {
            var action = new ActionMock(MockBehavior.Loose);
            action.When(x => x.Duration).Return(TimeSpan.FromMilliseconds(durationInMs));
            var matchersWithActions = new []
            {
                new MatcherWithAction(new TypedEventMatcher<BeforeExerciseEvent>(), action)
            };
            var scheduler = new TestSchedulerService();
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, matchersWithActions);
            var sut = new ExerciseViewModel(scheduler, model, Observable.Never<ExecutionContext>());

            Assert.AreEqual(TimeSpan.FromMilliseconds(durationInMs), sut.Duration);
        }

        [Test]
        public void progress_time_span_is_zero_if_there_is_no_execution_context()
        {
            var scheduler = new TestSchedulerService();
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var sut = new ExerciseViewModel(scheduler, model, Observable.Never<ExecutionContext>());

            Assert.AreEqual(TimeSpan.Zero, sut.ProgressTimeSpan);
        }

        [Test]
        public void progress_time_span_is_zero_if_no_progress_has_been_made_through_this_exercise()
        {
            var scheduler = new TestSchedulerService();
            var model1 = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var model2 = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var executionContext = new ExecutionContext();
            var sut = new ExerciseViewModel(scheduler, model2, Observable.Return(executionContext));

            executionContext.SetCurrentExercise(model1);
            executionContext.AddProgress(TimeSpan.FromSeconds(3));

            Assert.AreEqual(TimeSpan.Zero, sut.ProgressTimeSpan);
        }

        [Test]
        public void progress_time_span_reflects_any_progression_through_the_exercise()
        {
            var scheduler = new TestSchedulerService();
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var executionContext = new ExecutionContext();
            var sut = new ExerciseViewModel(scheduler, model, Observable.Return(executionContext));

            executionContext.SetCurrentExercise(model);

            Assert.AreEqual(TimeSpan.Zero, sut.ProgressTimeSpan);

            executionContext.AddProgress(TimeSpan.FromSeconds(3));
            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.ProgressTimeSpan);

            executionContext.AddProgress(TimeSpan.FromSeconds(2));
            Assert.AreEqual(TimeSpan.FromSeconds(5), sut.ProgressTimeSpan);
        }

        [Test]
        public void progress_time_span_is_not_reset_to_zero_if_another_exercise_is_started()
        {
            var scheduler = new TestSchedulerService();
            var model1 = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var model2 = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var executionContext = new ExecutionContext();
            var sut = new ExerciseViewModel(scheduler, model1, Observable.Return(executionContext));

            executionContext.SetCurrentExercise(model1);
            executionContext.AddProgress(TimeSpan.FromSeconds(3));

            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.ProgressTimeSpan);

            executionContext.SetCurrentExercise(model2);
            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.ProgressTimeSpan);

            executionContext.AddProgress(TimeSpan.FromSeconds(3));
            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.ProgressTimeSpan);
        }

        [Test]
        public void progress_time_span_is_reset_to_zero_if_the_execution_context_changes()
        {
            var scheduler = new TestSchedulerService();
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var executionContext = new ExecutionContext();
            var executionContextSubject = new Subject<ExecutionContext>();
            var sut = new ExerciseViewModel(scheduler, model, executionContextSubject);

            executionContextSubject.OnNext(executionContext);
            executionContext.SetCurrentExercise(model);

            executionContext.AddProgress(TimeSpan.FromSeconds(3));
            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.ProgressTimeSpan);

            executionContextSubject.OnNext(new ExecutionContext());
            Assert.AreEqual(TimeSpan.Zero, sut.ProgressTimeSpan);
        }

        [Test]
        public void progress_time_span_is_reset_to_zero_if_the_execution_context_changes_to_null()
        {
            var scheduler = new TestSchedulerService();
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var executionContext = new ExecutionContext();
            var executionContextSubject = new Subject<ExecutionContext>();
            var sut = new ExerciseViewModel(scheduler, model, executionContextSubject);

            executionContextSubject.OnNext(executionContext);
            executionContext.SetCurrentExercise(model);

            executionContext.AddProgress(TimeSpan.FromSeconds(3));
            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.ProgressTimeSpan);

            executionContextSubject.OnNext(null);
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
        public async Task progress_is_calculated_based_on_duration_and_progress_time_span(int durationInMs, int progressInMs, double expectedProgress)
        {
            var action = new ActionMock(MockBehavior.Loose);
            action.When(x => x.Duration).Return(TimeSpan.FromMilliseconds(durationInMs));
            var matchersWithActions = new []
            {
                new MatcherWithAction(new TypedEventMatcher<BeforeExerciseEvent>(), action)
            };
            var scheduler = new TestSchedulerService();
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, matchersWithActions);
            var executionContext = new ExecutionContext();
            var sut = new ExerciseViewModel(scheduler, model, Observable.Return(executionContext));

            executionContext.SetCurrentExercise(model);
            executionContext.AddProgress(TimeSpan.FromMilliseconds(progressInMs));

            scheduler.Start();

            Assert.AreEqual(expectedProgress, sut.Progress);
        }

        [Test]
        public void is_active_is_false_if_there_is_no_execution_context()
        {
            var scheduler = new TestSchedulerService();
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var sut = new ExerciseViewModel(scheduler, model, Observable.Never<ExecutionContext>());

            Assert.False(sut.IsActive);
        }

        [Test]
        public void is_active_is_true_if_this_exercise_is_the_current_exercise()
        {
            var scheduler = new TestSchedulerService();
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var executionContext = new ExecutionContext();
            var sut = new ExerciseViewModel(scheduler, model, Observable.Return(executionContext));

            Assert.False(sut.IsActive);
            executionContext.SetCurrentExercise(model);
            Assert.True(sut.IsActive);
        }

        [Test]
        public void is_active_is_false_if_this_exercise_is_not_the_current_exercise()
        {
            var scheduler = new TestSchedulerService();
            var model1 = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var model2 = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var executionContext = new ExecutionContext();
            var sut = new ExerciseViewModel(scheduler, model1, Observable.Return(executionContext));

            Assert.False(sut.IsActive);
            executionContext.SetCurrentExercise(model2);
            Assert.False(sut.IsActive);
        }
    }
}