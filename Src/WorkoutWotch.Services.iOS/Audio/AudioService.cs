namespace WorkoutWotch.Services.iOS.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using AVFoundation;
    using Foundation;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using WorkoutWotch.Services.Contracts.Audio;

    public sealed class AudioService : IAudioService
    {
        private readonly IDictionary<AVAudioPlayer, TaskCompletionSource<bool>> activeAudioPlayers;

        public AudioService()
        {
            this.activeAudioPlayers = new Dictionary<AVAudioPlayer, TaskCompletionSource<bool>>();
        }

        public Task PlayAsync(string resourceUri)
        {
            resourceUri.AssertNotNull("resourceUri");

            var tcs = new TaskCompletionSource<bool>();
            var url = new NSUrl(resourceUri);
            var audioPlayer = AVAudioPlayer.FromUrl(url);
            this.activeAudioPlayers[audioPlayer] = tcs;

            audioPlayer.FinishedPlaying += this.OnAudioPlayerFinishedPlaying;
            audioPlayer.Play();

            return tcs.Task;
        }

        private void OnAudioPlayerFinishedPlaying(object sender, EventArgs e)
        {
            var audioPlayer = (AVAudioPlayer)sender;
            Debug.Assert(this.activeAudioPlayers.ContainsKey(audioPlayer));
            var tcs = this.activeAudioPlayers[audioPlayer];

            this.activeAudioPlayers.Remove(audioPlayer);
            tcs.TrySetResult(true);

            SynchronizationContext.Current.Post(_ => audioPlayer.Dispose(), state: null);
        }
    }
}