namespace WorkoutWotch.ViewModels
{
    using System;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using Genesis.Ensure;
    using Genesis.Logging;
    using ReactiveUI;
    using Services.Contracts.Audio;
    using Services.Contracts.Delay;
    using Services.Contracts.Speech;
    using Sprache;
    using WorkoutWotch.Models;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.State;

    public sealed class ExerciseProgramsViewModel : ReactiveObject, IRoutableViewModel, ISupportsActivation
    {
        private const string exerciseProgramsCacheKey = "ExerciseProgramsDocument";

        private readonly ViewModelActivator activator;
        private readonly IExerciseDocumentService exerciseDocumentService;
        private readonly IStateService stateService;
        private readonly ILogger logger;
        private readonly IScreen hostScreen;
        private readonly ObservableAsPropertyHelper<ExerciseProgramsViewModelStatus> status;
        private readonly ObservableAsPropertyHelper<IReadOnlyReactiveList<ExerciseProgramViewModel>> programs;
        private readonly ObservableAsPropertyHelper<ExercisePrograms> model;
        private readonly ObservableAsPropertyHelper<string> parseErrorMessage;
        private ExerciseProgramViewModel selectedProgram;

        public ExerciseProgramsViewModel(
            IAudioService audioService,
            IDelayService delayService,
            IExerciseDocumentService exerciseDocumentService,
            IScheduler mainScheduler,
            IScheduler backgroundScheduler,
            ISpeechService speechService,
            IStateService stateService,
            IScreen hostScreen,
            ExerciseProgramViewModelFactory exerciseProgramViewModelFactory)
        {
            Ensure.ArgumentNotNull(audioService, nameof(audioService));
            Ensure.ArgumentNotNull(delayService, nameof(delayService));
            Ensure.ArgumentNotNull(exerciseDocumentService, nameof(exerciseDocumentService));
            Ensure.ArgumentNotNull(mainScheduler, nameof(mainScheduler));
            Ensure.ArgumentNotNull(backgroundScheduler, nameof(backgroundScheduler));
            Ensure.ArgumentNotNull(speechService, nameof(speechService));
            Ensure.ArgumentNotNull(stateService, nameof(stateService));
            Ensure.ArgumentNotNull(hostScreen, nameof(hostScreen));
            Ensure.ArgumentNotNull(exerciseProgramViewModelFactory, nameof(exerciseProgramViewModelFactory));

            this.logger = LoggerService.GetLogger(this.GetType());

            using (this.logger.Perf("Construction"))
            {
                this.activator = new ViewModelActivator();
                this.exerciseDocumentService = exerciseDocumentService;
                this.stateService = stateService;
                this.hostScreen = hostScreen;

                this
                    .WhenAnyValue(x => x.SelectedProgram)
                    .Where(x => x != null)
                    .SelectMany(x => this.hostScreen.Router.Navigate.Execute(x))
                    .SubscribeSafe();

                var isActivated = this
                    .GetIsActivated()
                    .Publish()
                    .RefCount();

                var documentsFromCache = this
                    .stateService
                    .Get<string>(exerciseProgramsCacheKey)
                    .Where(x => x != null)
                    .Select(x => new DocumentSourceWith<string>(DocumentSource.Cache, x));

                var documentsFromService = this
                    .exerciseDocumentService
                    .ExerciseDocument
                    .Where(x => x != null)
                    .Select(x => new DocumentSourceWith<string>(DocumentSource.Service, x));

                var documents = isActivated
                    .Select(
                        activated =>
                        {
                            if (activated)
                            {
                                return documentsFromCache
                                    .Catch((Exception ex) => Observable<DocumentSourceWith<string>>.Empty)
                                    .Concat(documentsFromService)
                                    .Do(x => this.logger.Debug("Received document from {0}.", x.Source));
                            }
                            else
                            {
                                return Observable<DocumentSourceWith<string>>.Empty;
                            }
                        })
                    .Switch()
                    .Publish();

                var results = documents
                    .ObserveOn(backgroundScheduler)
                    .Select(
                        x =>
                        {
                            IResult<ExercisePrograms> parsedExercisePrograms;

                            using (this.logger.Perf("Parsing exercise programs from {0}.", x.Source))
                            {
                                parsedExercisePrograms = ExercisePrograms.TryParse(x.Item, audioService, delayService, speechService);
                            }

                            return new DocumentSourceWith<IResult<ExercisePrograms>>(x.Source, parsedExercisePrograms);
                        })
                    .Publish();

                var safeResults = results
                    .Catch((Exception ex) => Observable<DocumentSourceWith<IResult<ExercisePrograms>>>.Empty);

                this.parseErrorMessage = safeResults
                    .Select(x => x.Item.WasSuccessful ? null : x.Item.ToString())
                    .ToProperty(this, x => x.ParseErrorMessage, scheduler: mainScheduler);

                this.status = results
                    .Select(x => !x.Item.WasSuccessful ? ExerciseProgramsViewModelStatus.ParseFailed : x.Source == DocumentSource.Cache ? ExerciseProgramsViewModelStatus.LoadedFromCache : ExerciseProgramsViewModelStatus.LoadedFromService)
                    .Catch((Exception ex) => Observable.Return(ExerciseProgramsViewModelStatus.LoadFailed))
                    .ObserveOn(mainScheduler)
                    .ToProperty(this, x => x.Status);

                this.model = safeResults
                    .Select(x => x.Item.WasSuccessful ? x.Item.Value : null)
                    .ToProperty(this, x => x.Model, scheduler: mainScheduler);

                this.programs = this
                    .WhenAnyValue(x => x.Model)
                    .Select(x => x == null ? null : x.Programs.CreateDerivedCollection(y => exerciseProgramViewModelFactory(y)))
                    .ObserveOn(mainScheduler)
                    .ToProperty(this, x => x.Programs);

                var safeDocuments = documents
                    .Catch((Exception ex) => Observable<DocumentSourceWith<string>>.Empty);

                safeDocuments
                    .Where(x => x.Source == DocumentSource.Service)
                    .SelectMany(x => this.stateService.Set(exerciseProgramsCacheKey, x.Item))
                    .SubscribeSafe();

                results
                    .Connect();

                documents
                    .Connect();

                this
                    .WhenActivated(
                        disposables =>
                        {
                            using (this.logger.Perf("Activation"))
                            {
                                this
                                    .hostScreen
                                    .Router
                                    .CurrentViewModel
                                    .OfType<ExerciseProgramsViewModel>()
                                    .SubscribeSafe(x => x.SelectedProgram = null)
                                    .AddTo(disposables);
                            }
                        });
            }
        }

        public ViewModelActivator Activator => this.activator;

        public string UrlPathSegment => "Exercise Programs";

        public IScreen HostScreen => hostScreen;

        public ExerciseProgramsViewModelStatus Status => this.status.Value;

        public ExerciseProgramViewModel SelectedProgram
        {
            get { return this.selectedProgram; }
            set { this.RaiseAndSetIfChanged(ref this.selectedProgram, value); }
        }

        public string ParseErrorMessage => this.parseErrorMessage.Value;

        public IReadOnlyReactiveList<ExerciseProgramViewModel> Programs => this.programs.Value;

        private ExercisePrograms Model => this.model.Value;

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