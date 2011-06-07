using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Persistence;

namespace wpf_player
{
	/// <summary>
	/// Logica di interazione per SearchList.xaml
	/// </summary>    
	public partial class SearchList : UserControl
	{
        private SearchListModel vm = null;
		public SearchList()
		{
			this.InitializeComponent();
			// Inserire il codice richiesto per la creazione dell'oggetto al di sotto di questo punto.
		}        
       
        public void SetDataContext(SearchListModel model)
        {
            vm = model;
            this.DataContext = vm;
        }

        private void ResultsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                DataGrid g = sender as DataGrid;
                vm.OnStreamRequest(new StreamRequestedArgs(g.SelectedItem as KademliaResource));   
            }
        }

        private void start_stream_button_Click(object sender, RoutedEventArgs e)
        {
            if (vm != null)
            {
                object item = this.FindName("ResultsGrid");
                if ((item != null) && (item is DataGrid))
                {
                    DataGrid g = item as DataGrid;
                    vm.OnStreamRequest(new StreamRequestedArgs(g.SelectedItem as KademliaResource));
                }
            }
        }       
	}
}