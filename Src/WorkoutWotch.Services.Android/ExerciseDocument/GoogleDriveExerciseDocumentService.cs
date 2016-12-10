namespace WorkoutWotch.Services.Android.ExerciseDocument
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using Genesis.Ensure;
    using Genesis.Logging;
    using global::Android.App;
    using global::Android.Gms.Common;
    using global::Android.Gms.Common.Apis;
    using global::Android.Gms.Drive;
    using global::Android.Gms.Drive.Events;
    using global::Android.Gms.Drive.Query;
    using Services.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;

    // TODO/WARNING: an unholy marriage of Rx and TPL follows
    public sealed class GoogleDriveExerciseDocumentService :
        Java.Lang.Object,
        IExerciseDocumentService,
        GoogleApiClient.IConnectionCallbacks,
        GoogleApiClient.IOnConnectionFailedListener,
        IChangeListener
    {
        private static readonly string documentFilename = "Workout Wotch Exercise Programs.mkd";
        private readonly ILogger logger;
        private readonly IConnectionResultHandler connectionResultHandler;
        private readonly BehaviorSubject<string> exerciseDocument;
        private readonly SerialDisposable connectedDisposable;
        private readonly object sync;
        private GoogleApiClient client;
        private int initialized;

        public GoogleDriveExerciseDocumentService(
            IConnectionResultHandler connectionResultHandler)
        {
            Ensure.ArgumentNotNull(connectionResultHandler, nameof(connectionResultHandler));

            this.logger = LoggerService.GetLogger(this.GetType());
            this.connectionResultHandler = connectionResultHandler;
            this.exerciseDocument = new BehaviorSubject<string>(null);
            this.sync = new object();
            this.connectedDisposable = new SerialDisposable();
        }

        public IObservable<string> ExerciseDocument
        {
            get
            {
                if (Interlocked.CompareExchange(ref this.initialized, 1, 0) == 0)
                {
                    this.Initialize();
                }

                return this.exerciseDocument;
            }
        }

        private void Initialize()
        {
            this.client = new GoogleApiClient.Builder(Application.Context)
                .AddApi(DriveClass.API)
                .AddScope(DriveClass.ScopeFile)
                .AddConnectionCallbacks(this)
                .AddOnConnectionFailedListener(this)
                .Build();

            client.Connect();
        }

        void GoogleApiClient.IOnConnectionFailedListener.OnConnectionFailed(ConnectionResult result)
        {
            this
                .connectionResultHandler
                .HandleConnectionResult(result)
                .Subscribe(
                    handled =>
                    {
                        if (handled)
                        {
                            this.client.Connect();
                        }
                    },
                    ex => this.logger.Error(ex, "Handling connection result failed."));
        }

        private async Task TickFileContents(IDriveFile file)
        {
            var contents = await this.ReadFileAsync(file);
            this.exerciseDocument.OnNext(contents);
        }

        private async Task<IDriveFile> GetFileAsync()
        {
            var rootFolder = DriveClass
                .DriveApi
                .GetRootFolder(this.client);
            var query = new QueryClass.Builder()
                .AddFilter(Filters.Eq(SearchableField.Title, documentFilename))
                .Build();

            var result = await rootFolder.QueryChildrenAsync(this.client, query);

            if (!result.Status.IsSuccess || result.MetadataBuffer.Count == 0)
            {
                this.logger.Warn("Failed to find existing exercise document.");
                return null;
            }

            var fileMetadata = result.MetadataBuffer.First();
            var file = fileMetadata
                .DriveId
                .AsDriveFile();

            return file;
        }

        private async Task<bool> CreateFileAsync()
        {
            this.logger.Debug("Creating file.");

            var result = await DriveClass
                .DriveApi
                .NewDriveContentsAsync(this.client);

            if (!result.Status.IsSuccess)
            {
                this.logger.Error("Failed to create drive contents. Status code: {0}. Status message: {1}.", result.Status.StatusCode, result.Status.StatusMessage);
                return false;
            }

            using (var streamWriter = new StreamWriter(result.DriveContents.OutputStream))
            {
                await streamWriter.WriteAsync(GetDefaultExerciseDocument());
            }

            var changeSet = new MetadataChangeSet.Builder()
                .SetTitle(documentFilename)
                .SetMimeType("text/plain")
                .Build();
            var createResult = await DriveClass
                .DriveApi
                .GetRootFolder(this.client)
                .CreateFile(this.client, changeSet, result.DriveContents);

            if (!createResult.Status.IsSuccess)
            {
                this.logger.Error("Failed to create file. Status code: {0}. Status message: {1}.", result.Status.StatusCode, result.Status.StatusMessage);
                return false;
            }

            return true;
        }

        private async Task<string> ReadFileAsync(IDriveFile file)
        {
            var result = await file.OpenAsync(this.client, DriveFile.ModeReadOnly, null);

            if (!result.Status.IsSuccess)
            {
                this.logger.Error("Failed to read file.");
                return null;
            }

            using (var streamReader = new StreamReader(result.DriveContents.InputStream))
            {
                return await streamReader.ReadToEndAsync();
            }
        }

        private static string GetDefaultExerciseDocument()
        {
            using (var stream = typeof(CannedExerciseDocumentService).Assembly.GetManifestResourceStream("WorkoutWotch.Services.ExerciseDocument.DefaultExerciseDocument.mkd"))
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        void GoogleApiClient.IConnectionCallbacks.OnConnected(global::Android.OS.Bundle connectionHint)
        {
            this.logger.Debug("Client connected.");

            var disposables = new CompositeDisposable();

            Observable
                .StartAsync(
                    async () =>
                    {
                        var file = await this.GetFileAsync();

                        if (file == null)
                        {
                            this.logger.Info("File not found - attempting to create it.");

                            var created = await this.CreateFileAsync();

                            if (!created)
                            {
                                this.logger.Error("File could not be created.");
                                return;
                            }

                            file = await this.GetFileAsync();

                            if (file == null)
                            {
                                this.logger.Error("File not found even after creating it.");
                                return;
                            }
                        }

                        var listener = await file.AddChangeListenerAsync(this.client, this);

                        if (!listener.Status.IsSuccess)
                        {
                            this.logger.Error("Change listener could not be added.");
                        }

                        await this.TickFileContents(file);
                    })
                .Subscribe(
                    _ => { },
                    ex => this.logger.Error(ex, "Failed getting file."))
                .AddTo(disposables);

            Observable
                .Timer(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), TaskPoolScheduler.Default)
                .SelectMany(
                    _ =>
                        DriveClass
                            .DriveApi
                            .RequestSyncAsync(this.client))
                .Subscribe(
                    _ => { },
                    ex => this.logger.Error(ex, "Request sync failed."))
                .AddTo(disposables);

            this.connectedDisposable.Disposable = disposables;
        }

        void GoogleApiClient.IConnectionCallbacks.OnConnectionSuspended(int cause)
        {
            this.logger.Warn("Connection suspended. Cause: {0}.", cause);
            this.connectedDisposable.Disposable = null;
        }

        public void OnChange(ChangeEvent evt)
        {
            this
                .RefreshFileAsync()
                .ContinueWith(
                    x =>
                    {
                        if (x.IsFaulted)
                        {
                            this.logger.Error(x.Exception, "Failed to refresh file.");
                        }
                        else
                        {
                            this.logger.Info("Successfully refreshed file.");
                        }
                    });
        }

        private async Task RefreshFileAsync()
        {
            var file = await this.GetFileAsync();
            await this.TickFileContents(file);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.exerciseDocument.Dispose();
                this.connectedDisposable.Dispose();
                this.client?.Dispose();
            }
        }
    }
}