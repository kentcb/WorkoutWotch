namespace WorkoutWotch.UI.iOS
{
    using System;
    using System.Reactive;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using UIKit;
    using WorkoutWotch.Services.iOS.SystemNotifications;

    // a factory for creating UI controls
    // sets any control properties that aren't controllable via the appearance proxy functionality, and provides automatic dynamic type support where relevant
    internal static class ControlFactory
    {
        private static IObservable<Unit> sharedDynamicTypeChanged;

        public static void Initialize(ISystemNotificationsService systemNotificationService)
        {
            sharedDynamicTypeChanged = systemNotificationService
                .DynamicTypeChanged
                .Publish()
                .RefCount();
        }

        public static UILabel CreateLabel(PreferredFont font = PreferredFont.Body) =>
            new DynamicTypeAwareLabel(sharedDynamicTypeChanged, font);

        public static UIButton CreateButton(PreferredFont font = PreferredFont.Headline, UIButtonType type = UIButtonType.System) =>
            new DynamicTypeAwareButton(sharedDynamicTypeChanged, font, type);

        public static UITextField CreateTextField(PreferredFont font = PreferredFont.Body) =>
            new DynamicTypeAwareTextField(sharedDynamicTypeChanged, font)
            {
                AutocorrectionType = UITextAutocorrectionType.No,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                BorderStyle = UITextBorderStyle.RoundedRect,
                ClearButtonMode = UITextFieldViewMode.WhileEditing
            };

        public static UITextView CreateTextView(PreferredFont font = PreferredFont.Body) =>
            new DynamicTypeAwareTextView(sharedDynamicTypeChanged, font);

        public static UIPickerView CreatePicker() =>
            new UIPickerView();

        public static UISlider CreateSlider() =>
            new UISlider();

        public static UIActivityIndicatorView CreateActivityIndicator() => 
            new UIActivityIndicatorView
            {
                ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge
            };

        public static UIProgressView CreateProgressView() =>
            new UIProgressView();

        public static UIImageView CreateImage() =>
            new UIImageView();

        private static UIFont GetUIFontForPreferredFont(PreferredFont font)
        {
            switch (font)
            {
                case PreferredFont.Body:
                    return UIFont.PreferredBody;
                case PreferredFont.Caption1:
                    return UIFont.PreferredCaption1;
                case PreferredFont.Caption2:
                    return UIFont.PreferredCaption2;
                case PreferredFont.Footnote:
                    return UIFont.PreferredFootnote;
                case PreferredFont.Headline:
                    return UIFont.PreferredHeadline;
                case PreferredFont.Subheadline:
                    return UIFont.PreferredSubheadline;
                default:
                    throw new InvalidOperationException("Unknown preferred font: " + font);
            }
        }

        private sealed class DynamicTypeAwareLabel : UILabel
        {
            private readonly IObservable<Unit> dynamicTypeChanged;
            private readonly PreferredFont font;
            private readonly SerialDisposable subscription;

            public DynamicTypeAwareLabel(IObservable<Unit> dynamicTypeChanged, PreferredFont font)
            {
                this.dynamicTypeChanged = dynamicTypeChanged;
                this.font = font;
                this.subscription = new SerialDisposable();

                this.TextColor = Resources.ThemeDarkestColor;
            }

            public override void WillMoveToSuperview(UIView newsuper)
            {
                base.WillMoveToSuperview(newsuper);

                if (newsuper != null)
                {
                    this.subscription.Disposable = this.dynamicTypeChanged
                        .StartWith(Unit.Default)
                        .Subscribe(_ => this.UpdateFont());
                }
                else
                {
                    this.subscription.Disposable = null;
                }
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    this.subscription.Dispose();
                }
            }

            private void UpdateFont() =>
                this.Font = GetUIFontForPreferredFont(this.font);
        }

        private sealed class DynamicTypeAwareButton : UIButton
        {
            private readonly IObservable<Unit> dynamicTypeChanged;
            private readonly PreferredFont font;
            private readonly SerialDisposable subscription;

            public DynamicTypeAwareButton(IObservable<Unit> dynamicTypeChanged, PreferredFont font, UIButtonType type)
                : base(type)
            {
                this.dynamicTypeChanged = dynamicTypeChanged;
                this.font = font;
                this.subscription = new SerialDisposable();
            }

            public override void WillMoveToSuperview(UIView newsuper)
            {
                base.WillMoveToSuperview(newsuper);

                if (newsuper != null)
                {
                    this.subscription.Disposable = this.dynamicTypeChanged
                        .StartWith(Unit.Default)
                        .Subscribe(_ => this.UpdateFont());
                }
                else
                {
                    this.subscription.Disposable = null;
                }
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    this.subscription.Dispose();
                }
            }

            private void UpdateFont() =>
                this.Font = GetUIFontForPreferredFont(this.font);
        }

        private sealed class DynamicTypeAwareTextField : UITextField
        {
            private readonly IObservable<Unit> dynamicTypeChanged;
            private readonly PreferredFont font;
            private readonly SerialDisposable subscription;

            public DynamicTypeAwareTextField(IObservable<Unit> dynamicTypeChanged, PreferredFont font)
            {
                this.dynamicTypeChanged = dynamicTypeChanged;
                this.font = font;
                this.subscription = new SerialDisposable();
            }

            public override void WillMoveToSuperview(UIView newsuper)
            {
                base.WillMoveToSuperview(newsuper);

                if (newsuper != null)
                {
                    this.subscription.Disposable = this.dynamicTypeChanged
                        .StartWith(Unit.Default)
                        .Subscribe(_ => this.UpdateFont());
                }
                else
                {
                    this.subscription.Disposable = null;
                }
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    this.subscription.Dispose();
                }
            }

            private void UpdateFont() =>
                this.Font = GetUIFontForPreferredFont(this.font);
        }

        private sealed class DynamicTypeAwareTextView : UITextView
        {
            private readonly IObservable<Unit> dynamicTypeChanged;
            private readonly PreferredFont font;
            private readonly SerialDisposable subscription;

            public DynamicTypeAwareTextView(IObservable<Unit> dynamicTypeChanged, PreferredFont font)
            {
                this.dynamicTypeChanged = dynamicTypeChanged;
                this.font = font;
                this.subscription = new SerialDisposable();
            }

            public override void WillMoveToSuperview(UIView newsuper)
            {
                base.WillMoveToSuperview(newsuper);

                if (newsuper != null)
                {
                    this.subscription.Disposable = this.dynamicTypeChanged
                        .StartWith(Unit.Default)
                        .Subscribe(_ => this.UpdateFont());
                }
                else
                {
                    this.subscription.Disposable = null;
                }
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    this.subscription.Dispose();
                }
            }

            private void UpdateFont() =>
                this.Font = GetUIFontForPreferredFont(this.font);
        }
    }
}