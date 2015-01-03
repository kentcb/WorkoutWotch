namespace WorkoutWotch.Services.iOS.ExerciseDocument
{
    using System;
    using System.IO;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using MonoTouch.Foundation;
    using MonoTouch.UIKit;
    using WorkoutWotch.Services.Contracts.ExerciseDocument;
    using WorkoutWotch.Services.Contracts.Logger;

    public sealed class iCloudExerciseDocumentService : NSObject, IExerciseDocumentService
    {
        private static readonly string documentFilename = "Workout Wotch Exercise Programs.mkd";

        private readonly ILogger logger;
        private readonly BehaviorSubject<string> exerciseDocument;
        private readonly object sync;
        private int initialized;
        private NSUrl ubiquityContainerUrl;

        public iCloudExerciseDocumentService(ILoggerService loggerService)
        {
            loggerService.AssertNotNull("loggerService");

            this.logger = loggerService.GetLogger(this.GetType());
            this.exerciseDocument = new BehaviorSubject<string>(null);
            this.sync = new object();
        }

        public IObservable<string> ExerciseDocument
        {
            get
            {
                if (Interlocked.CompareExchange(ref this.initialized, 1, 0) == 0)
                {
                    this
                        .InitializeAsync()
                        .ContinueWith(
                            x =>
                            {
                                if (x.IsFaulted)
                                {
                                    this.logger.Error(x.Exception, "Failed to initialize.");
                                }
                                else
                                {
                                    this.logger.Info("Successfully initialized.");
                                }
                            });
                }

                return this.exerciseDocument;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(this, UIDocument.StateChangedNotification);
                this.exerciseDocument.Dispose();
            }
        }

        private async Task InitializeAsync()
        {
            var synchronizationContextTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            try
            {
                await this.InitializeUbiquityContainerUrlAsync().ContinueOnAnyContext();
                await this.InstigateDocumentLookupAsync(synchronizationContextTaskScheduler).ContinueOnAnyContext();

                this.logger.Debug("Successfully initialized.");
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Initialization failed.");
                this.exerciseDocument.OnError(ex);
            }
        }

        private Task InitializeUbiquityContainerUrlAsync()
        {
            return Task.Run(
                () =>
                {
                    this.logger.Debug("Getting URL for ubiquity container.");

                    var localUbiquityContainerUrl = NSFileManager.DefaultManager.GetUrlForUbiquityContainer(containerIdentifier: null);

                    if (localUbiquityContainerUrl == null)
                    {
                        this.logger.Error("Failed to obtain URL for ubiquity container.");
                        throw new NotSupportedException("iCloud not enabled.");
                    }

                    this.logger.Debug("Ubiquity URL obtained: {0}", localUbiquityContainerUrl);

                    lock (this.sync)
                    {
                        this.ubiquityContainerUrl = localUbiquityContainerUrl;
                    }
                });
        }

        private Task InstigateDocumentLookupAsync(TaskScheduler synchronizationContextTaskScheduler)
        {
            return Task.Factory.StartNew(
                () =>
                {
                    var query = new NSMetadataQuery
                    {
                        SearchScopes = new NSObject[]
                        {
                            NSMetadataQuery.QueryUbiquitousDocumentsScope
                        },
                        Predicate = NSPredicate.FromFormat(
                            "%K == %@",
                            new NSObject[]
                            {
                                NSMetadataQuery.ItemFSNameKey,
                                new NSString(documentFilename)
                            })
                    };

                    NSNotificationCenter.DefaultCenter.AddObserver(NSMetadataQuery.DidFinishGatheringNotification, this.OnQueryFinished, query);
                    query.StartQuery();
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                synchronizationContextTaskScheduler);
        }

        private void OnQueryFinished(NSNotification notification)
        {
            var query = (NSMetadataQuery)notification.Object;
            query.DisableUpdates();
            query.StopQuery();
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, NSMetadataQuery.DidFinishGatheringNotification, query);

            this.LoadDocument(query);
        }

        private void OnDocumentUpdated(NSNotification notification)
        {
            var document = (ExerciseCloudDocument)notification.Object;
            this.exerciseDocument.OnNext(document.Data);
        }

        private void LoadDocument(NSMetadataQuery query)
        {
            if (query.ResultCount == 1)
            {
                this.logger.Debug("Query has 1 result.");

                var item = (NSMetadataItem)query.ResultAtIndex(0);
                var url = (NSUrl)item.ValueForAttribute(NSMetadataQuery.ItemURLKey);
                var exerciseCloudDocument = new ExerciseCloudDocument(url);

                exerciseCloudDocument.Open(
                    (success) =>
                    {
                        if (!success)
                        {
                            this.logger.Error("Failed to open document.");
                            this.exerciseDocument.OnError(new InvalidOperationException("Failed to open document."));
                        }
                    });
            }
            else
            {
                var documentsFolder = Path.Combine(this.ubiquityContainerUrl.Path, "Documents");
                var documentPath = Path.Combine(documentsFolder, documentFilename);
                var url = new NSUrl(documentPath, isDir: false);
                var exerciseCloudDocument = new ExerciseCloudDocument(url)
                {
                    Data = GetDefaultExerciseDocument()
                };

                exerciseCloudDocument.Save(
                    exerciseCloudDocument.FileUrl,
                    UIDocumentSaveOperation.ForCreating,
                    success =>
                    {
                        if (success)
                        {
                            this.logger.Info("Successfully created new document.");
                            this.exerciseDocument.OnNext(exerciseCloudDocument.Data);
                        }
                        else
                        {
                            this.logger.Error("Failed to create new document.");
                            this.exerciseDocument.OnError(new InvalidOperationException("Failed to create default document."));
                        }
                    });
            }

            // register for document updates
            NSNotificationCenter.DefaultCenter.AddObserver(UIDocument.StateChangedNotification, this.OnDocumentUpdated);
        }

        private static string GetDefaultExerciseDocument()
        {
            using (var stream = typeof(iCloudExerciseDocumentService).Assembly.GetManifestResourceStream("WorkoutWotch.Services.iOS.ExerciseDocument.DefaultExerciseDocument.mkd"))
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        private sealed class ExerciseCloudDocument : UIDocument
        {
            private NSString data;

            public ExerciseCloudDocument(NSUrl url)
                : base(url)
            {
            }

            public string Data
            {
                get { return this.data.ToString(); }
                set { this.data = new NSString(value); }
            }

            public override bool LoadFromContents(NSObject contents, string typeName, out NSError outError)
            {
                outError = null;

                if (contents != null)
                {
                    this.data = NSString.FromData((NSData)contents, NSStringEncoding.UTF8);
                }
            
                return true;
            }

            public override NSObject ContentsForType(string typeName, out NSError outError)
            {
                outError = null;
                return this.data.Encode(NSStringEncoding.UTF8);
            }
        }
    }
}