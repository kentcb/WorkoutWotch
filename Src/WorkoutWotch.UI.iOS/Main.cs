namespace WorkoutWotch.UI.iOS
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using Genesis.Logging;
    using UIKit;

    public sealed class Application
    {
        static void Main(string[] args)
        {
            ConfigureAmbientLoggerService();
            DirectLoggingOutputToConsole();

            UIApplication.Main(args, null, "AppDelegate");
        }

        [Conditional("LOGGING")]
        private static void ConfigureAmbientLoggerService() =>
            LoggerService.Current = new DefaultLoggerService();

        [Conditional("LOGGING")]
        private static void DirectLoggingOutputToConsole() =>
            LoggerService
                .Current
                .Entries
                .SubscribeSafe(
                    entry =>
                    {
                        Console.Out.Write(entry.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
                        Console.Out.Write(" [");
                        Console.Out.Write(entry.Level.ToString());
                        Console.Out.Write("] #");
                        Console.Out.Write(entry.ThreadId);
                        Console.Out.Write(" ");
                        Console.Out.Write(entry.Name);
                        Console.Out.Write(" : ");
                        Console.Out.WriteLine(entry.Message);
                    });
    }
}