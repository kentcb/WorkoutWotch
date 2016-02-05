namespace WorkoutWotch.ViewModels
{
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using ReactiveUI;
    using Services.Contracts.Audio;
    using Services.Contracts.Delay;
    using Services.Contracts.Speech;
    using Sprache;
    using WorkoutWotch.Models;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Contracts.Scheduler;
    using WorkoutWotch.Services.Contracts.State;
    using WorkoutWotch.Utility;

    public sealed class ExerciseProgramsViewModel : DisposableReactiveObject, IRoutableViewModel
    {
        private const string exerciseProgramsCacheKey = "ExerciseProgramsDocument";

        private readonly IExerciseDocumentService exerciseDocumentService;
        private readonly IStateService stateService;
        private readonly ILogger logger;
        private readonly IScreen hostScreen;
        private readonly CompositeDisposable disposables;
        private IReadOnlyReactiveList<ExerciseProgramViewModel> programs;
        private ExerciseProgramsViewModelStatus status;
        private ExercisePrograms model;
        private string parseErrorMessage;
        private ExerciseProgramViewModel selectedProgram;

        public ExerciseProgramsViewModel(
            IAudioService audioService,
            IDelayService delayService,
            IExerciseDocumentService exerciseDocumentService,
            ILoggerService loggerService,
            ISchedulerService schedulerService,
            ISpeechService speechService,
            IStateService stateService,
            IScreen hostScreen,
            ExerciseProgramViewModelFactory exerciseProgramViewModelFactory)
        {
            Ensure.ArgumentNotNull(audioService, nameof(audioService));
            Ensure.ArgumentNotNull(delayService, nameof(delayService));
            Ensure.ArgumentNotNull(exerciseDocumentService, nameof(exerciseDocumentService));
            Ensure.ArgumentNotNull(loggerService, nameof(loggerService));
            Ensure.ArgumentNotNull(schedulerService, nameof(schedulerService));
            Ensure.ArgumentNotNull(speechService, nameof(speechService));
            Ensure.ArgumentNotNull(stateService, nameof(stateService));
            Ensure.ArgumentNotNull(hostScreen, nameof(hostScreen));
            Ensure.ArgumentNotNull(exerciseProgramViewModelFactory, nameof(exerciseProgramViewModelFactory));

            this.exerciseDocumentService = exerciseDocumentService;
            this.stateService = stateService;
            this.logger = loggerService.GetLogger(this.GetType());
            this.hostScreen = hostScreen;
            this.disposables = new CompositeDisposable();

            var documentsFromCache = this
                .stateService
                .GetAsync<string>(exerciseProgramsCacheKey)
                .Where(x => x != null)
                .Select(x => new DocumentSourceWith<string>(DocumentSource.Cache, x));

            var documentsFromService = this
                .exerciseDocumentService
                .ExerciseDocument
                .Where(x => x != null)
                .Select(x => new DocumentSourceWith<string>(DocumentSource.Service, x));

            var documents = documentsFromCache
                .Catch((Exception ex) => Observable.Empty<DocumentSourceWith<string>>())
                .Concat(documentsFromService)
                .Do(x => this.logger.Debug("Received document from {0}.", x.Source))
                .Publish();

            var safeDocuments = documents
                .Catch((Exception ex) => Observable.Empty<DocumentSourceWith<string>>());

            var results = documents
                .ObserveOn(schedulerService.TaskPoolScheduler)
                .Select(
                    x =>
                    {
                        IResult<ExercisePrograms> parsedExercisePrograms;

                        using (this.logger.Perf("Parsing exercise programs from {0}.", x.Source))
                        {
                            parsedExercisePrograms = ExercisePrograms.TryParse(x.Item, audioService, delayService, loggerService, speechService);
                        }

                        return new DocumentSourceWith<IResult<ExercisePrograms>>(x.Source, parsedExercisePrograms);
                    })
                .Publish();

            var safeResults = results
                .Catch((Exception ex) => Observable.Empty<DocumentSourceWith<IResult<ExercisePrograms>>>());

            safeResults
                .Select(x => x.Item.WasSuccessful ? null : x.Item.ToString())
                .ObserveOn(schedulerService.MainScheduler)
                .Subscribe(x => this.ParseErrorMessage = x)
                .AddTo(this.disposables);

            results
                .Select(x => !x.Item.WasSuccessful ? ExerciseProgramsViewModelStatus.ParseFailed : x.Source == DocumentSource.Cache ? ExerciseProgramsViewModelStatus.LoadedFromCache : ExerciseProgramsViewModelStatus.LoadedFromService)
                .Catch((Exception ex) => Observable.Return(ExerciseProgramsViewModelStatus.LoadFailed))
                .ObserveOn(schedulerService.MainScheduler)
                .Subscribe(x => this.Status = x)
                .AddTo(this.disposables);

            safeResults
                .Select(x => x.Item.WasSuccessful ? x.Item.Value : null)
                .ObserveOn(schedulerService.MainScheduler)
                .Subscribe(x => this.Model = x)
                .AddTo(this.disposables);

            this.WhenAnyValue(x => x.Model)
                .Select(x => x == null ? null : x.Programs.CreateDerivedCollection(y => exerciseProgramViewModelFactory(y)))
                .ObserveOn(schedulerService.MainScheduler)
                .Subscribe(x => this.Programs = x)
                .AddTo(this.disposables);

            safeDocuments
                .Where(x => x.Source == DocumentSource.Service)
                .SelectMany(x => this.stateService.SetAsync(exerciseProgramsCacheKey, x.Item))
                .Subscribe()
                .AddTo(this.disposables);

            results
                .Connect()
                .AddTo(this.disposables);

            documents
                .Connect()
                .AddTo(this.disposables);

            this
                .WhenAnyValue(x => x.SelectedProgram)
                .Where(x => x != null)
                .Subscribe(x => this.hostScreen.Router.Navigate.Execute(x))
                .AddTo(this.disposables);

            this
                .hostScreen
                .Router
                .CurrentViewModel
                .OfType<ExerciseProgramsViewModel>()
                .Subscribe(x => x.SelectedProgram = null)
                .AddTo(this.disposables);
        }

        public string UrlPathSegment => "Exercise Programs";

        public IScreen HostScreen => hostScreen;

        public ExerciseProgramsViewModelStatus Status
        {
            get { return this.status; }
            private set { this.RaiseAndSetIfChanged(ref this.status, value); }
        }

        public ExerciseProgramViewModel SelectedProgram
        {
            get { return this.selectedProgram; }
            set { this.RaiseAndSetIfChanged(ref this.selectedProgram, value); }
        }

        public string ParseErrorMessage
        {
            get { return this.parseErrorMessage; }
            private set { this.RaiseAndSetIfChanged(ref this.parseErrorMessage, value); }
        }

        public IReadOnlyReactiveList<ExerciseProgramViewModel> Programs
        {
            get { return this.programs; }
            private set { this.RaiseAndSetIfChanged(ref this.programs, value); }
        }

        private ExercisePrograms Model
        {
            get { return this.model; }
            set { this.RaiseAndSetIfChanged(ref this.model, value); }
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
            Service
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

            public DocumentSource Source => this.source;

            public T Item => this.item;

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