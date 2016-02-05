namespace WorkoutWotch.UI
{
    using System;
    using Utility;
    using WorkoutWotch.ViewModels;
    using Xamarin.Forms;

    public partial class App : Application
    {
        private static App instance;
        private readonly MainViewModel mainViewModel;

        public App(MainViewModel mainViewModel)
        {
            if (instance != null)
            {
                throw new InvalidOperationException("More than one App instance created - this is most unexpected and probably worth worrying about.");
            }

            instance = this;
            Ensure.ArgumentNotNull(mainViewModel, nameof(mainViewModel));

            InitializeComponent();

            this.mainViewModel = mainViewModel;
        }

        public void Initialize()
        {
            this.mainViewModel.Initialize();

            this.MainPage = new MainView(mainViewModel)
            {
                BarBackgroundColor = NavigationColor,
                BarTextColor = BackgroundColor
            };
        }

        public static Color BackgroundColor => (Color)instance.Resources[nameof(BackgroundColor)];

        public static Color ForegroundColor => (Color)instance.Resources[nameof(ForegroundColor)];

        public static Color NavigationColor => (Color)instance.Resources[nameof(NavigationColor)];

        public static Color DisabledColor => (Color)instance.Resources[nameof(DisabledColor)];
    }
}