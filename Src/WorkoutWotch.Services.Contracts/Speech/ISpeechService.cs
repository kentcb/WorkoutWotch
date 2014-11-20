namespace WorkoutWotch.Services.Contracts.Speech
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface ISpeechService
    {
        Task SpeakAsync(string speechString, CancellationToken cancellationToken = default(CancellationToken));
    }
}