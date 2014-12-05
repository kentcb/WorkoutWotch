namespace WorkoutWotch.UnitTests.Services.Logger
{
    using System;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Services.Logger;

    [TestFixture]
    public class LoggerServiceFixture
    {
        [Test]
        public void is_debug_enabled_honors_threshold()
        {
            var sut = new LoggerService();
            sut.Threshold = LogLevel.Debug;
            Assert.True(sut.IsDebugEnabled);
            sut.Threshold = LogLevel.Info;
            Assert.False(sut.IsDebugEnabled);
        }

        [Test]
        public void is_info_enabled_honors_threshold()
        {
            var sut = new LoggerService();
            sut.Threshold = LogLevel.Info;
            Assert.True(sut.IsInfoEnabled);
            sut.Threshold = LogLevel.Warn;
            Assert.False(sut.IsInfoEnabled);
        }

        [Test]
        public void is_perf_enabled_honors_threshold()
        {
            var sut = new LoggerService();
            sut.Threshold = LogLevel.Perf;
            Assert.True(sut.IsPerfEnabled);
            sut.Threshold = LogLevel.Warn;
            Assert.False(sut.IsPerfEnabled);
        }

        [Test]
        public void is_warn_enabled_honors_threshold()
        {
            var sut = new LoggerService();
            sut.Threshold = LogLevel.Warn;
            Assert.True(sut.IsWarnEnabled);
            sut.Threshold = LogLevel.Error;
            Assert.False(sut.IsWarnEnabled);
        }

        [Test]
        public void is_error_enabled_is_always_true()
        {
            var sut = new LoggerService();
            sut.Threshold = LogLevel.Info;
            Assert.True(sut.IsErrorEnabled);
            sut.Threshold = LogLevel.Error;
            Assert.True(sut.IsErrorEnabled);
        }

        [Test]
        public void get_logger_throws_if_type_is_null()
        {
            var sut = new LoggerService();
            Assert.Throws<ArgumentNullException>(() => sut.GetLogger((Type)null));
        }

        [Test]
        public void get_logger_throws_if_name_is_null()
        {
            var sut = new LoggerService();
            Assert.Throws<ArgumentNullException>(() => sut.GetLogger((string)null));
        }

        [Test]
        public void get_logger_for_type_returns_a_logger_with_the_full_name_of_the_type_as_its_name()
        {
            var sut = new LoggerService();
            var logger = sut.GetLogger(this.GetType());
            Assert.AreEqual(this.GetType().FullName, logger.Name);
        }

        [Test]
        public async Task log_entries_ticks_for_log_calls_within_the_configured_threshold()
        {
            var sut = new LoggerService();
            var logger = sut.GetLogger("test");
            var entriesTask = sut
                .Entries
                .Take(3)
                .ToListAsync()
                .ToTask();

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

            var entries = await entriesTask;

            Assert.AreEqual("An informational message", entries[0].Message);
            Assert.AreEqual(LogLevel.Info, entries[0].Level);
            Assert.AreEqual("A warning message", entries[1].Message);
            Assert.AreEqual(LogLevel.Warn, entries[1].Level);
            Assert.AreEqual("An error message", entries[2].Message);
            Assert.AreEqual(LogLevel.Error, entries[2].Level);
        }

        [Test]
        public async Task log_entries_can_be_formatted()
        {
            var sut = new LoggerService();
            var logger = sut.GetLogger("test");
            var entryTask = sut
                .Entries
                .FirstAsync()
                .Timeout(TimeSpan.FromSeconds(3))
                .ToTask();

            logger.Debug("A message with a parameter: {0}", 42);

            var entry = await entryTask;

            Assert.AreEqual("A message with a parameter: 42", entry.Message);
        }

        [Test]
        public async Task log_entries_can_contain_exception_details()
        {
            var sut = new LoggerService();
            var logger = sut.GetLogger("test");
            var entryTask = sut
                .Entries
                .FirstAsync()
                .Timeout(TimeSpan.FromSeconds(3))
                .ToTask();

            logger.Debug(new InvalidOperationException("foo"), "A message with an exception and a parameter ({0}): ", 42);

            var entry = await entryTask;

            Assert.AreEqual("A message with an exception and a parameter (42): System.InvalidOperationException: foo", entry.Message);
        }

        [Test]
        public async Task logging_perf_is_a_noop_if_perf_level_is_disabled()
        {
            var sut = new LoggerService();
            var logger = sut.GetLogger("test");
            sut.Threshold = LogLevel.Warn;
            var entryTask = sut
                .Entries
                .FirstAsync()
                .Timeout(TimeSpan.FromSeconds(3))
                .ToTask();

            using (logger.Perf("This shouldn't be logged"))
            {
            }

            logger.Warn("This should be logged");

            var entry = await entryTask;

            Assert.AreEqual("This should be logged", entry.Message);
        }

        [Test]
        public async Task logging_perf_adds_extra_performance_information_to_the_log_message()
        {
            var sut = new LoggerService();
            var logger = sut.GetLogger("test");
            var entryTask = sut
                .Entries
                .FirstAsync()
                .Timeout(TimeSpan.FromSeconds(3))
                .ToTask();

            using (logger.Perf("Some performance {0}", "entry"))
            {
            }

            var entry = await entryTask;

            // Some performance entry [00:00:00.0045605 (4ms)]
            Assert.True(Regex.IsMatch(entry.Message, @"Some performance entry \[\d\d:\d\d:\d\d\.\d*? \(\d*?ms\)\]"));
        }
    }
}