namespace WorkoutWotch.UI
{
    using Behaviors;
    using Genesis.Logging;
    using global::ReactiveUI;
    using global::ReactiveUI.XamForms;
    using WorkoutWotch.ViewModels;

    public partial class ExerciseProgramCellView : ReactiveTextCell<ExerciseProgramViewModel>
    {
        public ExerciseProgramCellView()
        {
            var logger = LoggerService.GetLogger(this.GetType());

            using (logger.Perf("Initialize component."))
            {
                InitializeComponent();
            }

            // TODO: setting this from XAML is currently causing a compilation exception (last tried with XF 2.0.0)
            CellBehavior.SetAccessory(this, AccessoryType.HasChildView);

            this
                .WhenActivated(
                    disposables =>
                    {
                        using (logger.Perf("Activate."))
                        {
                            this.Text = this.ViewModel.Name;
                            this.Detail = this.ViewModel.Duration.ToString("mm\\:ss");
                        }
                    });
        }
    }
}