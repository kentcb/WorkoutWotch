namespace WorkoutWotch.Services.Contracts.Logger
{
    using System;

	public struct LogEntry
	{
        private readonly DateTime timestamp;
        private readonly string name;
        private readonly LogLevel level;
        private readonly int threadId;
        private readonly string message;

        public LogEntry(DateTime timestamp, string name, LogLevel level, int threadId, string message)
        {
            this.timestamp = timestamp;
            this.name = name;
            this.level = level;
            this.threadId = threadId;
            this.message = message;
        }

        public DateTime Timestamp
        {
            get { return this.timestamp; }
        }

        public string Name
        {
            get { return this.name; }
        }

        public LogLevel Level
        {
            get { return this.level; }
        }

        public int ThreadId
        {
            get { return this.threadId; }
        }

        public string Message
        {
            get { return this.message; }
        }
	}
}