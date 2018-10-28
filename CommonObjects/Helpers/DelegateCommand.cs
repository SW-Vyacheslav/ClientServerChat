using System;
using System.Windows.Input;

namespace CommonObjects.Helpers
{
    public class DelegateCommand : ICommand
    {
        readonly Action<object> _execute;
        readonly Predicate<object> _can_execute;

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public DelegateCommand(Action<object> execute) : this(execute, null) { }

        public DelegateCommand(Action<object> execute, Predicate<object> can_execute)
        {
            _execute = execute ?? throw new ArgumentNullException("execute");
            _can_execute = can_execute;
        }

        public bool CanExecute(object parameter)
        {
            return _can_execute?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
