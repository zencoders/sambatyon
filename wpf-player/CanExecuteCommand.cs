/*****************************************************************************************
 *  p2p-player
 *  An audio player developed in C# based on a shared base to obtain the music from.
 * 
 *  Copyright (C) 2010-2011 Dario Mazza, Sebastiano Merlino
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as
 *  published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *  
 *  Dario Mazza (dariomzz@gmail.com)
 *  Sebastiano Merlino (etr@pensieroartificiale.com)
 *  Full Source and Documentation available on Google Code Project "p2p-player", 
 *  see <http://code.google.com/p/p2p-player/>
 *
 ******************************************************************************************/

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Windows.Data;

namespace wpf_player
{   
    /// <summary>
    /// Command that execute always without any can execute condition
    /// </summary>
    public class AlwaysExecuteCommand : ICommand
    {
        #region ICommand

        /// <summary>
        /// Can Execute function that always returns true
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>true</returns>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Delegate to execute
        /// </summary>
        Action<object> _executeDelegate;
        /// <summary>
        /// Can Execute state changed event. This event is never called
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Executes the assinged delegate
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            _executeDelegate(parameter);
        }
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="executeDelegate">Delegate to execute</param>
        public AlwaysExecuteCommand(Action<object> executeDelegate)
        {
            this._executeDelegate = executeDelegate;
        }

        #endregion
    }
    /*
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
    }*/
}
