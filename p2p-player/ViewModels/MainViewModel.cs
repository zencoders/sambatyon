using System;
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