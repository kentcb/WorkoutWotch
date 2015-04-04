namespace WorkoutWotch.ViewModels
{
    using System;
    using System.Reactive;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using ReactiveUI;
    using Sprache;
    using WorkoutWotch.Models;
    using WorkoutWotch.Services.Contracts.Container;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using WorkoutWotch.Services.Contracts.State;
    using WorkoutWotch.Utility;

    public sealed class ExerciseProgramsViewModel : DisposableReactiveObject
    {
        private const string exerciseProgramsCacheKey = "ExerciseProgramsDocument";

        private readonly IContainerService containerService;
        private readonly IExerciseDocumentService exerciseDocumentService;
        private readonly IStateService stateService;
        private readonly CompositeDisposable disposables;
        private readonly ObservableAsPropertyHelper<IReadOnlyReactiveList<ExerciseProgramViewModel>> programs;
        private readonly ObservableAsPropertyHelper<string> parseErrorMessage;
        private readonly ObservableAsPropertyHelper<ExerciseProgramsViewModelStatus> status;
        private readonly ObservableAsPropertyHelper<ExercisePrograms> model;
        private ExerciseProgramViewModel selectedProgram;

        public ExerciseProgramsViewModel(
            IContainerService containerService,
            IExerciseDocumentService exerciseDocumentService,
            ILoggerService loggerService,
            ISchedulerService schedulerService,
            IStateService stateService)
        {
            containerService.AssertNotNull("containerService");
            exerciseDocumentService.AssertNotNull("exerciseDocumentService");
            loggerService.AssertNotNull("loggerService");
            schedulerService.AssertNotNull("schedulerService");
            stateService.AssertNotNull("stateService");

            this.containerService = containerService;
            this.exerciseDocumentService = exerciseDocumentService;
            this.stateService = stateService;
            this.disposables = new CompositeDisposable();

            var documentsFromCache = this.stateService
                .GetAsync<string>(exerciseProgramsCacheKey)
                .ToObservable()
                .Where(x => x != null)
                .Select(x => new DocumentSourceWith<string>(DocumentSource.Cache, x));

            var documentsFromCloud = this.exerciseDocumentService
                .ExerciseDocument
                .Where(x => x != null)
                .Select(x => new DocumentSourceWith<string>(DocumentSource.Cloud, x));

            var documents = documentsFromCache
                .Catch(Observable.Empty<DocumentSourceWith<string>>())
                .Concat(documentsFromCloud)
                .Publish();

            var safeDocuments = documents
                .Catch(Observable.Empty<DocumentSourceWith<string>>());

            var results = documents
                .Select(x => new DocumentSourceWith<IResult<ExercisePrograms>>(x.Source, ExercisePrograms.TryParse(x.Item, this.containerService)));

            var safeResults = results
                .Catch(Observable.Empty<DocumentSourceWith<IResult<ExercisePrograms>>>());

            this.parseErrorMessage = safeResults
                .Select(x => x.Item.WasSuccessful ? null : x.Item.ToString())
                .ObserveOn(schedulerService.SynchronizationContextScheduler)
                .ToProperty(this, x => x.ParseErrorMessage)
                .AddTo(this.disposables);

            this.status = results
                .Select(x => !x.Item.WasSuccessful ? ExerciseProgramsViewModelStatus.ParseFailed : x.Source == DocumentSource.Cache ? ExerciseProgramsViewModelStatus.LoadedFromCache : ExerciseProgramsViewModelStatus.LoadedFromCloud)
                .Catch(Observable.Return(ExerciseProgramsViewModelStatus.LoadFailed))
                .ObserveOn(schedulerService.SynchronizationContextScheduler)
                .ToProperty(this, x => x.Status)
                .AddTo(this.disposables);

            this.model = safeResults
                .Select(x => x.Item.WasSuccessful ? x.Item.Value : null)
                .ObserveOn(schedulerService.SynchronizationContextScheduler)
                .ToProperty(this, x => x.Model)
                .AddTo(this.disposables);

            this.programs = this.WhenAnyValue(x => x.Model)
                .Select(x => x == null ? null : x.Programs.CreateDerivedCollection(y => new ExerciseProgramViewModel(loggerService, schedulerService, y)))
                .ObserveOn(schedulerService.SynchronizationContextScheduler)
                .ToProperty(this, x => x.Programs)
                .AddTo(this.disposables);

            safeDocuments
                .Where(x => x.Source == DocumentSource.Cloud)
                .SelectMany(
                    async x =>
                    {
                        await this.stateService.SetAsync(exerciseProgramsCacheKey, x.Item);
                        return Unit.Default;
                    })
                .Subscribe()
                .AddTo(this.disposables);

            documents
                .Connect()
                .AddTo(this.disposables);
        }

        public ExerciseProgramsViewModelStatus Status
        {
            get { return this.status.Value; }
        }

        public ExerciseProgramViewModel SelectedProgram
        {
            get { return this.selectedProgram; }
            set { this.RaiseAndSetIfChanged(ref this.selectedProgram, value); }
        }

        public string ParseErrorMessage
        {
            get { return this.parseErrorMessage.Value; }
        }

        public IReadOnlyReactiveList<ExerciseProgramViewModel> Programs
        {
            get { return this.programs.Value; }
        }

        private ExercisePrograms Model
        {
            get { return this.model.Value; }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.disposables.Dispose();
            }
        }

        private enum DocumentSource
        {
            Cache,
            Cloud
        }

        private struct DocumentSourceWith<T>
        {
            private readonly DocumentSource source;
            private readonly T item;

            public DocumentSourceWith(DocumentSource source, T item)
            {
                this.source = source;
                this.item = item;
            }

            public DocumentSource Source
            {
                get { return this.source; }
            }

            public T Item
            {
                get { return this.item; }
            }

            public override bool Equals(object obj)
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }
    }
}