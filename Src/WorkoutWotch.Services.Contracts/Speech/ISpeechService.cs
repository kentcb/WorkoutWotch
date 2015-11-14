namespace WorkoutWotch.Services.Contracts.Speech
{
    using System;
    using System.Reactive;
    using System.Threading;

    public interface ISpeechService
    {
        IObservable<Unit> SpeakAsync(string speechString, CancellationToken cancellationToken = default(CancellationToken));
    }
}