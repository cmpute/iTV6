﻿using System;
using System.Windows.Input;

namespace iTV6.Mvvm
{
    // From Template10
    using System.Diagnostics;

    /// <summary>
    /// 通过<see cref="Action"/>实现<see cref="ICommand"/>的Command类
    /// </summary>
    /// <remarks> DOCS: https://github.com/Windows-XAML/Template10/wiki/MVVM </remarks>
    public class DelegateCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action execute, Func<bool> canexecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            _execute = execute;
            _canExecute = canexecute ?? (() => true);
        }

        [DebuggerStepThrough]
        public bool CanExecute(object p = null)
        {
            try { return _canExecute(); }
            catch { return false; }
        }

        public void Execute(object p = null)
        {
            if (!CanExecute(p))
                return;
            try { _execute(); }
            catch { Debugger.Break(); }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 通过<see cref="Action{T}"/>实现<see cref="ICommand"/>的Command类
    /// </summary>
    /// <remarks> DOCS: https://github.com/Windows-XAML/Template10/wiki/MVVM </remarks>
    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;
        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<T> execute, Func<T, bool> canexecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            _execute = execute;
            _canExecute = canexecute ?? (e => true);
        }

        [DebuggerStepThrough]
        public bool CanExecute(object p)
        {
            try { return _canExecute(ConvertParameterValue(p)); }
            catch { return false; }
        }

        public void Execute(object p)
        {
            if (!this.CanExecute(p))
                return;
            _execute(ConvertParameterValue(p));
        }

        private static T ConvertParameterValue(object parameter)
        {
            parameter = parameter is T ? parameter : Convert.ChangeType(parameter, typeof(T));
            return (T)parameter;
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
