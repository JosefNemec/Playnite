using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Playnite.SDK
{
    /// <summary>
    ///
    /// </summary>
    public abstract class RelayCommandBase : ICommand
    {
        /// <summary>
        ///
        /// </summary>
        public KeyGesture Gesture
        {
            get; set;
        }

        /// <summary>
        ///
        /// </summary>
        public string GestureText => Gesture?.GetDisplayStringForCulture(CultureInfo.CurrentUICulture);

        /// <summary>
        ///
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public abstract bool CanExecute(object parameter);

        /// <summary>
        ///
        /// </summary>
        /// <param name="parameter"></param>
        public abstract void Execute(object parameter);
    }

    /// <summary>
    ///
    /// </summary>
    public class RelayCommand : RelayCommandBase
    {
        private readonly Func<bool> canExecute;
        private readonly Action execute;

        /// <summary>
        ///
        /// </summary>
        /// <param name="execute"></param>
        public RelayCommand(Action execute)
            : this(execute, null, null)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="execute"></param>
        /// <param name="gesture"></param>
        public RelayCommand(Action execute, KeyGesture gesture)
            : this(execute, null, gesture)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="execute"></param>
        /// <param name="canExecute"></param>
        public RelayCommand(Action execute, Func<bool> canExecute)
            : this(execute, canExecute, null)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="execute"></param>
        /// <param name="canExecute"></param>
        /// <param name="gesture"></param>
        public RelayCommand(Action execute, Func<bool> canExecute, KeyGesture gesture)
        {
            this.execute = execute;
            this.canExecute = canExecute;
            Gesture = gesture;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public override bool CanExecute(object parameter = null)
        {
            if (canExecute == null)
            {
                return true;
            }

            return canExecute();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="parameter"></param>
        public override void Execute(object parameter = null)
        {
            execute();
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RelayCommand<T> : RelayCommandBase
    {
        private readonly Predicate<T> canExecute;
        private readonly Action<T> execute;

        /// <summary>
        ///
        /// </summary>
        /// <param name="execute"></param>
        public RelayCommand(Action<T> execute)
            : this(execute, null, null)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="execute"></param>
        /// <param name="gesture"></param>
        public RelayCommand(Action<T> execute, KeyGesture gesture)
            : this(execute, null, gesture)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="execute"></param>
        /// <param name="canExecute"></param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
            : this(execute, canExecute, null)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="execute"></param>
        /// <param name="canExecute"></param>
        /// <param name="gesture"></param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute, KeyGesture gesture)
        {
            this.execute = execute;
            this.canExecute = canExecute;
            Gesture = gesture;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public override bool CanExecute(object parameter)
        {
            if (canExecute == null)
            {
                return true;
            }

            return canExecute((T)parameter);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="parameter"></param>
        public override void Execute(object parameter)
        {
            if (parameter is T param)
            {
                execute(param);
            }
            else
            {
                execute(default);
            }
        }
    }
}
