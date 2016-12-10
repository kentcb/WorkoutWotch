namespace WorkoutWotch.UI.Android
{
    using System;
    using System.Diagnostics;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using Genesis.Logging;
    using global::Android.App;
    using global::Android.Content;
    using global::Android.Content.PM;
    using global::Android.Gms.Common;
    using global::Android.OS;
    using Services.Android.ExerciseDocument;
    using Splat;

    [Activity(
        Label = "Workout Wotch",
        Icon = "@drawable/icon",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public sealed class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity, IConnectionResultHandler
    {
        private const int RequestCodeResolution = 1;
        private readonly Genesis.Logging.ILogger logger;
        private readonly SerialDisposable<Subject<bool>> resultResolutionSubject;

        public MainActivity()
        {
            this.logger = LoggerService.GetLogger(this.GetType());
            this.resultResolutionSubject = new SerialDisposable<Subject<bool>>();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            var compositionRoot = MainApplication.Instance.CompositionRoot;
            new AndroidSplatRegistrar().Register(Locator.CurrentMutable, compositionRoot);
            var app = compositionRoot.ResolveApp();
            app.Initialize();
            LoadApplication(compositionRoot.ResolveApp());
        }

        IObservable<bool> IConnectionResultHandler.HandleConnectionResult(ConnectionResult connectionResult)
        {
            if (!connectionResult.HasResolution)
            {
                GoogleApiAvailability
                    .Instance
                    .GetErrorDialog(this, connectionResult.ErrorCode, 0)
                    .Show();
                return Observables.False;
            }

            try
            {
                this.resultResolutionSubject.Disposable = new Subject<bool>();
                connectionResult.StartResolutionForResult(this, RequestCodeResolution);
                return this.resultResolutionSubject.Disposable;
            }
            catch (IntentSender.SendIntentException ex)
            {
                this.logger.Error(ex, "Unable to resolve.");
                return Observables.False;
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == RequestCodeResolution)
            {
                if (resultCode == Result.Canceled)
                {
                    this
                        .logger
                        .Error("Attempting to resolve Google Drive authentication returned a canceled result. This indicates that the OAuth2 credential settings (package name/certificate fingerprint) are not in sync with the build. If your package name is correct, it's likely due to your debug keystore being regenerated/replaced by a new Xamarin installation. Visit https://console.cloud.google.com/apis/credentials and follow the instructions to update the fingerprint.");
                    Debugger.Break();
                }
                else
                {
                    this
                        .resultResolutionSubject
                        .Disposable
                        .OnNext(resultCode == Result.Ok);
                }
            }
        }
    }
}