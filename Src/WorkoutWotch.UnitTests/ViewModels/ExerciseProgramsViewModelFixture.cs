namespace WorkoutWotch.UnitTests.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using Builders;
    using Microsoft.Reactive.Testing;
    using PCLMock;
    using WorkoutWotch.UnitTests.Reactive;
    using WorkoutWotch.UnitTests.Services.ExerciseDocument.Mocks;
    using WorkoutWotch.UnitTests.Services.State.Mocks;
    using WorkoutWotch.ViewModels;
    using Xunit;

    public sealed class ExerciseProgramsViewModelFixture
    {
        [Fact]
        public void parse_error_message_is_null_by_default()
        {
            var sut = new ExerciseProgramsViewModelBuilder()
                .Build();

            Assert.Null(sut.ParseErrorMessage);
        }

        [Fact]
        public void parse_error_message_returns_appropriate_message_if_the_document_could_not_be_parsed()
        {
            var document = @"# First Program

## First Exercise

 whatever!";
            var scheduler = new TestScheduler();

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithCloudDocument(document)
                .WithScheduler(scheduler)
                .Build();

            scheduler.AdvanceMinimal();
            Assert.NotNull(sut.ParseErrorMessage);
            Assert.Equal("Parsing failure: unexpected '#'; expected end of input (Line 3, Column 1); recently consumed:  Program\n\n", sut.ParseErrorMessage);
        }

        [Fact]
        public void parse_error_message_is_null_if_a_document_is_successfully_parsed_after_one_fails_to_parse()
        {
            var badDocument = @"# First Program

## First Exercise

 whatever!";
            var goodDocument = "# First Program";
            var documents = new Subject<string>();
            var exerciseDocumentService = new ExerciseDocumentServiceMock();
            var scheduler = new TestScheduler();

            exerciseDocumentService
                .When(x => x.ExerciseDocument)
                .Return(documents);

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithExerciseDocumentService(exerciseDocumentService)
                .WithScheduler(scheduler)
                .Build();

            documents.OnNext(badDocument);

            scheduler.AdvanceMinimal();
            Assert.NotNull(sut.ParseErrorMessage);

            documents.OnNext(goodDocument);
            scheduler.AdvanceMinimal();
            Assert.Null(sut.ParseErrorMessage);
        }

        [Fact]
        public void programs_yields_any_successfully_parsed_programs()
        {
            var document = "# First Program";
            var scheduler = new TestScheduler();

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithCloudDocument(document)
                .WithScheduler(scheduler)
                .Build();

            scheduler.AdvanceMinimal();
            Assert.NotNull(sut.Programs);
            Assert.Equal(1, sut.Programs.Count);
            Assert.Equal("First Program", sut.Programs.ElementAt(0).Name);
        }

        [Fact]
        public void programs_is_null_if_both_cache_and_cloud_are_empty()
        {
            var scheduler = new TestScheduler();

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithScheduler(scheduler)
                .Build();

            scheduler.AdvanceMinimal();
            Assert.Null(sut.Programs);
        }

        [Fact]
        public void programs_is_populated_from_cache_if_cache_is_populated_and_cloud_is_empty()
        {
            var document = "# First Program";
            var scheduler = new TestScheduler();

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithScheduler(scheduler)
                .WithCachedDocument(document)
                .Build();

            scheduler.AdvanceMinimal();
            Assert.NotNull(sut.Programs);
            Assert.Equal(1, sut.Programs.Count);
        }

        [Fact]
        public void programs_is_populated_from_cloud_if_cache_is_empty_and_cloud_is_populated()
        {
            var document = "# First Program";
            var scheduler = new TestScheduler();

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithCloudDocument(document)
                .WithScheduler(scheduler)
                .Build();

            scheduler.AdvanceMinimal();
            Assert.NotNull(sut.Programs);
            Assert.Equal(1, sut.Programs.Count);
        }

        [Fact]
        public void programs_is_populated_from_cloud_if_cache_errors_and_cloud_is_populated()
        {
            var document = "# First Program";
            var stateService = new StateServiceMock();
            var scheduler = new TestScheduler();

            stateService
                .When(x => x.Get<string>(It.IsAny<string>()))
                .Return(Observable.Throw<string>(new InvalidOperationException()));

            stateService
                .When(x => x.Set<string>(It.IsAny<string>(), It.IsAny<string>()))
                .Return(Observable.Return(Unit.Default));

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithCloudDocument(document)
                .WithScheduler(scheduler)
                .WithStateService(stateService)
                .Build();

            scheduler.AdvanceMinimal();
            Assert.NotNull(sut.Programs);
            Assert.Equal(1, sut.Programs.Count);
        }

        [Fact]
        public void programs_is_populated_from_cloud_if_cache_is_populated_and_cloud_is_populated()
        {
            var cacheDocument = "# First Program";
            var cloudDocument = @"
# First Program
# Second Program";
            var scheduler = new TestScheduler();

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithCloudDocument(cloudDocument)
                .WithCachedDocument(cacheDocument)
                .WithScheduler(scheduler)
                .Build();

            scheduler.AdvanceMinimal();
            Assert.NotNull(sut.Programs);
            Assert.Equal(2, sut.Programs.Count);
        }

        [Fact]
        public void programs_is_populated_from_cache_whilst_document_from_cloud_loads()
        {
            var cacheDocument = "# First Program";
            var cloudDocument = @"
# First Program
# Second Program";
            var scheduler = new TestScheduler();

            var exerciseDocumentService = new ExerciseDocumentServiceMock(MockBehavior.Loose);

            exerciseDocumentService
                .When(x => x.ExerciseDocument)
                .Return(
                    Observable
                        .Return(cloudDocument)
                        .Delay(TimeSpan.FromSeconds(3), scheduler));

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithExerciseDocumentService(exerciseDocumentService)
                .WithCachedDocument(cacheDocument)
                .WithScheduler(scheduler)
                .Build();

            scheduler.AdvanceMinimal();

            Assert.NotNull(sut.Programs);
            Assert.Equal(1, sut.Programs.Count);

            scheduler.AdvanceBy(TimeSpan.FromSeconds(2));
            Assert.NotNull(sut.Programs);
            Assert.Equal(1, sut.Programs.Count);

            scheduler.AdvanceBy(TimeSpan.FromSeconds(2));
            Assert.NotNull(sut.Programs);
            Assert.Equal(2, sut.Programs.Count);
        }

        [Fact]
        public void status_is_parse_failed_if_the_document_could_not_be_parsed()
        {
            var document = @"# First Program

## First Exercise

 whatever!";
            var scheduler = new TestScheduler();

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithCloudDocument(document)
                .WithScheduler(scheduler)
                .Build();

            scheduler.AdvanceMinimal();
            Assert.Equal(ExerciseProgramsViewModelStatus.ParseFailed, sut.Status);
        }

        [Fact]
        public void status_is_loaded_from_cache_if_document_is_successfully_loaded_from_cache()
        {
            var document = "# First Program";
            var scheduler = new TestScheduler();

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithCachedDocument(document)
                .WithScheduler(scheduler)
                .Build();

            scheduler.AdvanceMinimal();
            Assert.Equal(ExerciseProgramsViewModelStatus.LoadedFromCache, sut.Status);
        }

        [Fact]
        public void status_is_loaded_from_cloud_if_document_is_successfully_loaded_from_cloud()
        {
            var document = "# First Program";
            var scheduler = new TestScheduler();

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithCloudDocument(document)
                .WithScheduler(scheduler)
                .Build();

            scheduler.AdvanceMinimal();
            Assert.Equal(ExerciseProgramsViewModelStatus.LoadedFromService, sut.Status);
        }

        [Fact]
        public void status_is_load_failed_if_the_document_fails_to_load_altogether()
        {
            var exerciseDocumentService = new ExerciseDocumentServiceMock();
            var scheduler = new TestScheduler();

            exerciseDocumentService
                .When(x => x.ExerciseDocument)
                .Return(Observable.Throw<string>(new InvalidOperationException()));

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithExerciseDocumentService(exerciseDocumentService)
                .WithScheduler(scheduler)
                .Build();

            scheduler.AdvanceMinimal();
            Assert.Equal(ExerciseProgramsViewModelStatus.LoadFailed, sut.Status);
        }

        [Fact]
        public void document_is_stored_in_cache_if_successfully_loaded_from_cloud()
        {
            var document = "# First Program";
            var scheduler = new TestScheduler();
            var stateService = new StateServiceMock();

            stateService
                .When(x => x.Get<string>(It.IsAny<string>()))
                .Return(Observable.Return<string>(null));

            stateService
                .When(x => x.Set<string>(It.IsAny<string>(), It.IsAny<string>()))
                .Return(Observable.Return(Unit.Default));

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithCloudDocument(document)
                .WithScheduler(scheduler)
                .WithStateService(stateService)
                .Build();

            scheduler.AdvanceMinimal();
            Assert.Equal(ExerciseProgramsViewModelStatus.LoadedFromService, sut.Status);

            stateService
                .Verify(x => x.Set<string>("ExerciseProgramsDocument", document))
                .WasCalledExactlyOnce();
        }

        [Fact]
        public void document_is_not_stored_in_cache_if_loaded_from_cache()
        {
            var document = "# First Program";
            var scheduler = new TestScheduler();
            var stateService = new StateServiceMock();

            stateService
                .When(x => x.Get<string>(It.IsAny<string>()))
                .Return(Observable.Return(document));

            stateService
                .When(x => x.Set<string>(It.IsAny<string>(), It.IsAny<string>()))
                .Return(Observable.Return(Unit.Default));

            var sut = new ExerciseProgramsViewModelBuilder()
                .WithScheduler(scheduler)
                .WithStateService(stateService)
                .Build();

            scheduler.AdvanceMinimal();
            Assert.Equal(ExerciseProgramsViewModelStatus.LoadedFromCache, sut.Status);

            stateService
                .Verify(x => x.Set<string>("ExerciseProgramsDocument", document))
                .WasNotCalled();
        }

        [Fact]
        public void selected_program_instigates_routing_when_set_to_non_null_value()
        {
            var sut = new ExerciseProgramsViewModelBuilder()
                .Build();
            var navigationStack = sut
                .HostScreen
                .Router
                .NavigationStack;

            Assert.Empty(navigationStack);

            var routeTo = new ExerciseProgramViewModelBuilder()
                .Build();
            sut.SelectedProgram = routeTo;

            Assert.Equal(1, navigationStack.Count);
            Assert.Same(routeTo, navigationStack[0]);
        }

        [Fact]
        public void selected_program_does_not_instigate_routing_when_set_to_null_value()
        {
            var sut = new ExerciseProgramsViewModelBuilder()
                .Build();
            var navigationStack = sut
                .HostScreen
                .Router
                .NavigationStack;

            Assert.Empty(navigationStack);

            var routeTo = new ExerciseProgramViewModelBuilder()
                .Build();
            sut.SelectedProgram = routeTo;
            Assert.Equal(1, navigationStack.Count);

            sut.SelectedProgram = null;
            Assert.Equal(1, navigationStack.Count);
        }

        [Fact]
        public void selected_program_resets_to_null_when_returning_to_exercise_programs_view_model()
        {
            var sut = new ExerciseProgramsViewModelBuilder()
                .Build();
            sut.SelectedProgram = new ExerciseProgramViewModelBuilder()
                .Build();
            sut
                .HostScreen
                .Router
                .NavigationStack
                .Add(sut);

            sut
                .HostScreen
                .Router
                .NavigateBack
                .Execute()
                .Subscribe();

            Assert.Null(sut.SelectedProgram);
        }
    }
}