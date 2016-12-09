namespace WorkoutWotch.Services.iOS.Speech
{
    using System;
    using System.Reactive;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using AVFoundation;
    using Genesis.Ensure;
    using WorkoutWotch.Services.Contracts.Speech;

    public sealed class SpeechService : ISpeechService
    {
        private static readonly AVSpeechSynthesisVoice voice = AVSpeechSynthesisVoice.FromLanguage("en-AU");

        public IObservable<Unit> Speak(string speechString)
        {
            Ensure.ArgumentNotNull(speechString, nameof(speechString));

            return Observable
                .Create<Unit>(
                    observer =>
                    {
                        var disposables = new CompositeDisposable();
                        var utterance = new AVSpeechUtterance(speechString)
                            {
                                Voice = voice,
                                Rate = 0.55f
                            }
                            .AddTo(disposables);
                        var synthesizer = new AVSpeechSynthesizer()
                            .AddTo(disposables);
                        var finishedUtterance = Observable
                            .FromEventPattern<AVSpeechSynthesizerUteranceEventArgs>(x => synthesizer.DidFinishSpeechUtterance += x, x => synthesizer.DidFinishSpeechUtterance -= x)
                            .ToSignal()
                            .Publish();

                        finishedUtterance
                            .SubscribeSafe(
                                _ =>
                                {
                                    utterance.Dispose();
                                    synthesizer.Dispose();
                                })
                            .AddTo(disposables);

                        finishedUtterance
                            .FirstAsync()
                            .Subscribe(observer)
                            .AddTo(disposables);

                        finishedUtterance
                            .Connect()
                            .AddTo(disposables);

                        synthesizer.SpeakUtterance(utterance);

                        return disposables;
                    });
        }
    }
}