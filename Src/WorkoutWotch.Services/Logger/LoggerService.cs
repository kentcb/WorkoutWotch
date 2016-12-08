namespace WorkoutWotch.Services.Logger
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using Genesis.Ensure;
    using WorkoutWotch.Services.Contracts.Logger;

    public sealed class LoggerService : ILoggerService
    {
        private readonly IDictionary<string, ILogger> loggers;
        private readonly ISubject<LogEntry> entries;
        private readonly object sync;
        private LogLevel threshold;

        public LoggerService()
        {
            this.loggers = new Dictionary<string, ILogger>();
            this.entries = new ReplaySubject<LogEntry>(64);
            this.sync = new object();
        }

        public LogLevel Threshold
        {
            get { return this.threshold; }
            set { this.threshold = value; }
        }

        public bool IsDebugEnabled => this.threshold <= LogLevel.Debug;

        public bool IsInfoEnabled => this.threshold <= LogLevel.Info;

        public bool IsPerfEnabled => this.threshold <= LogLevel.Perf;

        public bool IsWarnEnabled => this.threshold <= LogLevel.Warn;

        public bool IsErrorEnabled => this.threshold <= LogLevel.Error;

        public IObservable<LogEntry> Entries => this.entries.Where(x => x.Level >= this.Threshold);

        public ILogger GetLogger(Type forType)
        {
            Ensure.ArgumentNotNull(forType, nameof(forType));

            if (forType.IsConstructedGenericType)
            {
                forType = forType.GetGenericTypeDefinition();
            }

            return this.GetLogger(forType.FullName);
        }

        public ILogger GetLogger(string name)
        {
            Ensure.ArgumentNotNull(name, nameof(name));

            lock (this.sync)
            {
                ILogger logger;

                if (!this.loggers.TryGetValue(name, out logger))
                {
                    logger = new Logger(this, name);
                    this.loggers.Add(name, logger);
                }

                return logger;
            }
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

            public string Name => this.name;

            public bool IsDebugEnabled => this.owner.IsDebugEnabled;

            public bool IsInfoEnabled => this.owner.IsInfoEnabled;

            public bool IsPerfEnabled => this.owner.IsPerfEnabled;

            public bool IsWarnEnabled => this.owner.IsWarnEnabled;

            public bool IsErrorEnabled => this.owner.IsErrorEnabled;

            public void Log(LogLevel level, string message)
            {
                var entry = new LogEntry(DateTime.UtcNow, this.name, level, Environment.CurrentManagedThreadId, message);
                this.owner.entries.OnNext(entry);
            }
        }
    }
}