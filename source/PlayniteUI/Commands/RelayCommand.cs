using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayniteUI.Commands
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Predicate<T> canExecute;
        private readonly Action<T> execute;

        public KeyGesture Gesture
        {
            get; set;
        }

        public string GestureText => Gesture?.GetDisplayStringForCulture(CultureInfo.CurrentUICulture);

        public RelayCommand(Action<T> execute)
            : this(execute, null, null)
        {            
        }

        public RelayCommand(Action<T> execute, KeyGesture gesture)
            : this(execute, null, gesture)
        {
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
            : this(execute, canExecute, null)
        {
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute, KeyGesture gesture)
        {
            this.execute = execute;
            this.canExecute = canExecute;
            Gesture = gesture;
        }

        public bool CanExecute(object parameter)
        {
            if (canExecute == null)
            {
                return true;
            }

            return canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
