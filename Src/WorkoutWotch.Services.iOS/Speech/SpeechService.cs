namespace WorkoutWotch.Services.iOS.Speech
{
    using System.Threading;
    using System.Threading.Tasks;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using MonoTouch.AVFoundation;
    using WorkoutWotch.Services.Contracts.Speech;

    public sealed class SpeechService : ISpeechService
    {
        private static readonly AVSpeechSynthesisVoice voice = AVSpeechSynthesisVoice.FromLanguage("en-AU");

        public Task SpeakAsync(string speechString, CancellationToken cancellationToken = default(CancellationToken))
        {
            speechString.AssertNotNull("speechString");

            var tcs = new TaskCompletionSource<bool>();
            var utterance = new AVSpeechUtterance(speechString)
            {
                Voice = voice,
                Rate = 0.3f
            };
            var synthesizer = new AVSpeechSynthesizer();

            synthesizer.DidFinishSpeechUtterance +=
                (sender, e) =>
                {
                    utterance.Dispose();
                    synthesizer.Dispose();
                    tcs.TrySetResult(true);
                };

            if (cancellationToken.CanBeCanceled)
            {
                synthesizer.DidCancelSpeechUtterance +=
                    (sender, e) =>
                    {
                        utterance.Dispose();
                        synthesizer.Dispose();
                        tcs.TrySetCanceled();
                    };

                cancellationToken.Register(() => synthesizer.StopSpeaking(AVSpeechBoundary.Immediate));
            }

            synthesizer.SpeakUtterance(utterance);

            return tcs.Task;
        }
    }
}