namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using Builders;
    using PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.UnitTests.Services.Delay.Mocks;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;
    using Xunit;

    public class WaitWithPromptActionFixture
    {
        [Fact]
        public void ctor_throws_if_delay_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new WaitWithPromptAction(null, new SpeechServiceMock(), TimeSpan.FromSeconds(30), "whatever"));
        }

        [Fact]
        public void ctor_throws_if_speech_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new WaitWithPromptAction(new DelayServiceMock(), null, TimeSpan.FromSeconds(30), "whatever"));
        }

        [Fact]
        public void ctor_throws_if_duration_is_less_than_zero()
        {
            Assert.Throws<ArgumentException>(() => new WaitWithPromptAction(new DelayServiceMock(), new SpeechServiceMock(), TimeSpan.FromSeconds(-2), "whatever"));
        }

        [Fact]
        public void ctor_throws_if_start_speech_text_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new WaitWithPromptAction(new DelayServiceMock(), new SpeechServiceMock(), TimeSpan.Zero, null));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(23498)]
        public void duration_yields_the_duration_passed_into_ctor(int durationInMs)
        {
            var sut = new WaitWithPromptActionBuilder()
                .WithDuration(TimeSpan.FromMilliseconds(durationInMs))
                .Build();

            Assert.Equal(TimeSpan.FromMilliseconds(durationInMs), sut.Duration);
        }

        [Fact]
        public void execute_async_throws_if_the_context_is_null()
        {
            var sut = new WaitWithPromptActionBuilder()
                .Build();

            Assert.Throws<ArgumentNullException>(() => sut.ExecuteAsync(null));
        }

        [Fact]
        public void execute_async_composes_actions_appropriately_for_long_durations()
        {
            var delayService = new DelayServiceMock();
            var speechService = new SpeechServiceMock();
            var performedActions = new List<string>();

            delayService
                .When(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Do<TimeSpan, CancellationToken>((duration, ct) => performedActions.Add("Delayed for " + duration))
                .Return(Observable.Return(Unit.Default));

            speechService
                .When(x => x.SpeakAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Do<string, CancellationToken>((speechText, cty) => performedActions.Add("Saying '" + speechText + "'"))
                .Return(Observable.Return(Unit.Default));

            var sut = new WaitWithPromptActionBuilder()
                .WithDelayService(delayService)
                .WithSpeechService(speechService)
                .WithDuration(TimeSpan.FromSeconds(5))
                .WithPromptSpeechText("break")
                .Build();

            sut.ExecuteAsync(new ExecutionContext());

            Assert.Equal(7, performedActions.Count);
            Assert.Equal("Saying 'break'", performedActions[0]);
            Assert.Equal("Delayed for 00:00:01", performedActions[1]);
            Assert.Equal("Delayed for 00:00:01", performedActions[2]);
            Assert.Equal("Delayed for 00:00:01", performedActions[3]);
            Assert.Equal("Saying 'ready?'", performedActions[4]);
            Assert.Equal("Delayed for 00:00:01", performedActions[5]);
            Assert.Equal("Delayed for 00:00:01", performedActions[6]);
        }

        [Fact]
        public void execute_async_composes_actions_appropriately_for_short_durations()
        {
            var delayService = new DelayServiceMock();
            var speechService = new SpeechServiceMock();
            var performedActions = new List<string>();

            delayService
                .When(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Do<TimeSpan, CancellationToken>((duration, ct) => performedActions.Add("Delayed for " + duration))
                .Return(Observable.Return(Unit.Default));

            speechService
                .When(x => x.SpeakAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Do<string, CancellationToken>((speechText, cty) => performedActions.Add("Saying '" + speechText + "'"))
                .Return(Observable.Return(Unit.Default));

            var sut = new WaitWithPromptActionBuilder()
                .WithDelayService(delayService)
                .WithSpeechService(speechService)
                .WithDuration(TimeSpan.FromSeconds(1))
                .WithPromptSpeechText("break")
                .Build();

            sut.ExecuteAsync(new ExecutionContext());

            Assert.Equal(2, performedActions.Count);
            Assert.Equal("Saying 'break'", performedActions[0]);
            Assert.Equal("Delayed for 00:00:01", performedActions[1]);
        }

        [Theory]
        [InlineData("hello")]
        [InlineData("hello world")]
        [InlineData("goodbye")]
        public void execute_async_uses_the_specified_prompt_speech_text(string promptSpeechText)
        {
            var speechService = new SpeechServiceMock(MockBehavior.Loose);

            var sut = new WaitWithPromptActionBuilder()
                .WithSpeechService(speechService)
                .WithDuration(TimeSpan.FromSeconds(1))
                .WithPromptSpeechText(promptSpeechText)
                .Build();

            sut.ExecuteAsync(new ExecutionContext());

            speechService
                .Verify(x => x.SpeakAsync(promptSpeechText, It.IsAny<CancellationToken>()))
                .WasCalledExactlyOnce();
        }
    }
}