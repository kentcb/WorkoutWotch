namespace WorkoutWotch.Services.Logger
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reactive.Disposables;
    using System.Reactive.Subjects;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using WorkoutWotch.Services.Contracts.Logger;
    using WorkoutWotch.Utility;

    public sealed class LoggerService : ILoggerService
    {
        private readonly Subject<LogEntry> entries;
        private LogLevel threshold;

        public LoggerService()
        {
            this.entries = new Subject<LogEntry>();
        }

        public LogLevel Threshold
        {
            get { return this.threshold; }
            set { this.threshold = value; }
        }

        public bool IsDebugEnabled
        {
            get { return this.IsLevelEnabled(LogLevel.Debug); }
        }

        public bool IsInfoEnabled
        {
            get { return this.IsLevelEnabled(LogLevel.Info); }
        }

        public bool IsPerfEnabled
        {
            get { return this.IsLevelEnabled(LogLevel.Perf); }
        }

        public bool IsWarnEnabled
        {
            get { return this.IsLevelEnabled(LogLevel.Warn); }
        }

        public bool IsErrorEnabled
        {
            get { return this.IsLevelEnabled(LogLevel.Error); }
        }

        public IObservable<LogEntry> Entries
        {
            get { return this.entries; }
        }

        public ILogger GetLogger(Type forType)
        {
            forType.AssertNotNull("forType");
            return this.GetLogger(forType.FullName);
        }

        public ILogger GetLogger(string name)
        {
            name.AssertNotNull("name");
            return new Logger(this, name);
        }

        private bool IsLevelEnabled(LogLevel level)
        {
            return this.threshold <= level;
        }

        private sealed class Logger : ILogger
        {
            private readonly LoggerService owner;
            private readonly string name;

            public Logger(LoggerService owner, string name)
            {
                this.owner = owner;
                this.name = name;
            }

            public string Name
            {
                get { return this.name; }
            }

            public bool IsDebugEnabled
            {
                get { return this.owner.IsDebugEnabled; }
            }

            public bool IsInfoEnabled
            {
                get { return this.owner.IsInfoEnabled; }
            }

            public bool IsPerfEnabled
            {
                get { return this.owner.IsPerfEnabled; }
            }

            public bool IsWarnEnabled
            {
                get { return this.owner.IsWarnEnabled; }
            }

            public bool IsErrorEnabled
            {
                get { return this.owner.IsErrorEnabled; }
            }

            public void Debug(string message)
            {
                this.Log(LogLevel.Debug, message);
            }

            public void Debug(string format, params object[] args)
            {
                this.Log(LogLevel.Debug, format, args);
            }

            public void Debug(Exception exception, string format, params object[] args)
            {
                this.Log(LogLevel.Debug, exception, format, args);
            }

            public void Info(string message)
            {
                this.Log(LogLevel.Info, message);
            }

            public void Info(string format, params object[] args)
            {
                this.Log(LogLevel.Info, format, args);
            }

            public void Info(Exception exception, string format, params object[] args)
            {
                this.Log(LogLevel.Info, exception, format, args);
            }

            public IDisposable Perf(string message)
            {
                message.AssertNotNull("message");

                if (!this.IsPerfEnabled)
                {
                    return Disposable.Empty;
                }

                return new PerfBlock(this, message);
            }

            public IDisposable Perf(string format, params object[] args)
            {
                format.AssertNotNull("format");
                args.AssertNotNull("args");

                if (!this.IsPerfEnabled)
                {
                    return Disposable.Empty;
                }

                var message = string.Format(CultureInfo.InvariantCulture, format, args);
                return new PerfBlock(this, message);
            }

            public void Warn(string message)
            {
                this.Log(LogLevel.Warn, message);
            }

            public void Warn(string format, params object[] args)
            {
                this.Log(LogLevel.Warn, format, args);
            }

            public void Warn(Exception exception, string format, params object[] args)
            {
                this.Log(LogLevel.Warn, exception, format, args);
            }

            public void Error(string message)
            {
                this.Log(LogLevel.Error, message);
            }

            public void Error(string format, params object[] args)
            {
                this.Log(LogLevel.Error, format, args);
            }

            public void Error(Exception exception, string format, params object[] args)
            {
                this.Log(LogLevel.Error, exception, format, args);
            }

            private void Log(LogLevel level, string format, params object[] args)
            {
                format.AssertNotNull("format");
                args.AssertNotNull("args");

                if (!this.owner.IsLevelEnabled(level))
                {
                    return;
                }

                var message = string.Format(CultureInfo.InvariantCulture, format, args);
                this.Log(level, message);
            }

            private void Log(LogLevel level, Exception exception, string format, params object[] args)
            {
                exception.AssertNotNull("exception");
                format.AssertNotNull("format");
                args.AssertNotNull("args");

                if (!this.owner.IsLevelEnabled(level))
                {
                    return;
                }

                var message = string.Format(CultureInfo.InvariantCulture, format, args) + exception.ToString();
                this.Log(level, message);
            }

            private void Log(LogLevel level, string message)
            {
                message.AssertNotNull("message");

                if (!this.owner.IsLevelEnabled(level))
                {
                    return;
                }

                var entry = new LogEntry(DateTime.UtcNow, this.name, level, Environment.CurrentManagedThreadId, message);
                this.owner.entries.OnNext(entry);
            }

            private sealed class PerfBlock : DisposableBase
            {
                private readonly Logger owner;
                private readonly string message;
                private readonly Stopwatch stopwatch;

                public PerfBlock(Logger owner, string message)
                {
                    this.owner = owner;
                    this.message = message;
                    this.stopwatch = Stopwatch.StartNew();
                }

                protected override void Dispose(bool disposing)
                {
                    base.Dispose(disposing);

                    if (disposing)
                    {
                        this.stopwatch.Stop();
                        this.owner.Log(LogLevel.Perf, "{0} [{1} ({2}ms)]", message, this.stopwatch.Elapsed, this.stopwatch.ElapsedMilliseconds);
                    }
                }
            }
        }
    }
}

