namespace WorkoutWotch.UnitTests.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using ReactiveUI;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Models.EventMatchers;
    using WorkoutWotch.Models.Events;
    using WorkoutWotch.Services.Delay;
    using WorkoutWotch.UnitTests.Models.Mocks;
    using WorkoutWotch.UnitTests.Reactive;
    using WorkoutWotch.UnitTests.Services.Delay.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;
    using WorkoutWotch.ViewModels;

    [TestFixture]
    public class ExerciseProgramViewModelFixture
    {
        [Test]
        public void ctor_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgramViewModel(null, new TestSchedulerService(), new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), "name", Enumerable.Empty<Exercise>())));
        }

        [Test]
        public void ctor_throws_if_scheduler_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgramViewModel(new LoggerServiceMock(), null, new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), "name", Enumerable.Empty<Exercise>())));
        }

        [Test]
        public void ctor_throws_if_model_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgramViewModel(new LoggerServiceMock(), new TestSchedulerService(), null));
        }

        [TestCase("Name")]
        [TestCase("Some name")]
        [TestCase("Another name")]
        public void name_returns_name_in_model(string name)
        {
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var model = new ExerciseProgram(loggerService, name, Enumerable.Empty<Exercise>());
            var sut = new ExerciseProgramViewModel(loggerService, new TestSchedulerService(), model);
            Assert.AreEqual(name, sut.Name);
        }

        [TestCase(239)]
        [TestCase(1)]
        [TestCase(232389)]
        public void duration_returns_duration_in_model(int durationInMs)
        {
            var duration = TimeSpan.FromMilliseconds(durationInMs);
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var speechService = new SpeechServiceMock();
            var delayService = new DelayServiceMock();
            var exercise = new Exercise(
               loggerService,
               speechService,
               "Exercise name",
               1,
               1,
               new [] { new MatcherWithAction(new TypedEventMatcher<BeforeExerciseEvent>(), new WaitAction(delayService, duration)) });
            var model = new ExerciseProgram(loggerService, "Name", new [] { exercise });
            var sut = new ExerciseProgramViewModel(loggerService, new TestSchedulerService(), model);

            Assert.AreEqual(duration, sut.Duration);
        }

        [Test]
        public void exercises_returns_exercises_in_model()
        {
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var speechService = new SpeechServiceMock();
            var exercise1 = new Exercise(
                loggerService,
                speechService,
                "Exercise 1",
                1,
                1,
                Enumerable.Empty<MatcherWithAction>());
            var exercise2 = new Exercise(
                loggerService,
                speechService,
                "Exercise 2",
                1,
                1,
                Enumerable.Empty<MatcherWithAction>());
            var model = new ExerciseProgram(loggerService, "Name", new [] { exercise1, exercise2 });
            var sut = new ExerciseProgramViewModel(loggerService, new TestSchedulerService(), model);

            Assert.NotNull(sut.Exercises);
            Assert.AreEqual(2, sut.Exercises.Count);
//            Assert.AreEqual("Exercise 1", sut.Exercises[0].Name);
//            Assert.AreEqual("Exercise 2", sut.Exercises[1].Name);
        }

        [Test]
        public void is_started_is_false_by_default()
        {
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var sut = new ExerciseProgramViewModel(loggerService, new TestSchedulerService(), new ExerciseProgram(loggerService, "Name", Enumerable.Empty<Exercise>()));
            Assert.False(sut.IsStarted);
        }

        [Test]
        public async Task is_started_cycles_correctly_if_start_command_is_executed()
        {
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var scheduler = new TestSchedulerService();
            var sut = new ExerciseProgramViewModel(loggerService, scheduler, new ExerciseProgram(loggerService, "Name", Enumerable.Empty<Exercise>()));

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

        [Test]
        public void is_start_visible_is_true_by_default()
        {
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var sut = new ExerciseProgramViewModel(loggerService, new TestSchedulerService(), new ExerciseProgram(loggerService, "Name", Enumerable.Empty<Exercise>()));
            Assert.True(sut.IsStartVisible);
        }

        [Test]
        public async Task is_start_visible_cycles_correctly_if_start_command_is_executed()
        {
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var scheduler = new TestSchedulerService();
            var sut = new ExerciseProgramViewModel(loggerService, scheduler, new ExerciseProgram(loggerService, "Name", Enumerable.Empty<Exercise>()));

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

        [Test]
        public void is_paused_is_false_by_default()
        {
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var sut = new ExerciseProgramViewModel(loggerService, new TestSchedulerService(), new ExerciseProgram(loggerService, "Name", Enumerable.Empty<Exercise>()));
            Assert.False(sut.IsPaused);
        }

        [Test]
        public async Task is_paused_cycles_correctly_if_pause_command_is_executed()
        {
            var action = new WaitAction(new DelayService(), TimeSpan.FromMinutes(1));
            var matchersWithActions = new []
            {
                new MatcherWithAction(new TypedEventMatcher<BeforeExerciseEvent>(), action)
            };
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var scheduler = new TestSchedulerService();
            var exercise = new Exercise(loggerService, new SpeechServiceMock(MockBehavior.Loose), "Name", 1, 1, matchersWithActions);
            var sut = new ExerciseProgramViewModel(loggerService, scheduler, new ExerciseProgram(loggerService, "Name", new [] { exercise }));

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

        [Test]
        public void is_pause_visible_is_false_by_default()
        {
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var sut = new ExerciseProgramViewModel(loggerService, new TestSchedulerService(), new ExerciseProgram(loggerService, "Name", Enumerable.Empty<Exercise>()));
            Assert.False(sut.IsPauseVisible);
        }

        [Test]
        public async Task is_pause_visible_cycles_correctly_if_start_command_is_executed()
        {
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var scheduler = new TestSchedulerService();
            var sut = new ExerciseProgramViewModel(loggerService, scheduler, new ExerciseProgram(loggerService, "Name", Enumerable.Empty<Exercise>()));

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

        [Test]
        public async Task is_pause_visible_cycles_correctly_if_pause_command_is_executed()
        {
            var action = new WaitAction(new DelayService(), TimeSpan.FromMinutes(1));
            var matchersWithActions = new []
            {
                new MatcherWithAction(new TypedEventMatcher<BeforeExerciseEvent>(), action)
            };
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var scheduler = new TestSchedulerService();
            var exercise = new Exercise(loggerService, new SpeechServiceMock(MockBehavior.Loose), "Name", 1, 1, matchersWithActions);
            var sut = new ExerciseProgramViewModel(loggerService, scheduler, new ExerciseProgram(loggerService, "Name", new [] { exercise }));

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

        [Test]
        public void is_resume_visible_is_false_by_default()
        {
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var sut = new ExerciseProgramViewModel(loggerService, new TestSchedulerService(), new ExerciseProgram(loggerService, "Name", Enumerable.Empty<Exercise>()));
            Assert.False(sut.IsResumeVisible);
        }

        [Test]
        public async Task is_resume_visible_cycles_correctly_if_start_command_is_executed_and_execution_is_paused()
        {
            var action = new WaitAction(new DelayService(), TimeSpan.FromMinutes(1));
            var matchersWithActions = new []
            {
                new MatcherWithAction(new TypedEventMatcher<BeforeExerciseEvent>(), action)
            };
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var scheduler = new TestSchedulerService();
            var exercise = new Exercise(loggerService, new SpeechServiceMock(MockBehavior.Loose), "Name", 1, 1, matchersWithActions);
            var sut = new ExerciseProgramViewModel(loggerService, scheduler, new ExerciseProgram(loggerService, "Name", new [] { exercise }));

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

        [Test]
        public async Task progress_time_span_is_updated_throughout_execution()
        {
            var action = new ActionMock(MockBehavior.Loose);
            action
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec =>
                {
                    ec.AddProgress(TimeSpan.FromSeconds(1));
                    ec.AddProgress(TimeSpan.FromSeconds(3));
                })
                .Return(Task.FromResult(true));
            var matchersWithActions = new []
            {
                new MatcherWithAction(new TypedEventMatcher<BeforeExerciseEvent>(), action)
            };
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var scheduler = new TestSchedulerService();
            var exercise = new Exercise(loggerService, new SpeechServiceMock(MockBehavior.Loose), "Name", 1, 1, matchersWithActions);
            var sut = new ExerciseProgramViewModel(loggerService, scheduler, new ExerciseProgram(loggerService, "Name", new [] { exercise }));

            var progressTimeSpanTask = sut
                .WhenAnyValue(x => x.ProgressTimeSpan)
                .Take(3)
                .ToListAsync()
                .ToTask();

            using (scheduler.Pump())
            {
                sut.StartCommand.Execute(null);

                var progressTimeSpan = await progressTimeSpanTask;

                Assert.AreEqual(TimeSpan.Zero, progressTimeSpan[0]);
                Assert.AreEqual(TimeSpan.FromSeconds(1), progressTimeSpan[1]);
                Assert.AreEqual(TimeSpan.FromSeconds(4), progressTimeSpan[2]);
            }
        }

        [Test]
        public async Task progress_is_updated_throughout_execution()
        {
            var action = new ActionMock(MockBehavior.Loose);
            action.When(x => x.Duration).Return(TimeSpan.FromMinutes(1));
            action
                .When(x => x.ExecuteAsync(It.IsAny<ExecutionContext>()))
                .Do<ExecutionContext>(ec =>
                {
                    ec.AddProgress(TimeSpan.FromSeconds(15));
                    ec.AddProgress(TimeSpan.FromSeconds(30));
                })
                .Return(Task.FromResult(true));
            var matchersWithActions = new []
            {
                new MatcherWithAction(new TypedEventMatcher<BeforeExerciseEvent>(), action)
            };
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var scheduler = new TestSchedulerService();
            var exercise = new Exercise(loggerService, new SpeechServiceMock(MockBehavior.Loose), "Name", 1, 1, matchersWithActions);
            var sut = new ExerciseProgramViewModel(loggerService, scheduler, new ExerciseProgram(loggerService, "Name", new [] { exercise }));

            var progressTask = sut
                .WhenAnyValue(x => x.Progress)
                .Take(3)
                .ToListAsync()
                .ToTask();

            using (scheduler.Pump())
            {
                sut.StartCommand.Execute(null);

                var progress = await progressTask;

                Assert.AreEqual(0, progress[0]);
                Assert.AreEqual(0.25, progress[1]);
                Assert.AreEqual(0.75, progress[2]);
            }
        }

        [Test]
        public async Task skip_backwards_command_is_disabled_if_on_first_exercise()
        {
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
            var matchersWithActions = new []
            {
                new MatcherWithAction(new TypedEventMatcher<BeforeExerciseEvent>(), action)
            };
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var scheduler = new TestSchedulerService();
            var exercise = new Exercise(loggerService, new SpeechServiceMock(MockBehavior.Loose), "Name", 1, 1, matchersWithActions.ToList());
            var exerciseProgram = new ExerciseProgram(loggerService, "Name", new [] { exercise });
            var sut = new ExerciseProgramViewModel(loggerService, scheduler, exerciseProgram);

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

        [Test]
        public async Task skip_backwards_command_is_enabled_if_sufficient_progress_has_been_made_through_first_exercise()
        {
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
            var matchersWithActions = new []
            {
                new MatcherWithAction(new TypedEventMatcher<BeforeExerciseEvent>(), action),
            };
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var scheduler = new TestSchedulerService();
            var exercise = new Exercise(loggerService, new SpeechServiceMock(MockBehavior.Loose), "Name", 1, 1, matchersWithActions.ToList());
            var exerciseProgram = new ExerciseProgram(loggerService, "Name", new [] { exercise });
            var sut = new ExerciseProgramViewModel(loggerService, scheduler, exerciseProgram);

            using (scheduler.Pump())
            {
                sut.StartCommand.Execute(null);

                await sut.SkipBackwardsCommand.CanExecuteObservable
                    .Where(x => x)
                    .Timeout(TimeSpan.FromSeconds(3))
                    .FirstAsync();
            }
        }

        [Test]
        public async Task skip_backwards_command_restarts_the_execution_context_from_the_start_of_the_current_exercise_if_sufficient_progress_has_been_made()
        {
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
            var matchersWithActions = new []
            {
                new MatcherWithAction(new TypedEventMatcher<BeforeExerciseEvent>(), action),
            };
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var scheduler = new TestSchedulerService();
            var exercise = new Exercise(loggerService, new SpeechServiceMock(MockBehavior.Loose), "Name", 1, 1, matchersWithActions.ToList());
            var exerciseProgram = new ExerciseProgram(loggerService, "Name", new [] { exercise });
            var sut = new ExerciseProgramViewModel(loggerService, scheduler, exerciseProgram);

            var progressTask = sut.WhenAnyValue(x => x.ProgressTimeSpan)
                .Take(3)
                .ToListAsync()
                .ToTask();

            using (scheduler.Pump())
            {
                sut.StartCommand.Execute(null);

                await sut.SkipBackwardsCommand.CanExecuteObservable
                    .Where(x => x)
                    .Timeout(TimeSpan.FromSeconds(3))
                    .FirstAsync();

                sut.SkipBackwardsCommand.Execute(null);

                var progress = await progressTask;

                Assert.AreEqual(TimeSpan.Zero, progress[0]);
                Assert.AreEqual(TimeSpan.FromSeconds(4), progress[1]);
                Assert.AreEqual(TimeSpan.Zero, progress[2]);
            }
        }

        [Test]
        public async Task skip_backwards_command_restarts_the_execution_context_from_the_start_of_the_previous_exercise_if_the_current_exercise_if_only_recently_started()
        {
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
            var matchersWithActions = new []
            {
                new MatcherWithAction(new TypedEventMatcher<BeforeExerciseEvent>(), action),
            };
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var scheduler = new TestSchedulerService();
            var exercise1 = new Exercise(loggerService, new SpeechServiceMock(MockBehavior.Loose), "Exercise 1", 1, 1, matchersWithActions.ToList());
            var exercise2 = new Exercise(loggerService, new SpeechServiceMock(MockBehavior.Loose), "Exercise 2", 1, 1, matchersWithActions.ToList());
            var exerciseProgram = new ExerciseProgram(new LoggerServiceMock(MockBehavior.Loose), "Name", new [] { exercise1, exercise2 });
            var sut = new ExerciseProgramViewModel(loggerService, scheduler, exerciseProgram);

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

                await sut.SkipBackwardsCommand.CanExecuteObservable
                    .Where(x => x)
                    .Timeout(TimeSpan.FromSeconds(3))
                    .FirstAsync();

                sut.SkipBackwardsCommand.Execute(null);

                var progress = await progressTask;

                Assert.AreEqual(TimeSpan.Zero, progress[0]);
                Assert.AreEqual(TimeSpan.FromSeconds(10), progress[1]);
                Assert.AreEqual(TimeSpan.FromSeconds(10.5), progress[2]);
                Assert.AreEqual(TimeSpan.Zero, progress[3]);
            }
        }

        [Test]
        public async Task skip_forwards_command_is_disabled_if_on_last_exercise()
        {
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
            var matchersWithActions = new []
            {
                new MatcherWithAction(new TypedEventMatcher<BeforeExerciseEvent>(), action),
            };
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var scheduler = new TestSchedulerService();
            var exercise1 = new Exercise(loggerService, new SpeechServiceMock(MockBehavior.Loose), "Exercise 1", 1, 1, matchersWithActions.ToList());
            var exercise2 = new Exercise(loggerService, new SpeechServiceMock(MockBehavior.Loose), "Exercise 2", 1, 1, matchersWithActions.ToList());
            var exerciseProgram = new ExerciseProgram(loggerService, "Name", new [] { exercise1, exercise2 });
            var sut = new ExerciseProgramViewModel(loggerService, scheduler, exerciseProgram);

            using (scheduler.Pump())
            {
                sut.StartCommand.Execute(null);

                await sut.SkipForwardsCommand.CanExecuteObservable
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

        [Test]
        public async Task skip_forwards_command_skips_to_the_next_exercise()
        {
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
            var matchersWithActions = new []
            {
                new MatcherWithAction(new TypedEventMatcher<BeforeExerciseEvent>(), action),
            };
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var scheduler = new TestSchedulerService();
            var exercise1 = new Exercise(loggerService, new SpeechServiceMock(MockBehavior.Loose), "Exercise 1", 1, 1, matchersWithActions.ToList());
            var exercise2 = new Exercise(loggerService, new SpeechServiceMock(MockBehavior.Loose), "Exercise 2", 1, 1, matchersWithActions.ToList());
            var exerciseProgram = new ExerciseProgram(loggerService, "Name", new [] { exercise1, exercise2 });
            var sut = new ExerciseProgramViewModel(loggerService, scheduler, exerciseProgram);

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

                await sut.SkipForwardsCommand.CanExecuteObservable
                    .Where(x => x)
                    .Timeout(TimeSpan.FromSeconds(3))
                    .FirstAsync();

                sut.SkipForwardsCommand.Execute(null);

                var progress = await progressTask;

                Assert.AreEqual(TimeSpan.Zero, progress[0]);
                Assert.AreEqual(TimeSpan.FromSeconds(10), progress[1]);
            }
        }

        [Test]
        public async Task current_exercise_reflects_that_in_the_execution_context()
        {
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
            var matchersWithActions = new []
            {
                new MatcherWithAction(new TypedEventMatcher<BeforeExerciseEvent>(), action),
            };
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            var scheduler = new TestSchedulerService();
            var exercise1 = new Exercise(loggerService, new SpeechServiceMock(MockBehavior.Loose), "Exercise 1", 1, 1, matchersWithActions.ToList());
            var exercise2 = new Exercise(loggerService, new SpeechServiceMock(MockBehavior.Loose), "Exercise 2", 1, 1, matchersWithActions.ToList());
            var exercise3 = new Exercise(loggerService, new SpeechServiceMock(MockBehavior.Loose), "Exercise 3", 1, 1, matchersWithActions.ToList());
            var exerciseProgram = new ExerciseProgram(loggerService, "Name", new [] { exercise1, exercise2, exercise3 });
            var sut = new ExerciseProgramViewModel(loggerService, scheduler, exerciseProgram);

            var currentExercisesTask = sut.WhenAnyValue(x => x.CurrentExercise)
                .Where(x => x != null)
                .DistinctUntilChanged()
                .Take(4)
                .ToListAsync()
                .ToTask();

            using (scheduler.Pump())
            {
                sut.StartCommand.Execute(null);

                await sut.SkipForwardsCommand.CanExecuteObservable
                    .Where(x => x)
                    .Timeout(TimeSpan.FromSeconds(3))
                    .FirstAsync();

                sut.SkipForwardsCommand.Execute(null);

                await sut.SkipForwardsCommand.CanExecuteObservable
                    .Where(x => x)
                    .Timeout(TimeSpan.FromSeconds(3))
                    .FirstAsync();

                sut.SkipForwardsCommand.Execute(null);

                await sut.SkipBackwardsCommand.CanExecuteObservable
                    .Where(x => x)
                    .Timeout(TimeSpan.FromSeconds(3))
                    .FirstAsync();

                sut.SkipBackwardsCommand.Execute(null);

                var currentExercises = await currentExercisesTask;

//                Assert.AreEqual("Exercise 1", currentExercises[0].Name);
//                Assert.AreEqual("Exercise 2", currentExercises[1].Name);
//                Assert.AreEqual("Exercise 3", currentExercises[2].Name);
//                Assert.AreEqual("Exercise 2", currentExercises[3].Name);
            }
        }
    }
}