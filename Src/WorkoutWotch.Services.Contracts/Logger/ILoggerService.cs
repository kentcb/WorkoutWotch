using System;

namespace WorkoutWotch.Services.Contracts.Logger
{
    public interface ILoggerService
    {
        LogLevel Threshold
        {
            get;
            set;
        }

        bool IsDebugEnabled
        {
            get;
        }

        bool IsInfoEnabled
        {
            get;
        }

        bool IsPerfEnabled
        {
            get;
        }

        bool IsWarnEnabled
        {
            get;
        }

        bool IsErrorEnabled
        {
            get;
        }

        IObservable<LogEntry> Entries
        {
            get;
        }

        ILogger GetLogger(Type forType);

        ILogger GetLogger(string name);
    }
}

