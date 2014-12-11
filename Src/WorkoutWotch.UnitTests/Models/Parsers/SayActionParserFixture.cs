using System;
using NUnit.Framework;
using WorkoutWotch.Models.Parsers;
using Sprache;
using WorkoutWotch.UnitTests.Services.Speech.Mocks;

namespace WorkoutWotch.UnitTests.Models.Parsers
{
    [TestFixture]
    public class SayActionParserFixture
    {
        [Test]
        public void get_parser_throws_if_speech_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => SayActionParser.GetParser(null));
        }

        [TestCase("Say 'hello'", "hello")]
        [TestCase(@"Say ""hello""", "hello")]
        [TestCase("say 'hello'", "hello")]
        [TestCase("SAY 'hello'", "hello")]
        [TestCase("Say    \t \t   'hello'", "hello")]
        [TestCase(@"Say 'hello, how are you \'friend\'?'", "hello, how are you 'friend'?")]
        public void can_parse_correctly_formatted_input(string input, string expectedSpeechText)
        {
            var result = SayActionParser.GetParser(new SpeechServiceMock()).Parse(input);
            Assert.NotNull(result);
            Assert.AreEqual(expectedSpeechText, result.SpeechText);
        }

        [TestCase("Say")]
        [TestCase("Say hello")]
        [TestCase("Sai 'hello'")]
        [TestCase("Say'hello'")]
        public void cannot_parse_incorrectly_formatted_input(string input)
        {
            var result = SayActionParser.GetParser(new SpeechServiceMock())(new Input(input));
            Assert.False(result.WasSuccessful);
        }
    }
}

