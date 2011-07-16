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
using System.ComponentModel;
using System.Windows.Controls;

namespace p2p_player
{
	public class MainViewModel : INotifyPropertyChanged
	{
		public MainViewModel()
		{
			// Insert code required on object creation below this point.
		}
		
		private string viewModelProperty = "Runtime Property Value";
		/// <summary>
		/// Sample ViewModel property; this property is used in the view to display its value using a Binding.
		/// </summary>
		/// <returns></returns>
		public string ViewModelProperty
		{ 
			get
			{
				return this.viewModelProperty;
			}
			set
			{
				this.viewModelProperty = value;
				this.NotifyPropertyChanged("ViewModelProperty");
			}
		}
		
		/// <summary>
		/// Sample ViewModel method; this method is invoked by a Behavior that is associated with it in the View.
		/// </summary>
		public void ViewModelMethod()
		{ 
			if(!this.ViewModelProperty.EndsWith("Updated Value", StringComparison.Ordinal)) 
			{ 
				this.ViewModelProperty = this.ViewModelProperty + " - Updated Value";
			}
		}
		
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(String info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}
		#endregion

	}
}