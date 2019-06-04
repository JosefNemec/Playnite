using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Playnite.SDK
{
    public abstract class RelayCommand : ICommand
    {
        public KeyGesture Gesture
        {
            get; set;
        }

        public string GestureText => Gesture?.GetDisplayStringForCulture(CultureInfo.CurrentUICulture);

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);
    }

    public class RelayCommand<T> : RelayCommand
    {
        private readonly Predicate<T> canExecute;
        private readonly Action<T> execute;

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

        public override bool CanExecute(object parameter)
        {
            if (canExecute == null)
            {
                return true;
            }

            return canExecute((T)parameter);
        }

        public override void Execute(object parameter)
        {
            execute((T)parameter);
        }
    }
}
