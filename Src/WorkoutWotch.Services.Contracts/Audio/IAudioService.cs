using System;
using System.Threading.Tasks;

namespace WorkoutWotch.Services.Contracts.Audio
{
    public interface IAudioService
    {
        Task PlayAsync(string resourceUri);
    }
}

