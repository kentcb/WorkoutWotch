namespace WorkoutWotch.Services.Android.Speech
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Subjects;
    using System.Threading;
    using global::Android.App;
    using global::Android.OS;
    using global::Android.Speech.Tts;
    using WorkoutWotch.Services.Contracts.Speech;

    public sealed class SpeechService : UtteranceProgressListener, ISpeechService, TextToSpeech.IOnInitListener
    {
        private readonly TextToSpeech textToSpeech;
        private readonly ConcurrentDictionary<int, AsyncSubject<Unit>> inFlightSpeech;
        private int nextId;

        public SpeechService()
        {
            this.textToSpeech = new TextToSpeech(Application.Context, this);
            this.textToSpeech.SetOnUtteranceProgressListener(this);

            this.inFlightSpeech = new ConcurrentDictionary<int, AsyncSubject<Unit>>();
        }

        public IObservable<Unit> Speak(string speechString)
        {
            var id = Interlocked.Increment(ref this.nextId);
            var result = new AsyncSubject<Unit>();
            inFlightSpeech.TryAdd(id, result);

            this.textToSpeech.Speak(speechString, QueueMode.Add, Bundle.Empty, id.ToString());
            return result;
        }

        void TextToSpeech.IOnInitListener.OnInit(OperationResult status)
        {
            var bestVoice = this
                .textToSpeech
                .Voices
                .Where(voice => voice.Locale.Language == "en" && voice.Locale.Country == "GB")
                .OrderByDescending(voice => voice.Quality)
                .ThenByDescending(voice => voice.IsNetworkConnectionRequired)
                .First();
            this.textToSpeech.SetVoice(bestVoice);
        }

        public override void OnStart(string utteranceId)
        {
        }

        public override void OnDone(string utteranceId)
        {
            var result = GetAndRemoveInFlightSpeech(utteranceId);
            result.OnNext(Unit.Default);
            result.OnCompleted();
        }

        public override void OnError(string utteranceId)
        {
            var result = GetAndRemoveInFlightSpeech(utteranceId);
            result.OnError(new InvalidOperationException("Speech playback failed."));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.textToSpeech.Dispose();
            }
        }

        private AsyncSubject<Unit> GetAndRemoveInFlightSpeech(string utteranceId)
        {
            var id = int.Parse(utteranceId);
            AsyncSubject<Unit> result = null;
            this.inFlightSpeech.TryRemove(id, out result);

            return result;
        }
    }
}