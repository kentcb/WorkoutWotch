namespace WorkoutWotch.Services.Contracts.Logger
{
    using System;

    public interface ILogger
    {
        string Name
        {
            get;
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

        void Debug(string message);

        void Debug(string format, params object[] args);

        void Debug(Exception exception, string format, params object[] args);

        void Info(string message);

        void Info(string format, params object[] args);

        void Info(Exception exception, string format, params object[] args);

        IDisposable Perf(string message);

        IDisposable Perf(string format, params object[] args);

        void Warn(string message);

        void Warn(string format, params object[] args);

        void Warn(Exception exception, string format, params object[] args);

        void Error(string message);

        void Error(string format, params object[] args);

        void Error(Exception exception, string format, params object[] args);
    }
}