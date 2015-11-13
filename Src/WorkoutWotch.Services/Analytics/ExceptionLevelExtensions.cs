namespace WorkoutWotch.Services.Analytics
{
    using System;
    using Contracts.Analytics;
    using Xamarin;

    public static class ExceptionLevelExtensions
    {
        public static Insights.Severity ToSeverity(this ExceptionLevel @this)
        {
            switch (@this)
            {
                case ExceptionLevel.Warning:
                    return Insights.Severity.Warning;
                case ExceptionLevel.Error:
                    return Insights.Severity.Error;
                case ExceptionLevel.Critial:
                    return Insights.Severity.Critical;
                default:
                    throw new NotSupportedException("Unsupported exception level: " + @this);
            }
        }
    }
}