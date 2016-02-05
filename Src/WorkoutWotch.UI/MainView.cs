namespace WorkoutWotch.UI
{
    using global::ReactiveUI.XamForms;
    using Utility;
    using ViewModels;

    public sealed class MainView : RoutedViewHost
    {
        public MainView(MainViewModel viewModel)
        {
            Ensure.ArgumentNotNull(viewModel, nameof(viewModel));
            this.Router = viewModel.Router;
        }
    }
}