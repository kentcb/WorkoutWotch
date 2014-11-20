namespace WorkoutWotch.Services.iOS.SystemNotifications
{
    using System;
    using System.Reactive;
    using System.Reactive.Subjects;
    using MonoTouch.Foundation;
    using MonoTouch.UIKit;
    using WorkoutWotch.Utility;

    public sealed class SystemNotificationsService : DisposableBase, ISystemNotificationsService
    {
        private readonly Subject<Unit> dynamicTypeChanged;
        private NSObject contentSizeCategoryChangedObserver;

        public SystemNotificationsService()
        {
            this.dynamicTypeChanged = new Subject<Unit>();
            this.contentSizeCategoryChangedObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.ContentSizeCategoryChangedNotification, this.ContentSizeCategoryChanged);
        }

        public IObservable<Unit> DynamicTypeChanged
        {
            get { return this.dynamicTypeChanged; }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (this.contentSizeCategoryChangedObserver != null)
                {
                    NSNotificationCenter.DefaultCenter.RemoveObserver(this.contentSizeCategoryChangedObserver);
                }

                this.dynamicTypeChanged.Dispose();
            }
        }

        private void ContentSizeCategoryChanged(NSNotification notification)
        {
            this.dynamicTypeChanged.OnNext(Unit.Default);
        }
    }
}