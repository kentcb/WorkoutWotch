#if DEBUG

namespace WorkoutWotch.Services.Logger
{
    using System;
    using System.IO;
    using System.Text;
    using Contracts.Logger;
    using Humanizer;
    using Utility;

    public sealed class LogFileSink : DisposableBase
    {
        private readonly object sync;
        private readonly StreamWriter writer;
        private readonly Func<Stream> getInputStream;
        private readonly IDisposable entriesSubscription;

        public LogFileSink(ILoggerService loggerService, Stream outputStream, Func<Stream> getInputStream)
        {
            this.sync = new object();
            this.writer = new StreamWriter(outputStream, Encoding.UTF8);
            this.getInputStream = getInputStream;

            this.writer.WriteLine();
            this.writer.WriteLine();

            this.entriesSubscription = loggerService
                .Entries
                .Subscribe(this.Log);
        }

        public Stream GetLogFileStream() =>
            this.getInputStream();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.writer.Dispose();
                this.entriesSubscription.Dispose();
            }
        }

        private void Log(LogEntry logEntry)
        {
            lock (this.sync)
            {
                writer.WriteLine(
                    "{0:yyyy-MM-dd HH:mm:ss.fff} #{1:00} [{2,-5}] {3,-60} : {4}",
                    logEntry.Timestamp.ToLocalTime(),
                    logEntry.ThreadId,
                    logEntry.Level,
                    logEntry.Name.Truncate(60, Truncator.FixedLength, TruncateFrom.Left),
                    logEntry.Message);
                writer.Flush();

                System
                    .Diagnostics
                    .Debug
                    .WriteLine(
                        "{0:yyyy-MM-dd HH:mm:ss.fff} #{1:00} [{2,-5}] {3,-60} : {4}",
                        logEntry.Timestamp.ToLocalTime(),
                        logEntry.ThreadId,
                        logEntry.Level,
                        logEntry.Name.Truncate(60, Truncator.FixedLength, TruncateFrom.Left),
                        logEntry.Message);
            }
        }
    }
}

#endif