namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;
    using Builders;
    using PCLMock;
    using WorkoutWotch.Models;
    using WorkoutWotch.UnitTests.Services.Delay.Mocks;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;
    using Xunit;

    public sealed class WaitWithPromptActionFixture
    {
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
        public void execute_composes_actions_appropriately_for_long_durations()
        {
            var delayService = new DelayServiceMock();
            var speechService = new SpeechServiceMock();
            var performedActions = new List<string>();

            delayService
                .When(x => x.Delay(It.IsAny<TimeSpan>()))
                .Do<TimeSpan>(duration => performedActions.Add("Delayed for " + duration))
                .Return(Observables.Unit);

            speechService
                .When(x => x.Speak(It.IsAny<string>()))
                .Do<string>(speechText => performedActions.Add("Saying '" + speechText + "'"))
                .Return(Observables.Unit);

            var sut = new WaitWithPromptActionBuilder()
                .WithDelayService(delayService)
                .WithSpeechService(speechService)
                .WithDuration(TimeSpan.FromSeconds(5))
                .WithPromptSpeechText("break")
                .Build();

            sut
                .Execute(new ExecutionContext())
                .Subscribe();

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
        public void execute_composes_actions_appropriately_for_short_durations()
        {
            var delayService = new DelayServiceMock();
            var speechService = new SpeechServiceMock();
            var performedActions = new List<string>();

            delayService
                .When(x => x.Delay(It.IsAny<TimeSpan>()))
                .Do<TimeSpan>(duration => performedActions.Add("Delayed for " + duration))
                .Return(Observables.Unit);

            speechService
                .When(x => x.Speak(It.IsAny<string>()))
                .Do<string>(speechText => performedActions.Add("Saying '" + speechText + "'"))
                .Return(Observables.Unit);

            var sut = new WaitWithPromptActionBuilder()
                .WithDelayService(delayService)
                .WithSpeechService(speechService)
                .WithDuration(TimeSpan.FromSeconds(1))
                .WithPromptSpeechText("break")
                .Build();

            sut
                .Execute(new ExecutionContext())
                .Subscribe();

            Assert.Equal(2, performedActions.Count);
            Assert.Equal("Saying 'break'", performedActions[0]);
            Assert.Equal("Delayed for 00:00:01", performedActions[1]);
        }

        [Theory]
        [InlineData("hello")]
        [InlineData("hello world")]
        [InlineData("goodbye")]
        public void execute_uses_the_specified_prompt_speech_text(string promptSpeechText)
        {
            var speechService = new SpeechServiceMock(MockBehavior.Loose);

            var sut = new WaitWithPromptActionBuilder()
                .WithSpeechService(speechService)
                .WithDuration(TimeSpan.FromSeconds(1))
                .WithPromptSpeechText(promptSpeechText)
                .Build();

            sut
                .Execute(new ExecutionContext())
                .Subscribe();

            speechService
                .Verify(x => x.Speak(promptSpeechText))
                .WasCalledExactlyOnce();
        }
    }
}