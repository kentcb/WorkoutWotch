namespace WorkoutWotch.Services.Contracts.Audio
{
    using System.Threading.Tasks;

    public interface IAudioService
    {
        Task PlayAsync(string resourceUri);
    }
}