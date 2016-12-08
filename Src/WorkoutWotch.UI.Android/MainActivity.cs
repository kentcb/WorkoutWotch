namespace WorkoutWotch.UI.Android
{
    using System;
    using System.Diagnostics;
    using System.Reactive;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using global::Android.App;
    using global::Android.Content;
    using global::Android.Content.PM;
    using global::Android.Gms.Common;
    using global::Android.OS;
    using Services.Android.ExerciseDocument;
    using Splat;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.UI;

    [Activity(
        Label = "Workout Wotch",
        Icon = "@drawable/icon",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public sealed class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity, IConnectionResultHandler
    {
        private const int RequestCodeResolution = 1;
        private static App app;
        private readonly SerialDisposable<Subject<bool>> resultResolutionSubject;

        public MainActivity()
        {
            this.resultResolutionSubject = new SerialDisposable<Subject<bool>>();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            if (app == null)
            {
                var compositionRoot = new AndroidCompositionRoot(this);
                DirectLoggingOutputToConsole(compositionRoot.ResolveLoggerService());
                new AndroidSplatRegistrar().Register(Locator.CurrentMutable, compositionRoot);
                app = compositionRoot.ResolveApp();
                app.Initialize();
            }

            LoadApplication(app);
        }

        [Conditional("DEBUG")]
        private static void DirectLoggingOutputToConsole(ILoggerService loggerService) =>
            loggerService
                .Entries
                .Subscribe(entry => Console.WriteLine("[{0}] #{1} {2} : {3}", entry.Level, entry.ThreadId, entry.Name, entry.Message));

        IObservable<bool> IConnectionResultHandler.HandleConnectionResult(ConnectionResult connectionResult)
        {
            if (!connectionResult.HasResolution)
            {
                GoogleApiAvailability
                    .Instance
                    .GetErrorDialog(this, connectionResult.ErrorCode, 0)
                    .Show();
                return Observable.Return(false);
            }

            try
            {
                this.resultResolutionSubject.Disposable = new Subject<bool>();
                connectionResult.StartResolutionForResult(this, RequestCodeResolution);
                return this.resultResolutionSubject.Disposable;
            }
            catch (IntentSender.SendIntentException)
            {
                return Observable.Return(false);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == RequestCodeResolution)
            {
                this.resultResolutionSubject.Disposable.OnNext(resultCode == Result.Ok);
            }
        }
    }
}