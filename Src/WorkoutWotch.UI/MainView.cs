namespace WorkoutWotch.UI
{
    using Genesis.Ensure;
    using global::ReactiveUI.XamForms;
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