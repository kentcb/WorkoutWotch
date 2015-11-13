namespace WorkoutWotch.Services.Contracts.Analytics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public interface IAnalyticsService
    {
        void Identify(
            string userId,
            IDictionary<string, string> metadata = null);

        void Track(
            string id,
            IDictionary<string, string> metadata = null);

        IDisposable TrackTime(
            string id,
            IDictionary<string, string> metadata = null);

        // technically, this is all we'll be able to use without a paid subscription :(
        void RecordException(
            Exception exception,
            ExceptionLevel exceptionLevel = ExceptionLevel.Error,
            IDictionary metadata = null);
    }
}