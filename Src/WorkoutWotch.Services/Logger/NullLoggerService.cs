namespace WorkoutWotch.Services.Logger
{
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Subjects;
    using WorkoutWotch.Services.Contracts.Logger;

    public sealed class NullLoggerService : ILoggerService
    {
        public IObservable<LogEntry> Entries => NullSubject<LogEntry>.Instance;

        public bool IsDebugEnabled => false;

        public bool IsErrorEnabled => false;

        public bool IsInfoEnabled => false;

        public bool IsPerfEnabled => false;

        public bool IsWarnEnabled => false;

        public LogLevel Threshold
        {
            get { return LogLevel.Error; }
            set { }
        }

        public ILogger GetLogger(string name) => NullLogger.Instance;

        public ILogger GetLogger(Type forType) => NullLogger.Instance;

        private sealed class NullSubject<T> : ISubject<T>
        {
            public static NullSubject<T> Instance = new NullSubject<T>();

            private NullSubject()
            {
            }

            public IDisposable Subscribe(IObserver<T> observer) =>
                Disposable.Empty;

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(T value)
            {
            }
        }

        private sealed class NullLogger : ILogger
        {
            public static NullLogger Instance = new NullLogger();

            private NullLogger()
            {
            }

            public bool IsDebugEnabled => false;

            public bool IsErrorEnabled => false;

            public bool IsInfoEnabled => false;

            public bool IsPerfEnabled => false;

            public bool IsWarnEnabled => false;

            public string Name => "Null Logger";

            public void Log(LogLevel level, string message)
            {
            }
        }
    }
}