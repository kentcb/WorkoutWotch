namespace WorkoutWotch.ViewModels
{
    using System;
    using System.Reactive;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using Kent.Boogaart.HelperTrinity.Extensions;
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

    public sealed class ExerciseProgramsViewModel : DisposableReactiveObject
    {
        private const string exerciseProgramsCacheKey = "ExerciseProgramsDocument";

        private readonly IExerciseDocumentService exerciseDocumentService;
        private readonly IStateService stateService;
        private readonly CompositeDisposable disposables;
        private readonly ObservableAsPropertyHelper<IReadOnlyReactiveList<ExerciseProgramViewModel>> programs;
        private readonly ObservableAsPropertyHelper<string> parseErrorMessage;
        private readonly ObservableAsPropertyHelper<ExerciseProgramsViewModelStatus> status;
        private readonly ObservableAsPropertyHelper<ExercisePrograms> model;
        private ExerciseProgramViewModel selectedProgram;

        public ExerciseProgramsViewModel(
            IAudioService audioService,
            IDelayService delayService,
            IExerciseDocumentService exerciseDocumentService,
            ILoggerService loggerService,
            ISchedulerService schedulerService,
            ISpeechService speechService,
            IStateService stateService)
        {
            audioService.AssertNotNull(nameof(audioService));
            delayService.AssertNotNull(nameof(delayService));
            exerciseDocumentService.AssertNotNull(nameof(exerciseDocumentService));
            loggerService.AssertNotNull(nameof(loggerService));
            schedulerService.AssertNotNull(nameof(schedulerService));
            speechService.AssertNotNull(nameof(speechService));
            stateService.AssertNotNull(nameof(stateService));

            this.exerciseDocumentService = exerciseDocumentService;
            this.stateService = stateService;
            this.disposables = new CompositeDisposable();

            var documentsFromCache = this.stateService
                .GetAsync<string>(exerciseProgramsCacheKey)
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
                .Catch((Exception ex) => Observable.Empty<DocumentSourceWith<string>>());

            var results = documents
                .Select(x => new DocumentSourceWith<IResult<ExercisePrograms>>(x.Source, ExercisePrograms.TryParse(x.Item, audioService, delayService, loggerService, speechService)));

            var safeResults = results
                .Catch(Observable.Empty<DocumentSourceWith<IResult<ExercisePrograms>>>());

            this.parseErrorMessage = safeResults
                .Select(x => x.Item.WasSuccessful ? null : x.Item.ToString())
                .ObserveOn(schedulerService.MainScheduler)
                .ToProperty(this, x => x.ParseErrorMessage)
                .AddTo(this.disposables);

// HACK: TODO: remove this
safeResults
    .Subscribe()
    .AddTo(disposables);

            this.status = results
                .Select(x => !x.Item.WasSuccessful ? ExerciseProgramsViewModelStatus.ParseFailed : x.Source == DocumentSource.Cache ? ExerciseProgramsViewModelStatus.LoadedFromCache : ExerciseProgramsViewModelStatus.LoadedFromCloud)
                .Catch(Observable.Return(ExerciseProgramsViewModelStatus.LoadFailed))
                .ObserveOn(schedulerService.MainScheduler)
                .ToProperty(this, x => x.Status)
                .AddTo(this.disposables);

            this.model = safeResults
                .Select(x => x.Item.WasSuccessful ? x.Item.Value : null)
                .ObserveOn(schedulerService.MainScheduler)
                .ToProperty(this, x => x.Model)
                .AddTo(this.disposables);

            this.programs = this.WhenAnyValue(x => x.Model)
                .Select(x => x == null ? null : x.Programs.CreateDerivedCollection(y => new ExerciseProgramViewModel(loggerService, schedulerService, y)))
                .ObserveOn(schedulerService.MainScheduler)
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

        public ExerciseProgramsViewModelStatus Status => this.status.Value;

        public ExerciseProgramViewModel SelectedProgram
        {
            get { return this.selectedProgram; }
            set { this.RaiseAndSetIfChanged(ref this.selectedProgram, value); }
        }

        public string ParseErrorMessage => this.parseErrorMessage.Value;

        public IReadOnlyReactiveList<ExerciseProgramViewModel> Programs => this.programs.Value;

        private ExercisePrograms Model => this.model.Value;

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