namespace WorkoutWotch.Services.Contracts.Logger
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reactive.Disposables;
    using Genesis.Ensure;
    using WorkoutWotch.Utility;

    // extends ILogger to include level-specific logging methods, as well as poor man's variadic templates
    // to reduce allocations in logging scenarios involving 10 or fewer format parameters
    public static class LoggerExtensions
    {
        [Conditional("DEBUG")]
        public static void Debug(this ILogger @this, string message)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));

            if (!@this.IsDebugEnabled)
            {
                return;
            }

            @this.Log(LogLevel.Debug, message);
        }

        [Conditional("DEBUG")]
        public static void Debug<T0>(this ILogger @this, string format, T0 arg0)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsDebugEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0);
            @this.Log(LogLevel.Debug, message);
        }

        [Conditional("DEBUG")]
        public static void Debug<T0, T1>(this ILogger @this, string format, T0 arg0, T1 arg1)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsDebugEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1);
            @this.Log(LogLevel.Debug, message);
        }

        [Conditional("DEBUG")]
        public static void Debug<T0, T1, T2>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsDebugEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2);
            @this.Log(LogLevel.Debug, message);
        }

        [Conditional("DEBUG")]
        public static void Debug<T0, T1, T2, T3>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsDebugEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3);
            @this.Log(LogLevel.Debug, message);
        }

        [Conditional("DEBUG")]
        public static void Debug<T0, T1, T2, T3, T4>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsDebugEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4);
            @this.Log(LogLevel.Debug, message);
        }

        [Conditional("DEBUG")]
        public static void Debug<T0, T1, T2, T3, T4, T5>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsDebugEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5);
            @this.Log(LogLevel.Debug, message);
        }

        [Conditional("DEBUG")]
        public static void Debug<T0, T1, T2, T3, T4, T5, T6>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsDebugEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
            @this.Log(LogLevel.Debug, message);
        }

        [Conditional("DEBUG")]
        public static void Debug<T0, T1, T2, T3, T4, T5, T6, T7>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsDebugEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            @this.Log(LogLevel.Debug, message);
        }

        [Conditional("DEBUG")]
        public static void Debug<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsDebugEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            @this.Log(LogLevel.Debug, message);
        }

        [Conditional("DEBUG")]
        public static void Debug<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsDebugEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
            @this.Log(LogLevel.Debug, message);
        }

        [Conditional("DEBUG")]
        public static void Debug(this ILogger @this, string format, params object[] args)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsDebugEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, args);
            @this.Log(LogLevel.Debug, message);
        }

        [Conditional("DEBUG")]
        public static void Debug(this ILogger @this, Exception exception, string format, params object[] args)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(exception, nameof(exception));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsDebugEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, args) + exception;
            @this.Log(LogLevel.Debug, message);
        }

        [Conditional("DEBUG")]
        public static void Info(this ILogger @this, string message)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));

            if (!@this.IsInfoEnabled)
            {
                return;
            }

            @this.Log(LogLevel.Info, message);
        }

        [Conditional("DEBUG")]
        public static void Info<T0>(this ILogger @this, string format, T0 arg0)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsInfoEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0);
            @this.Log(LogLevel.Info, message);
        }

        [Conditional("DEBUG")]
        public static void Info<T0, T1>(this ILogger @this, string format, T0 arg0, T1 arg1)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsInfoEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1);
            @this.Log(LogLevel.Info, message);
        }

        [Conditional("DEBUG")]
        public static void Info<T0, T1, T2>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsInfoEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2);
            @this.Log(LogLevel.Info, message);
        }

        [Conditional("DEBUG")]
        public static void Info<T0, T1, T2, T3>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsInfoEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3);
            @this.Log(LogLevel.Info, message);
        }

        [Conditional("DEBUG")]
        public static void Info<T0, T1, T2, T3, T4>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsInfoEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4);
            @this.Log(LogLevel.Info, message);
        }

        [Conditional("DEBUG")]
        public static void Info<T0, T1, T2, T3, T4, T5>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsInfoEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5);
            @this.Log(LogLevel.Info, message);
        }

        [Conditional("DEBUG")]
        public static void Info<T0, T1, T2, T3, T4, T5, T6>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsInfoEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
            @this.Log(LogLevel.Info, message);
        }

        [Conditional("DEBUG")]
        public static void Info<T0, T1, T2, T3, T4, T5, T6, T7>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsInfoEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            @this.Log(LogLevel.Info, message);
        }

        [Conditional("DEBUG")]
        public static void Info<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsInfoEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            @this.Log(LogLevel.Info, message);
        }

        [Conditional("DEBUG")]
        public static void Info<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsInfoEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
            @this.Log(LogLevel.Info, message);
        }

        [Conditional("DEBUG")]
        public static void Info(this ILogger @this, string format, params object[] args)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsInfoEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, args);
            @this.Log(LogLevel.Info, message);
        }

        [Conditional("DEBUG")]
        public static void Info(this ILogger @this, Exception exception, string format, params object[] args)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(exception, nameof(exception));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsInfoEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, args) + exception;
            @this.Log(LogLevel.Info, message);
        }

        public static IDisposable Perf(this ILogger @this, string message)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));

            if (!@this.IsPerfEnabled)
            {
                return Disposable.Empty;
            }

            return new PerfBlock(@this, message);
        }

        public static IDisposable Perf<T0>(this ILogger @this, string format, T0 arg0)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsPerfEnabled)
            {
                return Disposable.Empty;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0);
            return new PerfBlock(@this, message);
        }

        public static IDisposable Perf<T0, T1>(this ILogger @this, string format, T0 arg0, T1 arg1)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsPerfEnabled)
            {
                return Disposable.Empty;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1);
            return new PerfBlock(@this, message);
        }

        public static IDisposable Perf<T0, T1, T2>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsPerfEnabled)
            {
                return Disposable.Empty;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2);
            return new PerfBlock(@this, message);
        }

        public static IDisposable Perf<T0, T1, T2, T3>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsPerfEnabled)
            {
                return Disposable.Empty;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3);
            return new PerfBlock(@this, message);
        }

        public static IDisposable Perf<T0, T1, T2, T3, T4>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsPerfEnabled)
            {
                return Disposable.Empty;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4);
            return new PerfBlock(@this, message);
        }

        public static IDisposable Perf<T0, T1, T2, T3, T4, T5>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsPerfEnabled)
            {
                return Disposable.Empty;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5);
            return new PerfBlock(@this, message);
        }

        public static IDisposable Perf<T0, T1, T2, T3, T4, T5, T6>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsPerfEnabled)
            {
                return Disposable.Empty;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
            return new PerfBlock(@this, message);
        }

        public static IDisposable Perf<T0, T1, T2, T3, T4, T5, T6, T7>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsPerfEnabled)
            {
                return Disposable.Empty;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            return new PerfBlock(@this, message);
        }

        public static IDisposable Perf<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsPerfEnabled)
            {
                return Disposable.Empty;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            return new PerfBlock(@this, message);
        }

        public static IDisposable Perf<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsPerfEnabled)
            {
                return Disposable.Empty;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
            return new PerfBlock(@this, message);
        }

        [Conditional("DEBUG")]
        public static void Warn(this ILogger @this, string message)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));

            if (!@this.IsWarnEnabled)
            {
                return;
            }

            @this.Log(LogLevel.Warn, message);
        }

        [Conditional("DEBUG")]
        public static void Warn<T0>(this ILogger @this, string format, T0 arg0)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsWarnEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0);
            @this.Log(LogLevel.Warn, message);
        }

        [Conditional("DEBUG")]
        public static void Warn<T0, T1>(this ILogger @this, string format, T0 arg0, T1 arg1)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsWarnEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1);
            @this.Log(LogLevel.Warn, message);
        }

        [Conditional("DEBUG")]
        public static void Warn<T0, T1, T2>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsWarnEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2);
            @this.Log(LogLevel.Warn, message);
        }

        [Conditional("DEBUG")]
        public static void Warn<T0, T1, T2, T3>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsWarnEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3);
            @this.Log(LogLevel.Warn, message);
        }

        [Conditional("DEBUG")]
        public static void Warn<T0, T1, T2, T3, T4>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsWarnEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4);
            @this.Log(LogLevel.Warn, message);
        }

        [Conditional("DEBUG")]
        public static void Warn<T0, T1, T2, T3, T4, T5>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsWarnEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5);
            @this.Log(LogLevel.Warn, message);
        }

        [Conditional("DEBUG")]
        public static void Warn<T0, T1, T2, T3, T4, T5, T6>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsWarnEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
            @this.Log(LogLevel.Warn, message);
        }

        [Conditional("DEBUG")]
        public static void Warn<T0, T1, T2, T3, T4, T5, T6, T7>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsWarnEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            @this.Log(LogLevel.Warn, message);
        }

        [Conditional("DEBUG")]
        public static void Warn<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsWarnEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            @this.Log(LogLevel.Warn, message);
        }

        [Conditional("DEBUG")]
        public static void Warn<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsWarnEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
            @this.Log(LogLevel.Warn, message);
        }

        [Conditional("DEBUG")]
        public static void Warn(this ILogger @this, string format, params object[] args)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsWarnEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, args);
            @this.Log(LogLevel.Warn, message);
        }

        [Conditional("DEBUG")]
        public static void Warn(this ILogger @this, Exception exception, string format, params object[] args)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(exception, nameof(exception));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsWarnEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, args) + exception;
            @this.Log(LogLevel.Warn, message);
        }

        [Conditional("DEBUG")]
        public static void Error(this ILogger @this, string message)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));

            if (!@this.IsErrorEnabled)
            {
                return;
            }

            @this.Log(LogLevel.Error, message);
        }

        [Conditional("DEBUG")]
        public static void Error<T0>(this ILogger @this, string format, T0 arg0)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsErrorEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0);
            @this.Log(LogLevel.Error, message);
        }

        [Conditional("DEBUG")]
        public static void Error<T0, T1>(this ILogger @this, string format, T0 arg0, T1 arg1)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsErrorEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1);
            @this.Log(LogLevel.Error, message);
        }

        [Conditional("DEBUG")]
        public static void Error<T0, T1, T2>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsErrorEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2);
            @this.Log(LogLevel.Error, message);
        }

        [Conditional("DEBUG")]
        public static void Error<T0, T1, T2, T3>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsErrorEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3);
            @this.Log(LogLevel.Error, message);
        }

        [Conditional("DEBUG")]
        public static void Error<T0, T1, T2, T3, T4>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsErrorEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4);
            @this.Log(LogLevel.Error, message);
        }

        [Conditional("DEBUG")]
        public static void Error<T0, T1, T2, T3, T4, T5>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsErrorEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5);
            @this.Log(LogLevel.Error, message);
        }

        [Conditional("DEBUG")]
        public static void Error<T0, T1, T2, T3, T4, T5, T6>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsErrorEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
            @this.Log(LogLevel.Error, message);
        }

        [Conditional("DEBUG")]
        public static void Error<T0, T1, T2, T3, T4, T5, T6, T7>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsErrorEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            @this.Log(LogLevel.Error, message);
        }

        [Conditional("DEBUG")]
        public static void Error<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsErrorEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            @this.Log(LogLevel.Error, message);
        }

        [Conditional("DEBUG")]
        public static void Error<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this ILogger @this, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsErrorEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
            @this.Log(LogLevel.Error, message);
        }

        [Conditional("DEBUG")]
        public static void Error(this ILogger @this, string format, params object[] args)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsErrorEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, args);
            @this.Log(LogLevel.Error, message);
        }

        [Conditional("DEBUG")]
        public static void Error(this ILogger @this, Exception exception, string format, params object[] args)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(exception, nameof(exception));
            Ensure.ArgumentNotNull(format, nameof(format));

            if (!@this.IsErrorEnabled)
            {
                return;
            }

            var message = string.Format(CultureInfo.InvariantCulture, format, args) + exception;
            @this.Log(LogLevel.Error, message);
        }

        private sealed class PerfBlock : DisposableBase
        {
            private readonly ILogger owner;
            private readonly string message;
            private readonly Stopwatch stopwatch;

            public PerfBlock(ILogger owner, string message)
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