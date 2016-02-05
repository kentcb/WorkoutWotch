namespace WorkoutWotch.UnitTests.Services.Logger
{
    using System;
    using System.Text.RegularExpressions;
    using global::ReactiveUI;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Logger;
    using Xunit;

    public class LoggerServiceFixture
    {
        [Fact]
        public void is_debug_enabled_honors_threshold()
        {
            var sut = new LoggerService();
            sut.Threshold = LogLevel.Debug;
            Assert.True(sut.IsDebugEnabled);
            sut.Threshold = LogLevel.Info;
            Assert.False(sut.IsDebugEnabled);
        }

        [Fact]
        public void is_info_enabled_honors_threshold()
        {
            var sut = new LoggerService();
            sut.Threshold = LogLevel.Info;
            Assert.True(sut.IsInfoEnabled);
            sut.Threshold = LogLevel.Warn;
            Assert.False(sut.IsInfoEnabled);
        }

        [Fact]
        public void is_perf_enabled_honors_threshold()
        {
            var sut = new LoggerService();
            sut.Threshold = LogLevel.Perf;
            Assert.True(sut.IsPerfEnabled);
            sut.Threshold = LogLevel.Warn;
            Assert.False(sut.IsPerfEnabled);
        }

        [Fact]
        public void is_warn_enabled_honors_threshold()
        {
            var sut = new LoggerService();
            sut.Threshold = LogLevel.Warn;
            Assert.True(sut.IsWarnEnabled);
            sut.Threshold = LogLevel.Error;
            Assert.False(sut.IsWarnEnabled);
        }

        [Fact]
        public void is_error_enabled_is_always_true()
        {
            var sut = new LoggerService();
            sut.Threshold = LogLevel.Info;
            Assert.True(sut.IsErrorEnabled);
            sut.Threshold = LogLevel.Error;
            Assert.True(sut.IsErrorEnabled);
        }

        [Fact]
        public void get_logger_for_type_returns_a_logger_with_the_full_name_of_the_type_as_its_name()
        {
            var sut = new LoggerService();
            var logger = sut.GetLogger(this.GetType());
            Assert.Equal(this.GetType().FullName, logger.Name);
        }

#if DEBUG

        [Fact]
        public void log_entries_ticks_for_log_calls_within_the_configured_threshold()
        {
            var sut = new LoggerService();
            var logger = sut.GetLogger("test");
            var entries = sut
                .Entries
                .CreateCollection();

            sut.Threshold = LogLevel.Info;
            logger.Debug("Whatever");
            logger.Debug("foo");
            logger.Debug("bar");
            logger.Info("An informational message");
            logger.Debug("foo");
            logger.Warn("A warning message");
            logger.Debug("foo");
            logger.Debug("foo");
            logger.Error("An error message");

            Assert.Equal(3, entries.Count);
            Assert.Equal("An informational message", entries[0].Message);
            Assert.Equal(LogLevel.Info, entries[0].Level);
            Assert.Equal("A warning message", entries[1].Message);
            Assert.Equal(LogLevel.Warn, entries[1].Level);
            Assert.Equal("An error message", entries[2].Message);
            Assert.Equal(LogLevel.Error, entries[2].Level);
        }

        [Fact]
        public void log_entries_can_be_formatted()
        {
            var sut = new LoggerService();
            var logger = sut.GetLogger("test");
            var entry = sut
                .Entries
                .CreateCollection();

            logger.Debug("A message with a parameter: {0}", 42);

            Assert.Equal(1, entry.Count);
            Assert.Equal("A message with a parameter: 42", entry[0].Message);
        }

        [Fact]
        public void log_entries_can_contain_exception_details()
        {
            var sut = new LoggerService();
            var logger = sut.GetLogger("test");
            var entry = sut
                .Entries
                .CreateCollection();

            logger.Debug(new InvalidOperationException("foo"), "A message with an exception and a parameter ({0}): ", 42);

            Assert.Equal(1, entry.Count);
            Assert.Equal("A message with an exception and a parameter (42): System.InvalidOperationException: foo", entry[0].Message);
        }

        [Fact]
        public void logging_perf_is_a_noop_if_perf_level_is_disabled()
        {
            var sut = new LoggerService();
            var logger = sut.GetLogger("test");
            sut.Threshold = LogLevel.Warn;
            var entry = sut
                .Entries
                .CreateCollection();

            using (logger.Perf("This shouldn't be logged"))
            {
            }

            logger.Warn("This should be logged");

            Assert.Equal(1, entry.Count);
            Assert.Equal("This should be logged", entry[0].Message);
        }

#endif

        [Fact]
        public void logging_perf_adds_extra_performance_information_to_the_log_message()
        {
            var sut = new LoggerService();
            var logger = sut.GetLogger("test");
            var entry = sut
                .Entries
                .CreateCollection();

            using (logger.Perf("Some performance {0}", "entry"))
            {
            }

            Assert.Equal(1, entry.Count);
            // Some performance entry [00:00:00.0045605 (4ms)]
            Assert.True(Regex.IsMatch(entry[0].Message, @"Some performance entry \[\d\d:\d\d:\d\d\.\d*? \(\d*?ms\)\]"));
        }
    }
}