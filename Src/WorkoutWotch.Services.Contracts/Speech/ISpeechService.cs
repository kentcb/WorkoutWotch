using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkoutWotch.Services.Contracts.Speech
{
    public interface ISpeechService
    {
        Task SpeakAsync(string speechString, CancellationToken cancellationToken = default(CancellationToken));
    }
}

