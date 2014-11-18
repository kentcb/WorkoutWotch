namespace WorkoutWotch.UnitTests.Services.Logger.Mocks
{
    using System;
    using System.Reactive.Disposables;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Services.Contracts.Logger;

    public sealed class LoggerMock : MockBase<ILogger>, ILogger
    {
        public LoggerMock(MockBehavior behavior = MockBehavior.Strict)
            : base(behavior)
        {
            if (behavior == MockBehavior.Loose)
            {
                this.When(x => x.Perf(It.IsAny<string>())).Return(Disposable.Empty);
                this.When(x => x.Perf(It.IsAny<string>(), It.IsAny<object[]>())).Return(Disposable.Empty);
            }
        }

        public string Name
        {
            get { return this.Apply(x => x.Name); }
        }

        public bool IsDebugEnabled
        {
            get { return this.Apply(x => x.IsDebugEnabled); }
        }

        public bool IsInfoEnabled
        {
            get { return this.Apply(x => x.IsInfoEnabled); }
        }

        public bool IsPerfEnabled
        {
            get { return this.Apply(x => x.IsPerfEnabled); }
        }

        public bool IsWarnEnabled
        {
            get { return this.Apply(x => x.IsWarnEnabled); }
        }

        public bool IsErrorEnabled
        {
            get { return this.Apply(x => x.IsErrorEnabled); }
        }

        public void Debug(string message)
        {
            this.Apply(x => x.Debug(message));
        }

        public void Debug(string format, params object[] args)
        {
            this.Apply(x => x.Debug(format, args));
        }

        public void Debug(Exception exception, string format, params object[] args)
        {
            this.Apply(x => x.Debug(exception, format, args));
        }

        public void Info(string message)
        {
            this.Apply(x => x.Info(message));
        }

        public void Info(string format, params object[] args)
        {
            this.Apply(x => x.Info(format, args));
        }

        public void Info(Exception exception, string format, params object[] args)
        {
            this.Apply(x => x.Info(exception, format, args));
        }

        public IDisposable Perf(string message)
        {
            return this.Apply(x => x.Perf(message));
        }

        public IDisposable Perf(string format, params object[] args)
        {
            return this.Apply(x => x.Perf(format, args));
        }

        public void Warn(string message)
        {
            this.Apply(x => x.Warn(message));
        }

        public void Warn(string format, params object[] args)
        {
            this.Apply(x => x.Warn(format, args));
        }

        public void Warn(Exception exception, string format, params object[] args)
        {
            this.Apply(x => x.Warn(exception, format, args));
        }

        public void Error(string message)
        {
            this.Apply(x => x.Error(message));
        }

        public void Error(string format, params object[] args)
        {
            this.Apply(x => x.Error(format, args));
        }

        public void Error(Exception exception, string format, params object[] args)
        {
            this.Apply(x => x.Error(exception, format, args));
        }
    }
}