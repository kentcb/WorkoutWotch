using System;
using System.Reactive;

namespace WorkoutWotch.Services.iOS.SystemNotifications
{
    public interface ISystemNotificationsService
    {
        IObservable<Unit> DynamicTypeChanged
        {
            get;
        }
    }
}

