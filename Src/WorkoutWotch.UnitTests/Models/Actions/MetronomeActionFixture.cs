namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Builders;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.UnitTests.Services.Audio.Mocks;
    using WorkoutWotch.UnitTests.Services.Delay.Mocks;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using Xunit;

    public class MetronomeActionFixture
    {
        [Fact]
        public void ctor_throws_if_audio_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new MetronomeAction(null, new DelayServiceMock(), new LoggerServiceMock(), Enumerable.Empty<MetronomeTick>()));
        }

        [Fact]
        public void ctor_throws_if_delay_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new MetronomeAction(new AudioServiceMock(), null, new LoggerServiceMock(), Enumerable.Empty<MetronomeTick>()));
        }

        [Fact]
        public void ctor_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new MetronomeAction(new AudioServiceMock(), new DelayServiceMock(), null, Enumerable.Empty<MetronomeTick>()));
        }

        [Fact]
        public void ctor_throws_if_ticks_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new MetronomeAction(new AudioServiceMock(), new DelayServiceMock(), new LoggerServiceMock(), null));
        }

        [Fact]
        public void duration_is_zero_if_there_are_no_ticks()
        {
            var sut = new MetronomeActionBuilder()
                .Build();

            Assert.Equal(TimeSpan.Zero, sut.Duration);
        }

        [Fact]
        public void duration_is_the_sum_of_all_tick_periods()
        {
            var sut = new MetronomeActionBuilder()
                .AddMetronomeTick(new MetronomeTick(TimeSpan.Zero))
                .AddMetronomeTick(new MetronomeTick(TimeSpan.FromSeconds(1)))
                .AddMetronomeTick(new MetronomeTick(TimeSpan.FromSeconds(2)))
                .AddMetronomeTick(new MetronomeTick(TimeSpan.FromMilliseconds(500)))
                .Build();

            Assert.Equal(TimeSpan.FromSeconds(3.5), sut.Duration);
        }

        [Fact]
        public async Task execute_async_throws_if_context_is_null()
        {
            var sut = new MetronomeActionBuilder()
                .Build();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.ExecuteAsync(null));
        }

        [Fact]
        public async Task execute_async_composes_the_appropriate_actions()
        {
            var audioService = new AudioServiceMock();
            var delayService = new DelayServiceMock();
            var actionsPerformed = new List<string>();

            audioService
                .When(x => x.PlayAsync(It.IsAny<string>()))
                .Do<string>((resource) => actionsPerformed.Add("Played audio resource " + resource))
                .Return(Task.FromResult(true));

            delayService
                .When(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Do<TimeSpan, CancellationToken>((period, ct) => actionsPerformed.Add("Delayed for " + period))
                .Return(Task.FromResult(true));

            var sut = new MetronomeActionBuilder()
                .WithAudioService(audioService)
                .WithDelayService(delayService)
                .AddMetronomeTick(new MetronomeTick(TimeSpan.Zero, MetronomeTickType.Bell))
                .AddMetronomeTick(new MetronomeTick(TimeSpan.FromMilliseconds(10)))
                .AddMetronomeTick(new MetronomeTick(TimeSpan.FromMilliseconds(20)))
                .AddMetronomeTick(new MetronomeTick(TimeSpan.FromMilliseconds(50), MetronomeTickType.Bell))
                .AddMetronomeTick(new MetronomeTick(TimeSpan.FromMilliseconds(30), MetronomeTickType.None))
                .Build();

            await sut.ExecuteAsync(new ExecutionContext());

            Assert.Equal(8, actionsPerformed.Count);
            Assert.Equal("Played audio resource Audio/MetronomeBell.mp3", actionsPerformed[0]);
            Assert.Equal("Delayed for 00:00:00.0100000", actionsPerformed[1]);
            Assert.Equal("Played audio resource Audio/MetronomeClick.mp3", actionsPerformed[2]);
            Assert.Equal("Delayed for 00:00:00.0200000", actionsPerformed[3]);
            Assert.Equal("Played audio resource Audio/MetronomeClick.mp3", actionsPerformed[4]);
            Assert.Equal("Delayed for 00:00:00.0500000", actionsPerformed[5]);
            Assert.Equal("Played audio resource Audio/MetronomeBell.mp3", actionsPerformed[6]);
            Assert.Equal("Delayed for 00:00:00.0300000", actionsPerformed[7]);
        }
    }
}