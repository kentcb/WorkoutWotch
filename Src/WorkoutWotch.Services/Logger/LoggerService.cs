using System;
using WorkoutWotch.Services.Contracts.Logger;
using System.Reactive.Subjects;
using Kent.Boogaart.HelperTrinity.Extensions;
using System.Globalization;
using System.Reactive.Disposables;
using WorkoutWotch.Utility;
using System.Diagnostics;

namespace WorkoutWotch.Services.Logger
{
    public sealed class LoggerService : ILoggerService
    {
        private readonly Subject<LogEntry> entries;
        private LogLevel threshold;

        public LoggerService()
        {
            this.entries = new Subject<LogEntry>();
        }


        #region ILoggerService implementation
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
        public LogLevel Threshold
        {
            get
            {
                return this.threshold;
            }
            set
            {
                this.threshold = value;
            }
        }
        public bool IsDebugEnabled
        {
            get
            {
                return this.threshold <= LogLevel.Debug;
            }
        }
        public bool IsInfoEnabled
        {
            get
            {
                return this.threshold <= LogLevel.Info;
            }
        }
        public bool IsPerfEnabled
        {
            get
            {
                return this.threshold <= LogLevel.Perf;
            }
        }
        public bool IsWarnEnabled
        {
            get
            {
                return this.threshold <= LogLevel.Warn;
            }
        }
        public bool IsErrorEnabled
        {
            get
            {
                return this.threshold <= LogLevel.Error;
            }
        }
        public IObservable<LogEntry> Entries
        {
            get
            {
                return this.entries;
            }
        }
        #endregion

        private sealed class Logger : ILogger
        {
            private readonly LoggerService owner;
            private readonly string name;

            public Logger(LoggerService owner, string name)
            {
                this.owner = owner;
                this.name = name;
            }

            #region ILogger implementation
            public void Debug(string message)
            {
                message.AssertNotNull("message");

                if (!this.IsDebugEnabled)
                {
                    return;
                }

                this.Log(LogLevel.Debug, message);
            }

            public void Debug(string format, params object[] args)
            {
                format.AssertNotNull("format");
                args.AssertNotNull("args");

                if (!this.IsDebugEnabled)
                {
                    return;
                }

                var message = string.Format(CultureInfo.InvariantCulture, format, args);
                this.Log(LogLevel.Debug, message);
            }
            public void Debug(Exception exception, string format, params object[] args)
            {
                exception.AssertNotNull("exception");
                format.AssertNotNull("format");
                args.AssertNotNull("args");

                if (!this.IsDebugEnabled)
                {
                    return;
                }

                var message = string.Format(CultureInfo.InvariantCulture, format, args) + exception.ToString();
                this.Log(LogLevel.Debug, message);
            }
            public void Info(string message)
            {
                message.AssertNotNull("message");

                if (!this.IsInfoEnabled)
                {
                    return;
                }

                this.Log(LogLevel.Info, message);
            }
            public void Info(string format, params object[] args)
            {
                format.AssertNotNull("format");
                args.AssertNotNull("args");

                if (!this.IsInfoEnabled)
                {
                    return;
                }

                var message = string.Format(CultureInfo.InvariantCulture, format, args);
                this.Log(LogLevel.Info, message);
            }
            public void Info(Exception exception, string format, params object[] args)
            {
                exception.AssertNotNull("exception");
                format.AssertNotNull("format");
                args.AssertNotNull("args");

                if (!this.IsInfoEnabled)
                {
                    return;
                }

                var message = string.Format(CultureInfo.InvariantCulture, format, args) + exception.ToString();
                this.Log(LogLevel.Info, message);
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
                message.AssertNotNull("message");

                if (!this.IsWarnEnabled)
                {
                    return;
                }

                this.Log(LogLevel.Warn, message);
            }
            public void Warn(string format, params object[] args)
            {
                format.AssertNotNull("format");
                args.AssertNotNull("args");

                if (!this.IsWarnEnabled)
                {
                    return;
                }

                var message = string.Format(CultureInfo.InvariantCulture, format, args);
                this.Log(LogLevel.Warn, message);
            }
            public void Warn(Exception exception, string format, params object[] args)
            {
                exception.AssertNotNull("exception");
                format.AssertNotNull("format");
                args.AssertNotNull("args");

                if (!this.IsWarnEnabled)
                {
                    return;
                }

                var message = string.Format(CultureInfo.InvariantCulture, format, args) + exception.ToString();
                this.Log(LogLevel.Warn, message);
            }
            public void Error(string message)
            {
                message.AssertNotNull("message");

                if (!this.IsErrorEnabled)
                {
                    return;
                }

                this.Log(LogLevel.Error, message);
            }
            public void Error(string format, params object[] args)
            {
                format.AssertNotNull("format");
                args.AssertNotNull("args");

                if (!this.IsErrorEnabled)
                {
                    return;
                }

                var message = string.Format(CultureInfo.InvariantCulture, format, args);
                this.Log(LogLevel.Error, message);
            }
            public void Error(Exception exception, string format, params object[] args)
            {
                exception.AssertNotNull("exception");
                format.AssertNotNull("format");
                args.AssertNotNull("args");

                if (!this.IsErrorEnabled)
                {
                    return;
                }

                var message = string.Format(CultureInfo.InvariantCulture, format, args) + exception.ToString();
                this.Log(LogLevel.Error, message);
            }

            private void Log(LogLevel level, string message)
            {
                var entry = new LogEntry(DateTime.UtcNow, this.name, level, Environment.CurrentManagedThreadId, message);
                this.owner.entries.OnNext(entry);
            }

            public string Name
            {
                get
                {
                    return this.name;
                }
            }
            public bool IsDebugEnabled
            {
                get
                {
                    return this.owner.IsDebugEnabled;
                }
            }
            public bool IsInfoEnabled
            {
                get
                {
                    return this.owner.IsInfoEnabled;
                }
            }
            public bool IsPerfEnabled
            {
                get
                {
                    return this.owner.IsPerfEnabled;
                }
            }
            public bool IsWarnEnabled
            {
                get
                {
                    return this.owner.IsWarnEnabled;
                }
            }
            public bool IsErrorEnabled
            {
                get
                {
                    return this.owner.IsErrorEnabled;
                }
            }
            #endregion

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
                        this.owner.Log(LogLevel.Perf, string.Format(CultureInfo.InvariantCulture, "{0} [{1} ({2}ms)]", message, this.stopwatch.Elapsed, this.stopwatch.ElapsedMilliseconds));
                    }
                }
            }
        }
    }
}

