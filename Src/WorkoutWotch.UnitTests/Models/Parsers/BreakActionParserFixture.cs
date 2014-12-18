namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using Sprache;
    using WorkoutWotch.Models.Parsers;
    using WorkoutWotch.UnitTests.Services.Container.Mocks;

    [TestFixture]
    public class BreakActionParserFixture
    {
        private const int msInSecond = 1000;
        private const int msInMinute = 60 * msInSecond;
        private const int msInHour = 60 * msInMinute;

        [Test]
        public void get_parser_throws_if_container_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => BreakActionParser.GetParser(null));
        }

        [TestCase("Break for 24s", 24 * msInSecond)]
        [TestCase("Break for 1m 24s", 1 * msInMinute + 24 * msInSecond)]
        [TestCase("break For 24s", 24 * msInSecond)]
        [TestCase("Break   For \t 24s", 24 * msInSecond)]
        public void can_parse_valid_input(string input, int expectedMilliseconds)
        {
            var result = BreakActionParser.GetParser(new ContainerServiceMock(MockBehavior.Loose)).Parse(input);
            Assert.AreEqual(TimeSpan.FromMilliseconds(expectedMilliseconds), result.Duration);
        }

        [TestCase("Brake for 24s")]
        [TestCase("Breakfor 24s")]
        [TestCase("Break for")]
        [TestCase("Break for tea")]
        public void cannot_parse_invalid_input(string input)
        {
            var result = BreakActionParser.GetParser(new ContainerServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.False(result.WasSuccessful);
        }
    }
}