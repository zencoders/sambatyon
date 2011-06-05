using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Windows.Data;

namespace wpf_player
{
    public class AlwaysExecuteCommand : ICommand
    {
        #region ICommand

        public bool CanExecute(object parameter)
        {
            return true;
        }

        Action<object> _executeDelegate;
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _executeDelegate(parameter);
        }

        public AlwaysExecuteCommand(Action<object> executeDelegate)
        {
            this._executeDelegate = executeDelegate;
        }

        #endregion
    }
    public class CanExecuteCommand : DependencyObject, ICommand
    {
        Action<object> _executeDelegate;
        public event EventHandler CanExecuteChanged;

        public bool CanExecuteAction
        {
            get { return (bool)GetValue(CanExecuteActionProperty); }
            set { SetValue(CanExecuteActionProperty, value); }
        }

        public static readonly DependencyProperty CanExecuteActionProperty =
               DependencyProperty.Register("CanExecuteAction", typeof(bool), typeof(CanExecuteCommand),
                                           new PropertyMetadata(OnCanExecuteActionChanged));

        public CanExecuteCommand(Action<object> executeDelegate) : this(executeDelegate, null) { }

        public CanExecuteCommand(Action<object> executeDelegate, Binding canExecuteActionBinding)
        {
            _executeDelegate = executeDelegate;
            if (canExecuteActionBinding != null) BindingOperations.SetBinding(this, CanExecuteActionProperty, canExecuteActionBinding);
        }

        #region ICommand Members

        public void Execute(object parameter) { _executeDelegate(parameter); }

        public bool CanExecute(object parameter) { bool resp = CanExecuteAction; MessageBox.Show(resp.ToString()); return resp; }

        #endregion

        private void RaiseCanExecuteActionChanged()
        {
            if (CanExecuteChanged != null) CanExecuteChanged(this, new EventArgs());
        }

        static void OnCanExecuteActionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            (dependencyObject as CanExecuteCommand).RaiseCanExecuteActionChanged();
        }
    }
}
