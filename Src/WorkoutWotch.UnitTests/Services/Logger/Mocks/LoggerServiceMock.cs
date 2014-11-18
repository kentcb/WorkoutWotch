using System;
using WorkoutWotch.Services.Contracts.Logger;
using Kent.Boogaart.PCLMock;
using System.Reactive.Linq;

namespace WorkoutWotch.UnitTests.Services.Logger.Mocks
{
    public sealed class LoggerServiceMock : MockBase<ILoggerService>, ILoggerService
    {
        public LoggerServiceMock(MockBehavior behavior = MockBehavior.Strict)
            : base(behavior)
        {
            if (behavior == MockBehavior.Loose)
            {
                this.When(x => x.GetLogger(It.IsAny<Type>())).Return(new LoggerMock(behavior));
                this.When(x => x.GetLogger(It.IsAny<string>())).Return(new LoggerMock(behavior));
                this.When(x => x.Entries).Return(Observable.Empty<LogEntry>());
            }
        }

        public ILogger GetLogger(Type forType)
        {
            return this.Apply(x => x.GetLogger(forType));
        }

        public ILogger GetLogger(string name)
        {
            return this.Apply(x => x.GetLogger(name));
        }

        public LogLevel Threshold
        {
            get
            {
                return this.Apply(x => x.Threshold);
            }
            set
            {
                this.ApplyPropertySet(x => x.Threshold, value);
            }
        }

        public bool IsDebugEnabled
        {
            get
            {
                return this.Apply(x => x.IsDebugEnabled);
            }
        }

        public bool IsInfoEnabled
        {
            get
            {
                return this.Apply(x => x.IsInfoEnabled);
            }
        }

        public bool IsPerfEnabled
        {
            get
            {
                return this.Apply(x => x.IsPerfEnabled);
            }
        }

        public bool IsWarnEnabled
        {
            get
            {
                return this.Apply(x => x.IsWarnEnabled);
            }
        }

        public bool IsErrorEnabled
        {
            get
            {
                return this.Apply(x => x.IsErrorEnabled);
            }
        }

        public IObservable<LogEntry> Entries
        {
            get
            {
                return this.Apply(x => x.Entries);
            }
        }
    }
}

