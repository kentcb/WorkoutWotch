namespace WorkoutWotch.UnitTests.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using WorkoutWotch.UnitTests.Reactive;
    using WorkoutWotch.UnitTests.Services.Container.Mocks;
    using WorkoutWotch.UnitTests.Services.ExerciseDocument.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.State.Mocks;
    using WorkoutWotch.ViewModels;

    [TestFixture]
    public class ExerciseProgramsViewModelFixture
    {
        [Test]
        public void ctor_throws_if_container_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgramsViewModel(null, new ExerciseDocumentServiceMock(), new LoggerServiceMock(), new TestSchedulerService(), new StateServiceMock()));
        }

        [Test]
        public void ctor_throws_if_exercise_document_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgramsViewModel(new ContainerServiceMock(), null, new LoggerServiceMock(), new TestSchedulerService(), new StateServiceMock()));
        }

        [Test]
        public void ctor_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgramsViewModel(new ContainerServiceMock(), new ExerciseDocumentServiceMock(), null, new TestSchedulerService(), new StateServiceMock()));
        }

        [Test]
        public void ctor_throws_if_scheduler_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgramsViewModel(new ContainerServiceMock(), new ExerciseDocumentServiceMock(), new LoggerServiceMock(), null, new StateServiceMock()));
        }

        [Test]
        public void ctor_throws_if_state_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExerciseProgramsViewModel(new ContainerServiceMock(), new ExerciseDocumentServiceMock(), new LoggerServiceMock(), new TestSchedulerService(), null));
        }

        [Test]
        public void parse_error_message_is_null_by_default()
        {
            var sut = new ExerciseProgramsViewModelBuilder().Build();

            Assert.Null(sut.ParseErrorMessage);
        }

        [Test]
        public void parse_error_message_returns_appropriate_message_if_the_document_could_not_be_parsed()
        {
            var document = @"# First Program

## First Exercise

 whatever!";
            var scheduler = new TestSchedulerService();

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithCloudDocument(document)
                .WithSchedulerService(scheduler)
                .Build();

            scheduler.Start();
            Assert.NotNull(sut.ParseErrorMessage);
            Assert.AreEqual("Parsing failure: unexpected '#'; expected end of input (Line 3, Column 1); recently consumed:  Program\n\n", sut.ParseErrorMessage);
        }

        [Test]
        public void parse_error_message_is_null_if_a_document_is_successfully_parsed_after_one_fails_to_parse()
        {
            var badDocument = @"# First Program

## First Exercise

 whatever!";
            var goodDocument = "# First Program";
            var documents = new Subject<string>();
            var exerciseDocumentService = new ExerciseDocumentServiceMock();
            var scheduler = new TestSchedulerService();

            exerciseDocumentService
                .When(x => x.ExerciseDocument)
                .Return(documents);

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithExerciseDocumentService(exerciseDocumentService)
                .WithSchedulerService(scheduler)
                .Build();

            documents.OnNext(badDocument);

            scheduler.Start();
            Assert.NotNull(sut.ParseErrorMessage);

            documents.OnNext(goodDocument);
            scheduler.Start();
            Assert.Null(sut.ParseErrorMessage);
        }

        [Test]
        public void programs_yields_any_successfully_parsed_programs()
        {
            var document = "# First Program";
            var scheduler = new TestSchedulerService();

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithCloudDocument(document)
                .WithSchedulerService(scheduler)
                .Build();

            scheduler.Start();
            Assert.NotNull(sut.Programs);
            Assert.AreEqual(1, sut.Programs.Count);
            Assert.AreEqual("First Program", sut.Programs.ElementAt(0).Name);
        }

        [Test]
        public void programs_is_null_if_both_cache_and_cloud_are_empty()
        {
            var scheduler = new TestSchedulerService();

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithSchedulerService(scheduler)
                .Build();

            scheduler.Start();
            Assert.Null(sut.Programs);
        }

        [Test]
        public void programs_is_populated_from_cache_if_cache_is_populated_and_cloud_is_empty()
        {
            var document = "# First Program";
            var scheduler = new TestSchedulerService();

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithSchedulerService(scheduler)
                .WithCachedDocument(document)
                .Build();

            scheduler.Start();
            Assert.NotNull(sut.Programs);
            Assert.AreEqual(1, sut.Programs.Count);
        }

        [Test]
        public void programs_is_populated_from_cloud_if_cache_is_empty_and_cloud_is_populated()
        {
            var document = "# First Program";
            var scheduler = new TestSchedulerService();

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithCloudDocument(document)
                .WithSchedulerService(scheduler)
                .Build();

            scheduler.Start();
            Assert.NotNull(sut.Programs);
            Assert.AreEqual(1, sut.Programs.Count);
        }

        [Test]
        public void programs_is_populated_from_cloud_if_cache_errors_and_cloud_is_populated()
        {
            var document = "# First Program";
            var stateService = new StateServiceMock();
            var scheduler = new TestSchedulerService();

            stateService
                .When(x => x.GetAsync<string>(It.IsAny<string>()))
                .Return(Task.Run<string>(() => { throw new InvalidOperationException(); }));

            stateService
                .When(x => x.SetAsync<string>(It.IsAny<string>(), It.IsAny<string>()))
                .Return(Task.FromResult(true));

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithCloudDocument(document)
                .WithSchedulerService(scheduler)
                .WithStateService(stateService)
                .Build();

            scheduler.Start();
            Assert.NotNull(sut.Programs);
            Assert.AreEqual(1, sut.Programs.Count);
        }

        [Test]
        public void programs_is_populated_from_cloud_if_cache_is_populated_and_cloud_is_populated()
        {
            var cacheDocument = "# First Program";
            var cloudDocument = @"
# First Program
# Second Program";
            var scheduler = new TestSchedulerService();

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithCloudDocument(cloudDocument)
                .WithCachedDocument(cacheDocument)
                .WithSchedulerService(scheduler)
                .Build();

            scheduler.Start();
            Assert.NotNull(sut.Programs);
            Assert.AreEqual(2, sut.Programs.Count);
        }

        [Test]
        public void status_is_parse_failed_if_the_document_could_not_be_parsed()
        {
            var document = @"# First Program

## First Exercise

 whatever!";
            var scheduler = new TestSchedulerService();

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithCloudDocument(document)
                .WithSchedulerService(scheduler)
                .Build();

            scheduler.Start();
            Assert.AreEqual(ExerciseProgramsViewModelStatus.ParseFailed, sut.Status);
        }

        [Test]
        public void status_is_loaded_from_cache_if_document_is_successfully_loaded_from_cache()
        {
            var document = "# First Program";
            var scheduler = new TestSchedulerService();

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithCachedDocument(document)
                .WithSchedulerService(scheduler)
                .Build();

            scheduler.Start();
            Assert.AreEqual(ExerciseProgramsViewModelStatus.LoadedFromCache, sut.Status);
        }

        [Test]
        public void status_is_loaded_from_cloud_if_document_is_successfully_loaded_from_cloud()
        {
            var document = "# First Program";
            var scheduler = new TestSchedulerService();

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithCloudDocument(document)
                .WithSchedulerService(scheduler)
                .Build();

            scheduler.Start();
            Assert.AreEqual(ExerciseProgramsViewModelStatus.LoadedFromCloud, sut.Status);
        }

        [Test]
        public void status_is_load_failed_if_the_document_fails_to_load_altogether()
        {
            var exerciseDocumentService = new ExerciseDocumentServiceMock();
            var scheduler = new TestSchedulerService();

            exerciseDocumentService
                .When(x => x.ExerciseDocument)
                .Return(Observable.Throw<string>(new InvalidOperationException()));

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithExerciseDocumentService(exerciseDocumentService)
                .WithSchedulerService(scheduler)
                .Build();

            scheduler.Start();
            Assert.AreEqual(ExerciseProgramsViewModelStatus.LoadFailed, sut.Status);
        }

        [Test]
        public void document_is_stored_in_cache_if_successfully_loaded_from_cloud()
        {
            var document = "# First Program";
            var scheduler = new TestSchedulerService();
            var stateService = new StateServiceMock();

            stateService
                .When(x => x.GetAsync<string>(It.IsAny<string>()))
                .Return(Task.FromResult<string>(null));

            stateService
                .When(x => x.SetAsync<string>(It.IsAny<string>(), It.IsAny<string>()))
                .Return(Task.FromResult(true));

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithCloudDocument(document)
                .WithSchedulerService(scheduler)
                .WithStateService(stateService)
                .Build();

            scheduler.Start();
            Assert.AreEqual(ExerciseProgramsViewModelStatus.LoadedFromCloud, sut.Status);

            stateService
                .Verify(x => x.SetAsync<string>("ExerciseProgramsDocument", It.Is(document)))
                .WasCalledExactlyOnce();
        }

        [Test]
        public void document_is_not_stored_in_cache_if_loaded_from_cache()
        {
            var document = "# First Program";
            var scheduler = new TestSchedulerService();
            var stateService = new StateServiceMock();

            stateService
                .When(x => x.GetAsync<string>(It.IsAny<string>()))
                .Return(Task.FromResult<string>(document));

            stateService
                .When(x => x.SetAsync<string>(It.IsAny<string>(), It.IsAny<string>()))
                .Return(Task.FromResult(true));

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithSchedulerService(scheduler)
                .WithStateService(stateService)
                .Build();

            scheduler.Start();
            Assert.AreEqual(ExerciseProgramsViewModelStatus.LoadedFromCache, sut.Status);

            stateService
                .Verify(x => x.SetAsync<string>("ExerciseProgramsDocument", document))
                .WasNotCalled();
        }
    }
}