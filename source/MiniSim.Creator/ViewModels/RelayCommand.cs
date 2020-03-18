using System;
using System.Windows.Input;

namespace MiniSim.Creator.ViewModels
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private readonly Action<object> _methodToExecute;
        private readonly Func<object, bool> _canExecuteEvaluator;
        public RelayCommand(Action<object> methodToExecute, Func<object, bool> canExecuteEvaluator)
        {
            this._methodToExecute = methodToExecute;
            this._canExecuteEvaluator = canExecuteEvaluator;
        }
        public RelayCommand(Action<object> methodToExecute)
            : this(methodToExecute, null)
        {
        }
        public bool CanExecute(object parameter)
        {
            if (_canExecuteEvaluator == null)
            {
                return true;
            }
            else
            {
                bool result = _canExecuteEvaluator.Invoke(parameter);
                return result;
            }
        }
        public void Execute(object parameter)
        {
            _methodToExecute.Invoke(parameter);
        }
    }
}
