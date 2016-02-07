namespace WorkoutWotch.Services.Contracts.Speech
{
    using System;
    using System.Reactive;

    public interface ISpeechService
    {
        IObservable<Unit> Speak(string speechString);
    }
}