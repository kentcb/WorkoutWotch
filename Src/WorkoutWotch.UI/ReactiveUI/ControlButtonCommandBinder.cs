namespace WorkoutWotch.UI.ReactiveUI
{
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Windows.Input;
    using Controls;
    using global::ReactiveUI;
    using Xamarin.Forms;

    // allows us to bind commands to our custom ControlButton control
    public sealed class ControlButtonCommandBinder : ICreatesCommandBinding
    {
        public int GetAffinityForObject(Type type, bool hasEventTarget) =>
            type.GetTypeInfo().IsAssignableFrom(typeof(ControlButton).GetTypeInfo()) ? 100 : 0;

        public IDisposable BindCommandToObject(ICommand command, object target, IObservable<object> commandParameter)
        {
            var controlButton = (ControlButton)target;
            var disposables = new CompositeDisposable();

            controlButton
                .TapGestureRecognizer
                .Events()
                .Tapped
                .Where(_ => controlButton.IsEnabledEx)
                .SubscribeSafe(_ => command.Execute(null))
                .AddTo(disposables);

            Observable
                .FromEventPattern(x => command.CanExecuteChanged += x, x => command.CanExecuteChanged -= x)
                .Select(_ => command.CanExecute(null))
                .StartWith(command.CanExecute(null))
                .SubscribeSafe(canExecute => controlButton.IsEnabledEx = canExecute)
                .AddTo(disposables);

            return disposables;
        }

        public IDisposable BindCommandToObject<TEventArgs>(ICommand command, object target, IObservable<object> commandParameter, string eventName)
        {
            throw new NotImplementedException();
        }
    }
}