namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using Sprache;
    using WorkoutWotch.Models.Parsers;
    using WorkoutWotch.UnitTests.Services.Container.Mocks;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;

    [TestFixture]
    public class PrepareActionParserFixture
    {
        private const int msInSecond = 1000;
        private const int msInMinute = 60 * msInSecond;
        private const int msInHour = 60 * msInMinute;

        [Test]
        public void get_parser_throws_if_container_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => PrepareActionParser.GetParser(null));
        }

        [TestCase("Prepare for 24s", 24 * msInSecond)]
        [TestCase("Prepare for 1m 24s", 1 * msInMinute + 24 * msInSecond)]
        [TestCase("prepare For 24s", 24 * msInSecond)]
        [TestCase("Prepare   For \t 24s", 24 * msInSecond)]
        public void can_parse_valid_input(string input, int expectedMilliseconds)
        {
            var result = PrepareActionParser.GetParser(new ContainerServiceMock(MockBehavior.Loose)).Parse(input);
            Assert.AreEqual(TimeSpan.FromMilliseconds(expectedMilliseconds), result.Duration);
        }

        [TestCase("Preapre for 24s")]
        [TestCase("Preparefor 24s")]
        [TestCase("Prepare for")]
        [TestCase("Prepare for tea")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = PrepareActionParser.GetParser(new ContainerServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.False(result.WasSuccessful);
        }
    }
}