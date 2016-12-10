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