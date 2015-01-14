namespace WorkoutWotch.Services.iOS.SystemNotifications
{
    using System;
    using System.Reactive;

    public interface ISystemNotificationsService
    {
        IObservable<Unit> DynamicTypeChanged
        {
            get;
        }
    }
}