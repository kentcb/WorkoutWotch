namespace WorkoutWotch.ViewModels
{
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading.Tasks;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using ReactiveUI;
    using WorkoutWotch.Models;
    using WorkoutWotch.Services.Contracts.Container;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;
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
        private ExerciseProgramsViewModelStatus status;
        private string parseErrorMessage;
        private ExercisePrograms model;
        private ExerciseProgramViewModel selectedProgram;

        public ExerciseProgramsViewModel(
            IContainerService containerService,
            IExerciseDocumentService exerciseDocumentService,
            ISchedulerService schedulerService,
            IStateService stateService)
        {
            containerService.AssertNotNull("containerService");
            exerciseDocumentService.AssertNotNull("exerciseDocumentService");
            schedulerService.AssertNotNull("schedulerService");
            stateService.AssertNotNull("stateService");

            this.containerService = containerService;
            this.exerciseDocumentService = exerciseDocumentService;
            this.stateService = stateService;
            this.disposables = new CompositeDisposable();

            this.Documents
                .ObserveOn(schedulerService.SynchronizationContextScheduler)
                .Subscribe(async x => await this.OnDocumentReceivedAsync(x), _ => this.Status = ExerciseProgramsViewModelStatus.LoadFailed)
                .AddTo(this.disposables);

            this.programs = this.WhenAnyValue(x => x.Model)
                .Select(x => x == null ? null : x.Programs.CreateDerivedCollection(y => new ExerciseProgramViewModel()))
                .ToProperty(this, x => x.Programs)
                .AddTo(this.disposables);
        }

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
            get { return this.programs.Value; }
        }

        private ExercisePrograms Model
        {
            get { return this.model; }
            set { this.RaiseAndSetIfChanged(ref this.model, value); }
        }

        private IObservable<Tuple<DocumentSource, string>> DocumentsFromCache
        {
            get
            {
                return this.stateService
                    .GetAsync<string>(exerciseProgramsCacheKey)
                    .ToObservable()
                    .Where(x => x != null)
                    .Select(x => Tuple.Create(DocumentSource.Cache, x));
            }
        }

        private IObservable<Tuple<DocumentSource, string>> DocumentsFromCloud
        {
            get
            {
                return this.exerciseDocumentService
                    .ExerciseDocument
                    .Select(x => Tuple.Create(DocumentSource.Cloud, x));
            }
        }

        private IObservable<Tuple<DocumentSource, string>> Documents
        {
            get
            {
                return this.DocumentsFromCache
                    .Catch(Observable.Empty<Tuple<DocumentSource, string>>())
                    .Concat(this.DocumentsFromCloud);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.disposables.Dispose();
            }
        }

        private async Task OnDocumentReceivedAsync(Tuple<DocumentSource, string> document)
        {
            var result = ExercisePrograms.TryParse(document.Item2, this.containerService);

            if (!result.WasSuccessful)
            {
                this.ParseErrorMessage = result.ToString();
                this.Status = ExerciseProgramsViewModelStatus.ParseFailed;
                return;
            }

            this.ParseErrorMessage = null;
            this.Model = result.Value;
            this.Status = document.Item1 == DocumentSource.Cache ? ExerciseProgramsViewModelStatus.LoadedFromCache : ExerciseProgramsViewModelStatus.LoadedFromCloud;

            if (document.Item1 == DocumentSource.Cloud)
            {
                await this.stateService.SetAsync(exerciseProgramsCacheKey, document.Item2);
            }
        }

        private enum DocumentSource
        {
            Cache,
            Cloud
        }
    }
}