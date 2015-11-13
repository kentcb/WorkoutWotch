namespace WorkoutWotch.Services.Analytics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Contracts.Analytics;
    using Xamarin;

    public sealed class AnalyticsService : IAnalyticsService
    {
        public static readonly AnalyticsService Instance = new AnalyticsService();

        private AnalyticsService()
        {
        }

        public void Identify(string userId, IDictionary<string, string> metadata = null) =>
            Insights.Identify(userId, metadata);

        public void Track(string id, IDictionary<string, string> metadata = null) =>
            Insights.Track(id, metadata);

        public IDisposable TrackTime(string id, IDictionary<string, string> metadata = null) =>
            Insights.TrackTime(id, metadata);

        public void RecordException(Exception exception, ExceptionLevel exceptionLevel = ExceptionLevel.Error, IDictionary metadata = null) =>
            Insights.Report(exception, metadata, exceptionLevel.ToSeverity());
    }
}