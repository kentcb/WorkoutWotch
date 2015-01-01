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
        public void ctor_throws_if_model_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseViewModel(null, Observable.Never<ExecutionContext>()));
        }

        [Test]
        public void ctor_throws_if_execution_context_is_null()
        {
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            Assert.Throws<ArgumentNullException>(() => new ExerciseViewModel(model, null));
        }

        [TestCase("Name")]
        [TestCase("Another name")]
        [TestCase("Yet another wacky &*(&!^^@9  \t823 name")]
        public void name_returns_name_in_model(string name)
        {
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), name, 1, 1, Enumerable.Empty<MatcherWithAction>());
            var sut = new ExerciseViewModel(model, Observable.Never<ExecutionContext>());

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
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, matchersWithActions);
            var sut = new ExerciseViewModel(model, Observable.Never<ExecutionContext>());

            Assert.AreEqual(TimeSpan.FromMilliseconds(durationInMs), sut.Duration);
        }

        [Test]
        public void progress_is_zero_if_there_is_no_execution_context()
        {
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var sut = new ExerciseViewModel(model, Observable.Never<ExecutionContext>());

            Assert.AreEqual(TimeSpan.Zero, sut.Progress);
        }

        [Test]
        public void progress_is_zero_if_no_progress_has_been_made_through_this_exercise()
        {
            var model1 = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var model2 = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var executionContext = new ExecutionContext();
            var sut = new ExerciseViewModel(model2, Observable.Return(executionContext));

            executionContext.SetCurrentExercise(model1);
            executionContext.AddProgress(TimeSpan.FromSeconds(3));

            Assert.AreEqual(TimeSpan.Zero, sut.Progress);
        }

        [Test]
        public void progress_reflects_any_progression_through_the_exercise()
        {
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var executionContext = new ExecutionContext();
            var sut = new ExerciseViewModel(model, Observable.Return(executionContext));

            executionContext.SetCurrentExercise(model);

            Assert.AreEqual(TimeSpan.Zero, sut.Progress);

            executionContext.AddProgress(TimeSpan.FromSeconds(3));
            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.Progress);

            executionContext.AddProgress(TimeSpan.FromSeconds(2));
            Assert.AreEqual(TimeSpan.FromSeconds(5), sut.Progress);
        }

        [Test]
        public void progress_is_not_reset_to_zero_if_another_exercise_is_started()
        {
            var model1 = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var model2 = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var executionContext = new ExecutionContext();
            var sut = new ExerciseViewModel(model1, Observable.Return(executionContext));

            executionContext.SetCurrentExercise(model1);
            executionContext.AddProgress(TimeSpan.FromSeconds(3));

            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.Progress);

            executionContext.SetCurrentExercise(model2);
            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.Progress);

            executionContext.AddProgress(TimeSpan.FromSeconds(3));
            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.Progress);
        }

        [Test]
        public void progress_is_reset_to_zero_if_the_execution_context_changes()
        {
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var executionContext = new ExecutionContext();
            var executionContextSubject = new Subject<ExecutionContext>();
            var sut = new ExerciseViewModel(model, executionContextSubject);

            executionContextSubject.OnNext(executionContext);
            executionContext.SetCurrentExercise(model);

            executionContext.AddProgress(TimeSpan.FromSeconds(3));
            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.Progress);

            executionContextSubject.OnNext(new ExecutionContext());
            Assert.AreEqual(TimeSpan.Zero, sut.Progress);
        }

        [Test]
        public void progress_is_reset_to_zero_if_the_execution_context_changes_to_null()
        {
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var executionContext = new ExecutionContext();
            var executionContextSubject = new Subject<ExecutionContext>();
            var sut = new ExerciseViewModel(model, executionContextSubject);

            executionContextSubject.OnNext(executionContext);
            executionContext.SetCurrentExercise(model);

            executionContext.AddProgress(TimeSpan.FromSeconds(3));
            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.Progress);

            executionContextSubject.OnNext(null);
            Assert.AreEqual(TimeSpan.Zero, sut.Progress);
        }

        [Test]
        public void is_active_is_false_if_there_is_no_execution_context()
        {
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var sut = new ExerciseViewModel(model, Observable.Never<ExecutionContext>());

            Assert.False(sut.IsActive);
        }

        [Test]
        public void is_active_is_true_if_this_exercise_is_the_current_exercise()
        {
            var model = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var executionContext = new ExecutionContext();
            var sut = new ExerciseViewModel(model, Observable.Return(executionContext));

            Assert.False(sut.IsActive);
            executionContext.SetCurrentExercise(model);
            Assert.True(sut.IsActive);
        }

        [Test]
        public void is_active_is_false_if_this_exercise_is_not_the_current_exercise()
        {
            var model1 = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var model2 = new Exercise(new LoggerServiceMock(MockBehavior.Loose), new SpeechServiceMock(), "Name", 1, 1, Enumerable.Empty<MatcherWithAction>());
            var executionContext = new ExecutionContext();
            var sut = new ExerciseViewModel(model1, Observable.Return(executionContext));

            Assert.False(sut.IsActive);
            executionContext.SetCurrentExercise(model2);
            Assert.False(sut.IsActive);
        }
    }
}